using System.Collections.Generic;
using System.Dynamic;
using SadRobot.Cmd.DBCD.Helpers;

namespace SadRobot.Cmd.DBCD
{
    public class DBCDRow : DynamicObject
    {
        public int ID;

        private readonly dynamic raw;
        private readonly FieldAccessor fieldAccessor;

        internal DBCDRow(int ID, dynamic raw, FieldAccessor fieldAccessor)
        {
            this.raw = raw;
            this.fieldAccessor = fieldAccessor;
            this.ID = ID;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return fieldAccessor.TryGetMember(this.raw, binder.Name, out result);
        }

        public object this[string fieldName]
        {
            get => fieldAccessor[this.raw, fieldName];
        }

        public T Field<T>(string fieldName)
        {
            return (T)fieldAccessor[this.raw, fieldName];
        }

        public T FieldAs<T>(string fieldName)
        {
            return fieldAccessor.GetMemberAs<T>(this.raw, fieldName);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return fieldAccessor.FieldNames;
        }
    }
}