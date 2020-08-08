using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SadRobot.Core.Apis.BlizzardApi.Models
{
    public static class BlizzardLocales
    {
        public static readonly IList<BlizzardLocale> Locales = new List<BlizzardLocale>
        {
            // Americas
            new BlizzardLocale("en_US", BlizzardLocaleFlags.EnglishUS),
            new BlizzardLocale("es_MX", BlizzardLocaleFlags.SpanishMX),
            new BlizzardLocale("pt_BR", BlizzardLocaleFlags.PortugeseBR),

            // Europe
            new BlizzardLocale("en_GB", BlizzardLocaleFlags.EnglishGB),
            new BlizzardLocale("es_ES", BlizzardLocaleFlags.SpanishES),
            new BlizzardLocale("fr_FR", BlizzardLocaleFlags.French),
            new BlizzardLocale("ru_RU", BlizzardLocaleFlags.Russian),
            new BlizzardLocale("de_DE", BlizzardLocaleFlags.German),
            new BlizzardLocale("pt_PT", BlizzardLocaleFlags.PortugesePT),
            new BlizzardLocale("it_IT", BlizzardLocaleFlags.Italian),

            // Korea
            new BlizzardLocale("kr_KR", BlizzardLocaleFlags.Korean),

            // Taiwan
            new BlizzardLocale("zh_TW", BlizzardLocaleFlags.ChineseTW),

            // China
            new BlizzardLocale("zh_CN", BlizzardLocaleFlags.ChineseCN)
        };

        public static BlizzardLocale Get(BlizzardLocaleFlags locale)
        {
            return Locales.Single(x => x.Flag == locale);
        }
    }
    
    [Flags]
    public enum BlizzardLocaleFlags : uint
    {
        All = 4294967295, // 0xFFFFFFFF
        None = 0,
        EnglishUS = 2,
        Korean = 4,
        French = 16,
        German = 32,
        ChineseCN = 64,
        SpanishES = 128,
        ChineseTW = 256,
        EnglishGB = 512,
        EnglishCN = 1024,
        EnglishTW = 2048,
        SpanishMX = 4096,
        Russian = 8192,
        PortugeseBR = 16384,
        Italian = 32768,
        PortugesePT = 65536,
        EnglishSG = 536870912,
        Polish = 1073741824,
        AllWowLocales = PortugesePT | Italian | PortugeseBR | Russian | SpanishMX | EnglishGB | ChineseTW | SpanishES | ChineseCN | German | French | Korean | EnglishUS, // 0x0001F3F6
    }

    public class BlizzardLocale
    {
        public BlizzardLocale(string name, BlizzardLocaleFlags flag)
        {
            Name = name;
            Flag = flag;
            Culture = CultureInfo.GetCultureInfo(name.Replace("_","-"));
        }

        public string Name { get; set; }

        public BlizzardLocaleFlags Flag { get; set; }

        public CultureInfo Culture { get; set; }
    }
}
