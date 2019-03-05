using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Astrometry.Nova;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AstroBot.Astrometry;
using System.Threading;

namespace AstroBot.Commands
{
    public class Astrometry : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public List<string> Regex => new List<string> {
            @"(can (you )?)?(please )?(analy(s|z)e|plate(-|\s)?solve) this (image|photo)(\?)?",
            @"what can you (find|figure) out (about|on|from|for) this (image|photo)(\?)?",
            @"what (space )?(objects|DSO) (are|do you know) in this (image|photo)(\?)?",
        };

        public string Description => "Platesolves and analyses the attached photo";

        public string[] ExampleCalls => new[] { "analyse this image" };

        public override string Name => "Astrometry";

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return Task.Factory.StartNew(() =>
            {
                // If no attachment provided --> error
                if (receivedMessage.Attachments.Count == 0)
                {
                    receivedMessage.Channel.SendMessageAsync("No file attached!").Wait();
                    return true;
                }

                var formatter = receivedMessage.ApiWrapper.MessageFormatter;
                var novaAstrometryClient = new NovaAstrometryClient();
                receivedMessage.Channel.SendMessageAsync("Submitting your image to nova.astrometry.net for platesolving and image analysis ...").Wait();
                var sessionId = novaAstrometryClient.Login();

                var submissionId = novaAstrometryClient.UploadFile(
                    receivedMessage.Attachments.First().Content,
                    receivedMessage.Attachments.First().Name,
                    sessionId);

                receivedMessage.Channel.SendMessageAsync($"Submission {submissionId} successful, awaiting analysis results...\r\n(Depending on the image, this might take a few minutes, please be patient...)").Wait();

                // Wait for the astrometry server to complete the plate solving process
                var waitDelta = 5000;
                var maxWait = 300 * 1000;
                var curWait = 0;
                var jobId = 0;
                var finished = false;
                while (curWait <= maxWait)
                {
                    var status = novaAstrometryClient.GetSubmissionStatus(submissionId);

                    if (status.JobID != null)
                        jobId = status.JobID.Value;

                    if (status.State == AstrometrySubmissionState.JOB_FINISHED)
                    {
                        finished = true;
                        break;
                    }

                    curWait += waitDelta;
                    Thread.Sleep(waitDelta);
                }

                if (!finished)
                {
                    receivedMessage.Channel.SendMessageAsync($"{formatter.Bold(formatter.Underline("WARNING:"))}\r\nAstrometry could not finish the image analysis within {maxWait / 1000 / 60} minutes for submission {formatter.Bold(submissionId)}.Please check the results yourself on the provided submission link:\r\n http://nova.astrometry.net/status/{submissionId}").Wait();
                    return true;
                }

                var calibrationData = novaAstrometryClient.GetCalibrationFromFinishedJob(jobId);
                var objectsInImage = string.Join(", ", calibrationData.ObjectsInfField);
                var tags = string.Join(", ", calibrationData.Tags);

                receivedMessage.Channel.SendMessageAsync($"Image analysis for submission {submissionId} completed. Here is the result:").Wait();

                var msg = formatter.CodeBlock($"RA:             {calibrationData.CalibrationData.Coordinates.RightAscension}\r\n" +
                                              $"DEC:            {calibrationData.CalibrationData.Coordinates.Declination}\r\n" +
                                              $"Orientation:    up is {calibrationData.CalibrationData.Orientation} deg\r\n" +
                                              $"Radius:         {calibrationData.CalibrationData.Radius} deg\r\n" +
                                              $"PixelScale:     {calibrationData.CalibrationData.PixScale} arcsec/pixel\r\n" +
                                              $"ObjectsInImage: {objectsInImage}", "css");


                receivedMessage.Channel.SendMessageAsync(msg).Wait();
                var annotatedImage = novaAstrometryClient.DownloadAnnotatedImage(jobId);

                receivedMessage.Channel.SendMessageAsync(new SendMessage(string.Empty, new List<Attachment> { new SendAttachment() {
                    Name = $"annotated_{calibrationData.FileName}",
                    Content = annotatedImage
                }})).Wait();

                receivedMessage.Channel.SendMessageAsync($"Link to astrometry job result: http://nova.astrometry.net/status/{submissionId}").Wait();

                return true;
            });
        }
    }
}