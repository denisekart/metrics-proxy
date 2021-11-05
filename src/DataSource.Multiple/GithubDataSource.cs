using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSource.Multiple
{
    public class GithubDataSource : IDataSource
    {
        record GithubResponse(int id);

        private readonly IConfigurationAccessor<GithubDataSource> _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public GithubDataSource(IConfigurationAccessor<GithubDataSource> configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }
        
        public string Name => "GitHub";
        
        public async Task<IEnumerable<Kpi>> Query()
        {
            var accessToken = _configuration.Get<Credentials>(this).AccessToken;
            var client = _clientFactory.CreateClient();

            // GH API: https://docs.github.com/en/rest/reference
            var assignedIssues = await ExecuteApiRequest<GithubResponse[]>(HttpMethod.Get, "/issues?filter=assigned", accessToken, client);
            var createdIssues = await ExecuteApiRequest<GithubResponse[]>(HttpMethod.Get, "/issues?filter=created", accessToken, client);
            var mentionedIssues = await ExecuteApiRequest<GithubResponse[]>(HttpMethod.Get, "/issues?filter=mentioned", accessToken, client);

            return new Kpi[]
            {
                new Kpi("AssignedIssueCount", $"{assignedIssues.Length}"),
                new Kpi("CreatedIssueCount", $"{createdIssues.Length}"),
                new Kpi("MentionedIssueCount", $"{mentionedIssues.Length}"),
            };
        }

        private static async Task<T> ExecuteApiRequest<T>(HttpMethod method, string endpoint, string accessToken, HttpClient client)
        {
            var request = new HttpRequestMessage(method, $"https://api.github.com{endpoint}")
            {
                Headers =
                {
                    Accept = {MediaTypeWithQualityHeaderValue.Parse("application/vnd.github.v3+json")},
                    UserAgent = {ProductInfoHeaderValue.Parse("MetricsProxy")},
                    Authorization = AuthenticationHeaderValue.Parse($"Token {accessToken}")
                }
            };
            var result = await client.SendAsync(request);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<T>();
        }
    }
}
