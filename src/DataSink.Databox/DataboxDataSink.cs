using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSink.Databox
{
    public class Credentials
    {
        public string Token { get; set; }
    }

    public class DataboxDataSink : IDataSink
    {
        record GenericApiResponse(string status, string message);

        private readonly IConfigurationAccessor<DataboxDataSink> _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public DataboxDataSink(IConfigurationAccessor<DataboxDataSink> configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        public string Name => "Databox";

        public async Task Report(IEnumerable<Kpi> items)
        {
            var token = _configuration.Get<Credentials>(this).Token;
            var client = _clientFactory.CreateClient();

            // Databox API: https://developers.databox.com/send-data/
            var serializedData = GenerateRequestBody(items);

            var request = GenerateRequest(token, serializedData);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseModel = await response.Content.ReadFromJsonAsync<GenericApiResponse>();

            if (!responseModel.status.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Databox data sink failed to process the data with error: {responseModel.message}");
            }
        }

        private static HttpRequestMessage GenerateRequest(string token, string serializedData)
        {
            return new(HttpMethod.Post, "https://push.databox.com")
            {
                Headers =
                {
                    UserAgent = {ProductInfoHeaderValue.Parse("MetricsProxy")},
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/vnd.databox.v2+json") },
                    Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{token}:")))
                },
                Content = new StringContent(serializedData, Encoding.UTF8, "application/json")
            };
        }

        private static string GenerateRequestBody(IEnumerable<Kpi> items)
        {
            return JsonSerializer.Serialize(new
            {
                data = items.Select(x => new Dictionary<string, object>
                {
                    {$"${x.Source}{x.Key}", decimal.TryParse(x.UnitOrValue, out var d)?d:0m},
                    {"date", $"{x.CreatedOn:O}"}
                })
            }, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
