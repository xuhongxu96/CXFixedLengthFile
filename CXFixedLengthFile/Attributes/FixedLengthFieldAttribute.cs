using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFile.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FixedLengthFieldAttribute : Attribute
    {
        private int _length = -1;

        /// <summary>
        /// Only for reading as a union
        /// </summary>
        public long offset = -1;
        public string encoding = "UTF-8";

        public FixedLengthFieldAttribute()
        { }

        /// <summary>
        /// Init with specific field length
        /// </summary>
        /// <param name="length">Field length</param>
        public FixedLengthFieldAttribute(int length)
        {
            _length = length;
        }

        public int GetLength(int defaultLength)
        {
            if (_length == -1) return defaultLength;
            return _length;
        }
    }
}
