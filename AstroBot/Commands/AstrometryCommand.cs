using System.Collections.Generic;
using System.Threading.Tasks;
using AstroBot.Astrometry.Nova;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AstroBot.Astrometry;
using System;

namespace AstroBot.Commands
{
    public class AstrometryCommand : AwesomeChatBot.Commands.Command, ISlashCommand, ICommandDescription
    {
        public string Description => "Platesolves and analyses the attached photo";

        public override string Name => "Astrometry";

        public string SlashName => "astrometry";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption { Name = "image", Description = "The astro photo to analyse", Type = SlashOptionType.Attachment, Required = true }
        };

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            // If no attachment provided -> error
            var image = invocation.GetOption<Attachment>("image");
            if (image == null)
            {
                await invocation.RespondAsync("No file attached!").ConfigureAwait(false);
                return;
            }

            var formatter = Globals.ApiWrapper.MessageFormatter;
            var novaAstrometryClient = new NovaAstrometryClient();
            await invocation.RespondAsync("Submitting your image to nova.astrometry.net for platesolving and image analysis ...").ConfigureAwait(false);
            var sessionId = await novaAstrometryClient.LoginAsync().ConfigureAwait(false);

            var submissionId = await novaAstrometryClient.UploadFileAsync(
                await image.GetContentAsync().ConfigureAwait(false),
                image.Name,
                sessionId).ConfigureAwait(false);

            await invocation.RespondAsync($"Submission {submissionId} successful, awaiting analysis results...\r\n(Depending on the image, this might take a few minutes, please be patient...)").ConfigureAwait(false);

            // Wait for the astrometry server to complete the plate solving process
            const int waitDelta = 5000;         // 5s
            const int maxWait = 300 * 1000;     // 300s
            var curWait = 0;
            var jobId = 0;
            var finished = false;
            while (curWait <= maxWait)
            {
                var status = await novaAstrometryClient.GetSubmissionStatusAsync(submissionId).ConfigureAwait(false);

                if (status.JobID != null)
                    jobId = status.JobID.Value;

                if (status.State == AstrometrySubmissionState.JOB_FINISHED)
                {
                    finished = true;
                    break;
                }

                curWait += waitDelta;
                await Task.Delay(waitDelta).ConfigureAwait(false);
            }

            if (!finished)
            {
                await invocation.RespondAsync($"{formatter.Bold(formatter.Underline("WARNING:"))}\r\nAstrometry could not finish the image analysis within {maxWait / 1000 / 60} minutes for submission {formatter.Bold(submissionId)}.Please check the results yourself on the provided submission link:\r\n http://nova.astrometry.net/status/{submissionId}").ConfigureAwait(false);
                return;
            }

            var calibrationData = await novaAstrometryClient.GetCalibrationFromFinishedJobAsync(jobId).ConfigureAwait(false);
            var embeddedMessage = await CreateEmbeddedMessageAsync(invocation, submissionId, calibrationData).ConfigureAwait(false);

            await invocation.RespondAsync(new SendMessage(embeddedMessage)).ConfigureAwait(false);
            var annotatedImage = await novaAstrometryClient.DownloadAnnotatedImageAsync(jobId).ConfigureAwait(false);

            await invocation.RespondAsync(new SendMessage(string.Empty, new List<Attachment> { new SendAttachment
                {
                    Name = $"annotated_{calibrationData.FileName}",
                    Content = annotatedImage
                }
            })).ConfigureAwait(false);

            await invocation.RespondAsync($"Link to astrometry job result: http://nova.astrometry.net/status/{submissionId}").ConfigureAwait(false);
        }

        private static async Task<EmbeddedMessage> CreateEmbeddedMessageAsync(SlashCommandInvocation invocation, string submissionId, AstrometrySubmissionResult calibrationData)
        {
            var objectsInImage = string.Join(", ", calibrationData.ObjectsInfField);

            await invocation.RespondAsync($"Image analysis for submission {submissionId} completed. Here is the result:").ConfigureAwait(false);

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
                Content = string.IsNullOrEmpty(objectsInImage) ? "-" : objectsInImage,
                Inline = false
            });
            return embeddedMessage;
        }
    }
}
