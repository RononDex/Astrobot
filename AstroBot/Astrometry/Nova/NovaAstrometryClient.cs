using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
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
            Log<DiscordAstroBot>.Info("Login into Astrometry...");

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

            Log<DiscordAstroBot>.Info($"Login into Astrometry successfull. SessionKey: {jsonResult.session}");

            return jsonResult.session;
        }

        /// <summary>
        /// Upload a file for plate solving
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="paramName"></param>
        /// <param name="contentType"></param>
        /// <param name="nvc"></param>
        /// <returns></returns>
        public string HttpUploadFile(
            string url,
            Stream file,
            string fileName,
            string paramName,
            string contentType,
            NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, fileName, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            Stream fileStream = file;
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                return reader2.ReadToEnd();
            }
            catch (Exception)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return null;
        }

        /// <summary>
        /// Uploads a file from an url
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="fileName"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public string UploadFile(string fileUrl, string fileName, string sessionID)
        {
            Log<DiscordAstroBot>.Info($"Submitting a file to astrometry (SessionID: {sessionID}, File: {fileUrl})");

            // Setup json payload
            var json = new { session = sessionID, allow_commercial_use = "n", allow_modifications = "n", publicly_visible = "y" };

            var wc = new WebClient();
            var memstream = new MemoryStream(wc.DownloadData(fileUrl));

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("request-json", JsonConvert.SerializeObject(json));

            // Get answer from server
            string text = HttpUploadFile("http://nova.astrometry.net/api/upload", memstream, fileName, "file", "application/octet-stream", nvc);

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            if (jsonResult.status != "success")
                throw new Exception("Submitting your file to Astrometry failed.");

            Log<DiscordAstroBot>.Info($"Submission was successfull (SessionID: {jsonResult.session}, File: {fileUrl}, SubmissionID: {jsonResult.subid})");

            return jsonResult.subid;
        }

        /// <summary>
        /// Gets the plate solving status for a given submission
        /// </summary>
        /// <param name="submissionID"></param>
        /// <returns></returns>
        public AstrometrySubmissionStatus GetSubmissionStatus(string submissionID)
        {
            Log<DiscordAstroBot>.Info($"Getting submission status from astrometry for submission {submissionID}");

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
        /// Gets determined calibration data from a finished plate solving job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public AstrometrySubmissionResult GetCalibrationFromFinishedJob(string jobID)
        {
            Log<DiscordAstroBot>.Info($"Getting job result from astrometry for job {jobID}");

            var webRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://nova.astrometry.net/api/jobs/{0}/info/", jobID));
            var response = (HttpWebResponse)webRequest.GetResponse();

            // Get answer from server
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic jsonResult = JsonConvert.DeserializeObject(text);
            var result = new AstrometrySubmissionResult();
            var calibrationData = new AstrometrySubmissionCalibrationData();

            calibrationData.Orientation = Convert.ToSingle(jsonResult.calibration.orientation);
            calibrationData.Parity = Convert.ToSingle(jsonResult.calibration.parity);
            calibrationData.PixScale = Convert.ToSingle(jsonResult.calibration.pixscale);
            calibrationData.Radius = Convert.ToSingle(jsonResult.calibration.radius);
            calibrationData.Coordinates = new Objects.RaDecCoordinate(
                rightAscension: Convert.ToSingle(jsonResult.calibration.ra),
                declination: Convert.ToSingle(jsonResult.calibration.dec));
            calibrationData.WCSFileUrl = GetWCSFileUrl(jobID);

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
        public MemoryStream DownlaodAnnotatedImage(string jobID)
        {
            var wc = new WebClient();
            return new MemoryStream(wc.DownloadData(GetAnnotatedImageURL(jobID)));
        }

        /// <summary>
        /// Gets the WCS.fit file for transforming the coordinates of RA/DEC to x,y of the 2D-image
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public string GetWCSFileUrl(string jobID)
        {
            return $"http://nova.astrometry.net/wcs_file/{jobID}";
        }

        /// <summary>
        /// Gets the Url to the annoted image
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public string GetAnnotatedImageURL(string jobID)
        {
            return $"http://nova.astrometry.net/annotated_display/{jobID}";
        }
    }
}