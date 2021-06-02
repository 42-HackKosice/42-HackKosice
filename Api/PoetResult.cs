using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using Azure;
using Azure.AI.TextAnalytics;

namespace BlazorApp.Api
{
    public static class PoetResult
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("d1764c4fa6514255b4b6fc39c9c04e98");
        private static readonly Uri endpoint = new Uri("https://poetrycheck.cognitiveservices.azure.com/");
        [FunctionName("PoetResult")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var client = new TextAnalyticsClient(endpoint, credentials);

            string text = req.Query["text"];
            string output = "";
            Console.WriteLine(text);
            Console.WriteLine(text.Replace("\n", " "));
            if (text != null)
            {
                text = text.Replace("\n"," ").Replace(",,",",");
                DocumentSentiment documentSentiment = client.AnalyzeSentiment(text);
                output += documentSentiment.Sentiment + ";";

                var topic = client.ExtractKeyPhrases(text);
                foreach (string phrase in topic.Value)
                    output += $"{phrase},";
                output.Remove(output.Length-1);
                output += ";";

                text = text.Replace(".","").Replace(",","").Replace("!", "").Replace("?", "").Replace("'", "");
                text = text.ToLower();
                Dictionary<string, int> roznorodostSlov = new Dictionary<string, int>();
                foreach(string item in text.Split(" "))
                {
                    if (roznorodostSlov.ContainsKey(item))
                        roznorodostSlov[item] += 1;
                    else
                        roznorodostSlov.Add(item, 1);
                }
                output += roznorodostSlov.Count / (text.Split(" ").Length * 1.0) * 100.0 + "%;";

                text = text.Replace(" ", "");
                Dictionary<char, int> kvalita = new Dictionary<char, int>();
                for (int i = 0; i < text.Length; i++)
                {
                    if(!kvalita.ContainsKey(text[i]))
                        kvalita.Add(text[i],1);
                    else
                        kvalita[text[i]]+=1;
                }
                output += kvalita.Count/26.0*100.0+"%";
            }

            return new OkObjectResult(output);
        }
    }
}
