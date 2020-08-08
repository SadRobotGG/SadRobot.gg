using SadRobot.Core.Apis.BlizzardApi;
using SadRobot.Core.Apis.BlizzardApi.Models;
using Xunit;

namespace SadRobot.Tests
{
    public class BlizzardUrlBuilderTests
    {
        [Fact]
        public void GetRegionCode()
        {
            Assert.Equal("cn", BlizzardUrlBuilder.GetRegionCode(BlizzardRegion.China));
            Assert.Equal("eu", BlizzardUrlBuilder.GetRegionCode(BlizzardRegion.Europe));
            Assert.Equal("kr", BlizzardUrlBuilder.GetRegionCode(BlizzardRegion.Korea));
            Assert.Equal("us", BlizzardUrlBuilder.GetRegionCode(BlizzardRegion.NorthAmerica));
            Assert.Equal("tw", BlizzardUrlBuilder.GetRegionCode(BlizzardRegion.Taiwan));
        }

        [Fact]
        public void GetRegionHostName()
        {
            Assert.Equal("gateway.battlenet.com.cn", BlizzardUrlBuilder.GetRegionHostname(BlizzardRegion.China));
            Assert.Equal("eu.api.blizzard.com", BlizzardUrlBuilder.GetRegionHostname(BlizzardRegion.Europe));
            Assert.Equal("kr.api.blizzard.com", BlizzardUrlBuilder.GetRegionHostname(BlizzardRegion.Korea));
            Assert.Equal("us.api.blizzard.com", BlizzardUrlBuilder.GetRegionHostname(BlizzardRegion.NorthAmerica));
            Assert.Equal("tw.api.blizzard.com", BlizzardUrlBuilder.GetRegionHostname(BlizzardRegion.Taiwan));
        }

        [Fact]
        public void GetNamespaceString()
        {
            Assert.Equal("dynamic-us", BlizzardUrlBuilder.GetNamespaceString( BlizzardNamespace.Dynamic, BlizzardRegion.NorthAmerica));
            Assert.Equal("static-us", BlizzardUrlBuilder.GetNamespaceString( BlizzardNamespace.Static, BlizzardRegion.NorthAmerica));
            Assert.Equal("dynamic-eu", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Dynamic, BlizzardRegion.Europe));
            Assert.Equal("static-eu", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Static, BlizzardRegion.Europe));
            Assert.Equal("dynamic-tw", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Dynamic, BlizzardRegion.Taiwan));
            Assert.Equal("static-tw", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Static, BlizzardRegion.Taiwan));
            Assert.Equal("dynamic-kr", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Dynamic, BlizzardRegion.Korea));
            Assert.Equal("static-kr", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Static, BlizzardRegion.Korea));
            Assert.Equal("dynamic-cn", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Dynamic, BlizzardRegion.China));
            Assert.Equal("static-cn", BlizzardUrlBuilder.GetNamespaceString(BlizzardNamespace.Static, BlizzardRegion.China));
        }

        [Fact]
        public void GetLocaleString()
        {
            // NA
            Assert.Equal("en_US", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.EnglishUS));
            Assert.Equal("es_MX", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.SpanishMX));
            Assert.Equal("pt_BR", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.PortugeseBR));

            // Europe
            Assert.Equal("en_GB", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.EnglishGB));
            Assert.Equal("es_ES", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.SpanishES));
            Assert.Equal("fr_FR", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.French));
            Assert.Equal("ru_RU", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.Russian));
            Assert.Equal("de_DE", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.German));
            Assert.Equal("pt_PT", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.PortugesePT));
            Assert.Equal("it_IT", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.Italian));
            
            // Korea
            Assert.Equal("kr_KR", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.Korean));
            
            // Taiwan
            Assert.Equal("zh_TW", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.ChineseTW));
            
            // China
            Assert.Equal("zh_CN", BlizzardUrlBuilder.GetLocaleString(BlizzardLocaleFlags.ChineseCN));
        }

        [Fact]
        public void GetUrl()
        {
            Assert.Equal("https://us.api.blizzard.com/data/wow/keystone-affix?namespace=dynamic-us&locale=en_US",
                BlizzardUrlBuilder.GetUrl(BlizzardRegion.NorthAmerica, "/data/wow/keystone-affix").ToString());

            Assert.Equal("https://us.api.blizzard.com/data/wow/keystone-affix?namespace=static-us&locale=es_MX",
                BlizzardUrlBuilder.GetUrl(BlizzardRegion.NorthAmerica, "/data/wow/keystone-affix", BlizzardLocaleFlags.SpanishMX, BlizzardNamespace.Static).ToString());
        }
    }
}
