using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Astrometry.Nova;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AstroBot.Astrometry;
using System;

namespace AstroBot.Commands
{
    public class AstrometryCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public List<string> Regex => new List<string> {
            @"(can (you )?)?(please )?(analy(s|z)e|plate(-|\s)?solve) this (image|photo)(\?)?",
            @"what can you (find|figure) out (about|on|from|for) this (image|photo)(\?)?",
            @"what (space )?(objects|DSO) (are|do you know) in this (image|photo)(\?)?",
        };

        public string Description => "Platesolves and analyses the attached photo";

        public string[] ExampleCalls => new[] { "analyse this image" };

        public override string Name => "Astrometry";

        public async Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            // If no attachment provided -> error
            if (receivedMessage.Attachments.Count == 0)
            {
                await receivedMessage.Channel.SendMessageAsync("No file attached!");
                return true;
            }

            var formatter = receivedMessage.ApiWrapper.MessageFormatter;
            var novaAstrometryClient = new NovaAstrometryClient();
            await receivedMessage.Channel.SendMessageAsync("Submitting your image to nova.astrometry.net for platesolving and image analysis ...");
            var sessionId = novaAstrometryClient.Login();

            var submissionId = novaAstrometryClient.UploadFile(
                receivedMessage.Attachments.First().Content,
                receivedMessage.Attachments.First().Name,
                sessionId);

            await receivedMessage.Channel.SendMessageAsync($"Submission {submissionId} successful, awaiting analysis results...\r\n(Depending on the image, this might take a few minutes, please be patient...)");

            // Wait for the astrometry server to complete the plate solving process
            const int waitDelta = 5000;         // 5s
            const int maxWait = 300 * 1000;     // 300s
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
                await Task.Delay(waitDelta);
            }

            if (!finished)
            {
                await receivedMessage.Channel.SendMessageAsync($"{formatter.Bold(formatter.Underline("WARNING:"))}\r\nAstrometry could not finish the image analysis within {maxWait / 1000 / 60} minutes for submission {formatter.Bold(submissionId)}.Please check the results yourself on the provided submission link:\r\n http://nova.astrometry.net/status/{submissionId}");
                return true;
            }

            var calibrationData = novaAstrometryClient.GetCalibrationFromFinishedJob(jobId);
            var embeddedMessage = await CreateEmbeddedMessage(receivedMessage, submissionId, calibrationData);

            await receivedMessage.Channel.SendMessageAsync(embeddedMessage);
            var annotatedImage = novaAstrometryClient.DownloadAnnotatedImage(jobId);

            await receivedMessage.Channel.SendMessageAsync(new SendMessage(string.Empty, new List<Attachment> { new SendAttachment
                {
                    Name = $"annotated_{calibrationData.FileName}",
                    Content = annotatedImage
                }
            }));

            await receivedMessage.Channel.SendMessageAsync($"Link to astrometry job result: http://nova.astrometry.net/status/{submissionId}");

            return true;
        }

        private static async Task<EmbeddedMessage> CreateEmbeddedMessage(ReceivedMessage receivedMessage, string submissionId, AstrometrySubmissionResult calibrationData)
        {
            var objectsInImage = string.Join(", ", calibrationData.ObjectsInfField);
            var tags = string.Join(", ", calibrationData.Tags);

            await receivedMessage.Channel.SendMessageAsync($"Image analysis for submission {submissionId} completed. Here is the result:");

            var embeddedMessage = new EmbeddedMessage
            {
                Title = "Astrometry.Net plate-solving result",
            };

            embeddedMessage.Fields.Add(new EmbeddedMessageField
            {
                Name = "Coordinates at center of image:",
                Content = $"RA: {Math.Round(calibrationData.CalibrationData.Coordinates.RightAscension, 5)}\r\nDEC: {Math.Round(calibrationData.CalibrationData.Coordinates.Declination, 5)}\r\nAngle: {Math.Round(calibrationData.CalibrationData.Orientation, 3)}°",
                Inline = true
            });

            embeddedMessage.Fields.Add(new EmbeddedMessageField
            {
                Name = "Angular size of image:",
                Content = $"{Math.Round(calibrationData.CalibrationData.Radius * 2, 5)}°",
                Inline = true
            });

            embeddedMessage.Fields.Add(new EmbeddedMessageField
            {
                Name = "Pixel Scale:",
                Content = $"{calibrationData.CalibrationData.PixScale} arcsec/pixel",
                Inline = true
            });

            embeddedMessage.Fields.Add(new EmbeddedMessageField
            {
                Name = "Some objects found in image:",
                Content = objectsInImage,
                Inline = false
            });
            return embeddedMessage;
        }
    }
}