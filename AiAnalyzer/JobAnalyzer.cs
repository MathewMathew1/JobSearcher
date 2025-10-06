using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;

namespace JobSearcher.AiAnalyzer
{
    public class HuggingFaceJobAnalyzerService : IJobAnalyzerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model = "sentence-transformers/all-MiniLM-L6-v2";

        public HuggingFaceJobAnalyzerService(IConfiguration config)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api-inference.huggingface.co/")
            };
            var apiKey = config["Hugging:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Hugging Face API key is not set");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<float> AnalyzeCvAsync(string cvText, string jobDescription)
        {
            var similarities = await GetSimilarityAsync(cvText, new List<string> { jobDescription } );

            var score = similarities.First();

            return score;
        }

        public async Task<float[]> AnalyzeJobDescriptionsAsync(string cvText, List<string> jobDescriptions)
        {
            var similarities = await GetSimilarityAsync(cvText, jobDescriptions);

            return similarities;
        }

        private async Task<float[]> GetSimilarityAsync(string checkedObject, List<string> comparedObjects)
        {
            var requestData = JsonConvert.SerializeObject(new
            {
                inputs = new
                {
                    source_sentence = checkedObject,
                    sentences = comparedObjects.ToArray()
                }
            });
            try
            {
                var content = new StringContent(requestData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"models/{_model}",
                    content
                );
                var responseString = await response.Content.ReadAsStringAsync();


                var scores = JsonConvert.DeserializeObject<float[]>(responseString);

                return scores;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }


    }
}
