using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace StudentAID.Model
{
    public class MicRecognition
    {
        public async Task<string> SpeechText(Action<bool> updateMicStatus)
        {
            string speechText = string.Empty;

            try
            {
                var config = SpeechConfig.FromSubscription("347bd1d0b7c84f7696e043b561f40267", "uksouth");
                using var speech = new SpeechRecognizer(config);
                
                updateMicStatus(true);
                var result = await speech.RecognizeOnceAsync();
                updateMicStatus(false);
                
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    speechText = result.Text;
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    await Application.Current.MainPage.DisplayAlert(
                      "Speech could not be recognised",
                      "Please try again",
                      "OK");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    await Application.Current.MainPage.DisplayAlert(
                      $"Speech recognition was cancelled: {cancellation}",
                      "Please try again",
                      "OK");
                }
            }
            catch (Exception e) 
            {
                await Application.Current.MainPage.DisplayAlert(
                      $"Speech recognition was cancelled: {e.Message}",
                      "Please try again",
                      "OK");
                updateMicStatus(false); 
            }
            return speechText;
        }
        }
}
