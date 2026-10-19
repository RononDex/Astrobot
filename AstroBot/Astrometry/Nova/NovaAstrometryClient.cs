using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AstroBot.Astrometry.Nova
{
    class NovaAstrometryClient
    {
        private static string? _apiKey;

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
        public async Task<string> LoginAsync()
        {
            Log<AstroBotController>.Info("Login into Astrometry...");

            // Setup json payload
            var json = new { apikey = ApiKey };

            // Send the json payload
            var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["request-json"] = JsonConvert.SerializeObject(json) });
            using var response = await Globals.HttpClient.PostAsync("http://nova.astrometry.net/api/login", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic jsonResult = ParseJson(text);
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
        public async Task<string> UploadFileAsync(byte[] file, string fileName, string sessionID)
        {
            Log<AstroBotController>.Info($"Submitting a file to astrometry (SessionID: {sessionID}, File: {fileName})");

            // Get answer from server
            string text = await PostFileAsync(sessionID, file, fileName).ConfigureAwait(false);

            dynamic jsonResult = ParseJson(text);
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
        public async Task<AstrometrySubmissionStatus> GetSubmissionStatusAsync(string submissionID)
        {
            Log<AstroBotController>.Info($"Getting submission status from astrometry for submission {submissionID}");

            var text = await Globals.HttpClient.GetStringAsync($"http://nova.astrometry.net/api/submissions/{submissionID}").ConfigureAwait(false);

            dynamic jsonResult = ParseJson(text);
            var status = new AstrometrySubmissionStatus { SubmissionID = submissionID };
            if (jsonResult.processing_finished.Value != "None")
                status.FinishTime = Convert.ToDateTime(jsonResult.processing_finished.Value);
            if (jsonResult.processing_started.Value != "None")
                status.StartTime = Convert.ToDateTime(jsonResult.processing_started.Value);

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


        private static dynamic ParseJson(string text)
        {
            return JsonConvert.DeserializeObject(text) ?? throw new Exception("Astrometry API returned an empty response");
        }

        /// <summary>
        /// Posts the file to the upload endpoint, returns the raw response
        /// </summary>
        private static async Task<string> PostFileAsync(string sessionID, byte[] file, string fileName)
        {
            var url = "http://nova.astrometry.net/api/upload";

            // Setup json payload
            var json = new { session = sessionID, allow_commercial_use = "n", allow_modifications = "n", publicly_visible = "y" };

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(file, 0, file.Length), "file", fileName);
            content.Add(new StringContent(JsonConvert.SerializeObject(json)), "request-json");

            using var response = await Globals.HttpClient.PostAsync(url, content).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets determined calibration data from a finished plate solving job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public async Task<AstrometrySubmissionResult> GetCalibrationFromFinishedJobAsync(int jobID)
        {
            Log<AstroBotController>.Info($"Getting job result from astrometry for job {jobID}");

            var text = await Globals.HttpClient.GetStringAsync($"http://nova.astrometry.net/api/jobs/{jobID}/info/").ConfigureAwait(false);

            dynamic jsonResult = ParseJson(text);
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

            var result = new AstrometrySubmissionResult
            {
                CalibrationData = calibrationData,
                JobID = jobID,
                FileName = jsonResult.original_filename
            };

            for (var i = 0; i < jsonResult.machine_tags.Count; i++)
                result.MachineTags.Add(Convert.ToString(jsonResult.machine_tags[i]));

            for (var i = 0; i < jsonResult.objects_in_field.Count; i++)
                result.ObjectsInfField.Add(Convert.ToString(jsonResult.objects_in_field[i]));

            for (var i = 0; i < jsonResult.tags.Count; i++)
                result.Tags.Add(Convert.ToString(jsonResult.tags[i]));

            return result;
        }

        /// <summary>
        /// Gets the annoted image that belongs to the finished plate solving job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public Task<byte[]> DownloadAnnotatedImageAsync(int jobID)
        {
            return Globals.HttpClient.GetByteArrayAsync(GetAnnotatedImageURL(jobID));
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
            return $"http://nova.astrometry.net/annotated_full/{jobID}";
        }
    }
}
