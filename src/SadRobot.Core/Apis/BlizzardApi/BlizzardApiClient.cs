using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SadRobot.Core.Apis.BlizzardApi.Models;
using SadRobot.Core.Json;

namespace SadRobot.Core.Apis.BlizzardApi
{
    public class BlizzardApiClient
    {
        readonly CancellationToken token;
        readonly HttpClient client;
        readonly JsonSerializerOptions deserializeOptions;
        public BlizzardRegion Region { get; set; }

        public BlizzardApiClient(CancellationToken token)
        {
            this.token = token;
            client = HttpClientFactory.Create(new BlizzardApiAuthHandler());

            deserializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingStrategy()
            };

            deserializeOptions.Converters.Add(new JsonConverterBlizzardDateTimeNullable());
        }

        public async Task<T> GetAsync<T>(string relativeUrl, BlizzardLocaleFlags locale = BlizzardLocaleFlags.EnglishUS, BlizzardNamespace ns = BlizzardNamespace.Dynamic)
        {
            var uri = BlizzardUrlBuilder.GetUrl(Region, relativeUrl, locale, ns);

            var response = await client.GetAsync(uri, token);

#if DEBUG
            var stringValue = await response.Content.ReadAsStringAsync();
#endif

            await using var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<T>(stream, deserializeOptions, token);
        }

        public async Task<string> GetJsonAsync(string relativeUrl, BlizzardLocaleFlags locale = BlizzardLocaleFlags.EnglishUS, BlizzardNamespace ns = BlizzardNamespace.Dynamic)
        {
            var uri = BlizzardUrlBuilder.GetUrl(Region, relativeUrl, locale, ns);
            return await client.GetStringAsync(uri);
        }
    }
}