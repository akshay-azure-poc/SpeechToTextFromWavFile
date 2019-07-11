using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SpeechToTextFromWavFilePOC
{
    class Program
    {
        static void Main(string[] args)
        {
            //pass Pyron wav file with fully qualified path
            string wavfilename = "";
            SpeechToTextFromWavFileInput(wavfilename).Wait();
            Console.WriteLine("Please press a key to continue.");
            Console.ReadLine();
        }
        
        /// <summary>
        /// This methods takes the wav file name as input to convert into text and writes output to console log
        /// </summary>
        /// <param name="wavfileName"></param>
        /// <returns></returns>
        public static async Task SpeechToTextFromWavFileInput(string wavfileName)
        {
            var taskCompleteionSource = new TaskCompletionSource<int>();

            // Creates an instance of a speech translation config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechTranslationConfig.FromSubscription("YourSubscriptionKey", "YourServiceRegion");
            
            var transcriptionStringBuilder = new StringBuilder();

            using (var audioInput = AudioConfig.FromWavFileInput(wavfileName))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    try
                    {
                        // Subscribes to events.  
                        recognizer.Recognizing += (sender, eventargs) =>
                        {
                            Console.WriteLine(eventargs.Result.Text); //view text as it comes in  
                        };

                        recognizer.Recognized += (sender, eventargs) =>
                        {
                            if (eventargs.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                transcriptionStringBuilder.Append(eventargs.Result.Text);
                            }
                            else if (eventargs.Result.Reason == ResultReason.NoMatch)
                            {
                                //TODO: Handle not recognized value  
                            }
                        };

                        recognizer.Canceled += (sender, eventargs) =>
                        {
                            if (eventargs.Reason == CancellationReason.Error)
                            {
                                //TODO: Handle error  
                            }

                            if (eventargs.Reason == CancellationReason.EndOfStream)
                            {
                                Console.WriteLine(transcriptionStringBuilder.ToString());
                            }

                            taskCompleteionSource.TrySetResult(0);
                        };

                        recognizer.SessionStarted += (sender, eventargs) =>
                        {
                            Console.WriteLine(transcriptionStringBuilder.ToString());
                            //Started recognition session  
                        };

                        recognizer.SessionStopped += (sender, eventargs) =>
                        {
                            //Ended recognition session  
                            taskCompleteionSource.TrySetResult(0);
                        };
                        
                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.  
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(true);

                        // Waits for completion.  
                        // Use Task.WaitAny to keep the task rooted.  
                        Task.WaitAny(new[] { taskCompleteionSource.Task });

                        // Stops recognition.  
                        await recognizer.StopContinuousRecognitionAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
