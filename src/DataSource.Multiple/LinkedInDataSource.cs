using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSource.Multiple
{
    /// <summary>
    /// INACTIVE: This data source is not working due to missing permission from LinkedIn to use the client credential flow
    /// Moving on....
    ///
    /// Remove the [Obsolete] attribute to enable this data source
    /// </summary>
    [Obsolete]
    class LinkedInDataSource : IDataSource
    {
        record LinkedInAuthReponse(string access_token);

        private readonly IConfigurationAccessor<LinkedInDataSource> _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public LinkedInDataSource(IConfigurationAccessor<LinkedInDataSource> configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }
        public string Name => "LinkedIn";
        public async Task<IEnumerable<Kpi>> Query()
        {
            var accessToken = await ExchangeClientCredentialsForAccessToken();
            
            throw new NotImplementedException();
        }

        private async Task<string> ExchangeClientCredentialsForAccessToken()
        {
            var credentials = _configuration.Get<Credentials>(this);
            var client = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
            {
                Headers =
                {
                    Accept = {MediaTypeWithQualityHeaderValue.Parse("application/json")},
                    UserAgent = {ProductInfoHeaderValue.Parse("MetricsProxy")}
                },
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", credentials.ClientId},
                    {"client_secret", credentials.ClientSecret},
                })
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseModel = await response.Content.ReadFromJsonAsync<LinkedInAuthReponse>();
            return responseModel.access_token;
        }
    }
}
