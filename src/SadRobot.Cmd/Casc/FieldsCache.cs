using System;
using System.Linq;
using System.Reflection;

namespace SadRobot.Cmd.Casc
{
    public class FieldsCache<T>
    {
        private static readonly FieldCache[] fieldsCache;

        static FieldsCache()
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.MetadataToken).ToArray();

            fieldsCache = new FieldCache[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                fieldsCache[i] = (FieldCache)Activator.CreateInstance(typeof(FieldCache<,>).MakeGenericType(typeof(T), fields[i].FieldType), fields[i]);

            //Console.WriteLine($"FieldsCache<{typeof(T).Name}> created");
        }

        public static FieldCache[] Cache => fieldsCache;
    }
}