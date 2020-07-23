using System.Collections.Generic;
using CASCLib;
using SadRobot.Cmd.Casc;

namespace SadRobot.Cmd
{
    public static class CascExtensions
    {
        public static IEnumerable<KeyValuePair<int, WDC3Row>> EnumerateTable(this CASCFolder folder, string name, CASCHandler handler)
        {
            var entry = folder.GetEntry(name + ".db2");
            using var stream = handler.OpenFile(entry.Hash);
            var reader = new WDC3Reader(stream);
            foreach (var pair in reader)
            {
                yield return pair;
            }
        }
    }
}