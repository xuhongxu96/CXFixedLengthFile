using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFile.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldLengthAttribute : Attribute
    {
        private int _length;
   
        public FieldLengthAttribute(int length)
        {
            _length = length;
        }

        public int GetLength()
        {
            return _length;
        }
    }
}
