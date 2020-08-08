using System.Net.Http;

namespace SadRobot.Core.Apis
{
    /// <summary>
    /// Exposes a singleton <see cref="HttpClient"/> for re-use
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// A default, singleton, thread-safe <see cref="HttpClient"/>
        /// </summary>
        public static HttpClient Client { get; } = HttpClientFactory.Create();
    }
}
