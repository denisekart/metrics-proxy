namespace DataSource.Multiple
{
    /// <summary>
    /// Common credentials model used for various data source options
    /// </summary>
    public class Credentials
    {
        public string AppId { get; init; }
        public string AppSecret { get; init; }
        public string AccessToken { get; init; }
        public string ClientId { get; init; }
        public string ClientSecret { get; init; }
    }
}