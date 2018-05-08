using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFile.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldEncodingAttribute : Attribute
    {
        private string _encoding;

        public FieldEncodingAttribute(string encoding)
        {
            _encoding = encoding;
        }

        public Encoding GetEncoding()
        {
            return Encoding.GetEncoding(_encoding);
        }
    }
}
