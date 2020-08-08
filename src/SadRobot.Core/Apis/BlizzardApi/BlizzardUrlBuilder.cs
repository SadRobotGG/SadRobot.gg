using System;
using SadRobot.Core.Apis.BlizzardApi.Models;

namespace SadRobot.Core.Apis.BlizzardApi
{
    public static class BlizzardUrlBuilder
    {
        public static Uri GetUrl(BlizzardRegion region, string relativeUrl, BlizzardLocaleFlags locale = BlizzardLocaleFlags.EnglishUS, BlizzardNamespace ns = BlizzardNamespace.Dynamic)
        {
            var host = GetRegionHostname(region);
            var builder = new UriBuilder("https", host, -1, relativeUrl);

            if (builder.Query.Length > 0) builder.Query += "&";

            builder.Query += "namespace=" + GetNamespaceString(ns, region);
            
            if (locale != BlizzardLocaleFlags.All && locale != BlizzardLocaleFlags.AllWowLocales)
            {
                builder.Query += "&locale=" + GetLocaleString(locale);
            }

            return builder.Uri;
        }

        internal static string GetLocaleString(BlizzardLocaleFlags locale)
        {
            return BlizzardLocales.Get(locale).Name;
        }

        internal static string GetNamespaceString(BlizzardNamespace ns, BlizzardRegion region)
        {
            return ns.ToString().ToLowerInvariant() + "-" + GetRegionCode(region);
        }

        public static string GetRegionCode(BlizzardRegion region)
        {
            switch (region)
            {
                case BlizzardRegion.NorthAmerica:
                    return "us";

                case BlizzardRegion.Europe:
                    return "eu";

                case BlizzardRegion.Korea:
                    return "kr";

                case BlizzardRegion.Taiwan:
                    return "tw";

                case BlizzardRegion.China:
                    return "cn";

                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }

        public static string GetRegionHostname(BlizzardRegion region)
        {
            switch (region)
            {
                case BlizzardRegion.NorthAmerica:
                case BlizzardRegion.Europe:
                case BlizzardRegion.Korea:
                case BlizzardRegion.Taiwan:
                    return GetRegionCode(region) + ".api.blizzard.com";

                case BlizzardRegion.China:
                    return "gateway.battlenet.com.cn";

                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }
    }
}