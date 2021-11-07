namespace MetricsProxy.Contracts
{
    /// <summary>
    /// A service with a unique name
    /// </summary>
    public interface INamedService
    {
        /// <summary>
        /// The name of the service
        /// </summary>
        string Name { get; }
    }
}