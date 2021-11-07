using MetricsProxy.Contracts;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DataSource.Multiple
{
    public class FacebookDataSource : IDataSource
    {
        public record AuthResponse(string access_token);
        public record DataResponse(object[] data);

        private readonly IConfigurationAccessor<FacebookDataSource> _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public FacebookDataSource(IConfigurationAccessor<FacebookDataSource> configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        public string Name => "Facebook";

        public async Task<IEnumerable<Kpi>> Query()
        {
            var appId = _configuration.Get<Credentials>(this).AppId;
            var client = _clientFactory.CreateClient();
            var accessToken = await ExchangeClientCredentialsForAccessToken(client);

            //Graph API: https://developers.facebook.com/docs/graph-api/reference/v12.0/app/
            var roles = await ExecuteApiRequest<DataResponse>(HttpMethod.Get, $"/v12.0/{appId}/roles", accessToken, client);
            var subscriptions = await ExecuteApiRequest<DataResponse>(HttpMethod.Get, $"/v12.0/{appId}/subscriptions", accessToken, client);
            var assets = await ExecuteApiRequest<DataResponse>(HttpMethod.Get, $"/v12.0/{appId}/appassets", accessToken, client);

            return new Kpi[]
            {
                new Kpi("RoleCount", $"{roles.data.Length}"),
                new Kpi("SubscriptionCount", $"{subscriptions.data.Length}"),
                new Kpi("AssetCount", $"{assets.data.Length}"),
            };
        }

        private static async Task<T> ExecuteApiRequest<T>(HttpMethod method, string endpoint, string accessToken, HttpClient client)
        {
            var request = new HttpRequestMessage(method, $"https://graph.facebook.com{endpoint}?access_token={accessToken}")
            {
                Headers =
                {
                    Accept = {MediaTypeWithQualityHeaderValue.Parse("application/json")},
                    UserAgent = {ProductInfoHeaderValue.Parse("MetricsProxy")}
                }
            };
            var result = await client.SendAsync(request);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<T>();
        }

        private async Task<string> ExchangeClientCredentialsForAccessToken(HttpClient client)
        {
            var credentials = _configuration.Get<Credentials>(this);
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://graph.facebook.com/oauth/access_token?client_id={credentials.AppId}&client_secret={credentials.AppSecret}&grant_type=client_credentials")
            {
                Headers =
                {
                    Accept = {MediaTypeWithQualityHeaderValue.Parse("application/json")},
                    UserAgent = {ProductInfoHeaderValue.Parse("MetricsProxy")}
                }
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseModel = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return responseModel.access_token;
        }
    }
}