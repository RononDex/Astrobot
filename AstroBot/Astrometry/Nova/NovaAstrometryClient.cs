using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace AstroBot.Astrometry.Nova
{
    class NovaAstrometryClient
    {
        private static string _apiKey;

        /// <summary>
        /// Cache the api key from the file to reduce I/O
        /// </summary>
        /// <value></value>
        private static string ApiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _apiKey = File.ReadAllText(Globals.BotSettings.NovaAstrometryApiKeyPath);
                }

                return _apiKey;
            }
        }

        /// <summary>
        /// Logs into the Astrometry API using a private token, gets a sessionID in return
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string Login()
        {
            Log<AstroBotController>.Info("Login into Astrometry...");

            // Setup json payload
            var json = new { apikey = ApiKey };

            var webRequest = (HttpWebRequest)WebRequest.Create("http://nova.astrometry.net/api/login");
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";

            // Send the json payload
            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                streamWriter.Write(string.Format("request-json={0}", WebUtility.UrlEncode(JsonConvert.SerializeObject(json))));
                streamWriter.Flush();
            }

            var response = (HttpWebResponse)webRequest.GetResponse();

            // Get answer from server
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            if (jsonResult.status != "success")
                throw new Exception("Login was refused by Astrometry API");

            Log<AstroBotController>.Info($"Login into Astrometry successfull. SessionKey: {jsonResult.session}");

            return jsonResult.session;
        }

        /// <summary>
        /// Uploads a file from a byte array
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="fileName"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string UploadFile(byte[] file, string fileName, string sessionID)
        {
            Log<AstroBotController>.Info($"Submitting a file to astrometry (SessionID: {sessionID}, File: {fileName})");

            // Get answer from server
            string text = UploadFile(sessionID, file, fileName);

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            if (jsonResult.status != "success")
                throw new Exception("Submission of your file to Astrometry failed.");

            Log<AstroBotController>.Info($"Submission was successfull (SessionID: {jsonResult.session}, File: {fileName}, SubmissionID: {jsonResult.subid})");

            return jsonResult.subid;
        }

        /// <summary>
        /// Gets the plate solving status for a given submission
        /// </summary>
        /// <param name="submissionID"></param>
        /// <returns></returns>
        public AstrometrySubmissionStatus GetSubmissionStatus(string submissionID)
        {
            Log<AstroBotController>.Info($"Getting submission status from astrometry for submission {submissionID}");

            var webRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://nova.astrometry.net/api/submissions/{0}", submissionID));
            var response = (HttpWebResponse)webRequest.GetResponse();

            // Get answer from server
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            var status = new AstrometrySubmissionStatus();
            if (jsonResult.processing_finished.Value != "None")
                status.FinishTime = Convert.ToDateTime(jsonResult.processing_finished.Value);
            if (jsonResult.processing_started.Value != "None")
                status.StartTime = Convert.ToDateTime(jsonResult.processing_started.Value);

            status.SubmissionID = submissionID;

            if (jsonResult.jobs.Count > 0 && jsonResult.jobs[0] != null)
                status.JobID = Convert.ToInt32(jsonResult.jobs[0]);

            if (status.JobID == null)
                status.State = AstrometrySubmissionState.CREATED;
            else if (jsonResult.job_calibrations.Count == 0)
                status.State = AstrometrySubmissionState.JOB_STARTED;
            else
                status.State = AstrometrySubmissionState.JOB_FINISHED;

            return status;
        }


        /// <summary>
        /// Query SIMBAD with the given TAP query
        /// </summary>
        /// <param name="simbadTAPQuery"></param>
        /// <returns></returns>
        public string UploadFile(string sessionID, byte[] file, string fileName)
        {
            var client = new HttpClient();
            var url = "http://nova.astrometry.net/api/upload";

            // Setup json payload
            var json = new { session = sessionID, allow_commercial_use = "n", allow_modifications = "n", publicly_visible = "y" };

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(file, 0, file.Length), "file", fileName);
            content.Add(new StringContent(JsonConvert.SerializeObject(json)), "request-json");

            var response = client.PostAsync(url, content).Result;
            var text = response.Content.ReadAsStringAsync().Result;

            return text;
        }

        /// <summary>
        /// Gets determined calibration data from a finished plate solving job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public AstrometrySubmissionResult GetCalibrationFromFinishedJob(int jobID)
        {
            Log<AstroBotController>.Info($"Getting job result from astrometry for job {jobID}");

            var webRequest = (HttpWebRequest)WebRequest.Create($"http://nova.astrometry.net/api/jobs/{jobID}/info/");
            var response = (HttpWebResponse)webRequest.GetResponse();

            // Get answer from server
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            var result = new AstrometrySubmissionResult();
            var calibrationData = new AstrometrySubmissionCalibrationData
            {
                Orientation = Convert.ToSingle(jsonResult.calibration.orientation),
                Parity = Convert.ToSingle(jsonResult.calibration.parity),
                PixScale = Convert.ToSingle(jsonResult.calibration.pixscale),
                Radius = Convert.ToSingle(jsonResult.calibration.radius),
                Coordinates = new Objects.RaDecCoordinate(
                    rightAscension: Convert.ToSingle(jsonResult.calibration.ra),
                    declination: Convert.ToSingle(jsonResult.calibration.dec)),
                WCSFileUrl = GetWCSFileUrl(jobID)
            };

            result.CalibrationData = calibrationData;
            result.JobID = jobID;

            result.FileName = jsonResult.original_filename;

            for (var i = 0; i < jsonResult.machine_tags.Count; i++)
                result.MachineTags.Add(Convert.ToString(jsonResult.machine_tags[i]));

            for (var i = 0; i < jsonResult.machine_tags.Count; i++)
                result.ObjectsInfField.Add(Convert.ToString(jsonResult.objects_in_field[i]));

            for (var i = 0; i < jsonResult.machine_tags.Count; i++)
                result.Tags.Add(Convert.ToString(jsonResult.tags[i]));

            return result;
        }

        /// <summary>
        /// Gets the annoted image that belongs to the finished plate solving job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public byte[] DownloadAnnotatedImage(int jobID)
        {
            var wc = new WebClient();
            return wc.DownloadData(GetAnnotatedImageURL(jobID));
        }

        /// <summary>
        /// Gets the WCS.fit file for transforming the coordinates of RA/DEC to x,y of the 2D-image
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public string GetWCSFileUrl(int jobID)
        {
            return $"http://nova.astrometry.net/wcs_file/{jobID}";
        }

        /// <summary>
        /// Gets the Url to the annoted image
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public string GetAnnotatedImageURL(int jobID)
        {
            return $"http://nova.astrometry.net/annotated_display/{jobID}";
        }
    }
}
