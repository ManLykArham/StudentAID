using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StudentAID.Services
{
    internal class TextExtractionService
    {
        private readonly ComputerVisionClient _client;

        public TextExtractionService(string endpoint, string apiKey)
        {
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> ExtractTextFromImageAsync(Stream imageStream)
        {
            try
            {
                // Ensure the stream is ready for reading
                if (imageStream.CanSeek)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);
                }

                var textHeaders = await _client.ReadInStreamAsync(imageStream);
                string operationLocation = textHeaders.OperationLocation;

                // Retrieve the URL where results are stored
                string operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

                // Wait for the read operation to complete
                ReadOperationResult results;
                do
                {
                    results = await _client.GetReadResultAsync(Guid.Parse(operationId));
                    await Task.Delay(1000);  // Polling delay
                }
                while ((results.Status == OperationStatusCodes.Running) ||
                       (results.Status == OperationStatusCodes.NotStarted));

                // Extract text
                var extractedText = "";
                if (results.Status == OperationStatusCodes.Succeeded)
                {
                    foreach (var page in results.AnalyzeResult.ReadResults)
                    {
                        foreach (var line in page.Lines)
                        {
                            extractedText += line.Text + "\n";
                        }
                    }
                }
                return extractedText;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error extracting text: {e.Message}");
                return null;
            }
        }
    }
}
