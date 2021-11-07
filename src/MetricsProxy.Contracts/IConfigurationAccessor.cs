namespace MetricsProxy.Contracts
{
    /// <summary>
    /// A service used to access configuration
    /// </summary>
    /// <typeparam name="TService">The type of service that will be used to access configuration</typeparam>
    public interface IConfigurationAccessor<TService> where TService : INamedService
    {
        /// <summary>
        /// Returns an Options instance based on the service and type of object requested
        /// </summary>
        /// <typeparam name="T">The type of object requested</typeparam>
        /// <param name="instance">The service instance</param>
        /// <param name="path">The path relative to the service configuration root</param>
        T Get<T>(TService instance, string path = null) where T : class, new();
    }
}
