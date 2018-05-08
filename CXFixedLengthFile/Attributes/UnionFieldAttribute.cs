using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFile.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UnionFieldAttribute : Attribute
    {
        private int _offset;

        public UnionFieldAttribute(int offset)
        {
            _offset = offset;
        }

        public int GetOffset()
        {
            return _offset;
        }
    }
}
