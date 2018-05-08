using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CXFixedLengthFileUnitTest.Models
{
    public struct FileModel
    {
        /// <summary>
        /// Offset: 0
        /// Length: 100
        /// </summary>
        [FixedLengthField(100)]
        public string utf8StrField;

        /// <summary>
        /// Offset: 100
        /// Length: 10
        /// </summary>
        [FixedLengthField(10)]
        public int intField;

        /// <summary>
        /// Offset: 110
        /// Length: 8
        /// </summary>
        [FixedLengthField]
        public long longField;

        /// <summary>
        /// Offset: 118
        /// Length: 8
        /// </summary>
        [FixedLengthField(8, encoding = "ascii")]
        public string asciiStrField;

        /// <summary>
        /// Offset: 0
        /// Length: 4
        /// </summary>
        [FixedLengthField(offset = 0)]
        public int intFieldUnion;

        /// <summary>
        /// Offset: 126
        /// Length: 2
        /// </summary>
        [FixedLengthField]
        public char charField;

        /// <summary>
        /// Offset: 128
        /// Length: 2
        /// </summary>
        [FixedLengthField(2)]
        public byte[] byteArrField;

        public override bool Equals(object obj)
        {
            if (!(obj is FileModel))
            {
                return false;
            }

            var model = (FileModel)obj;
            return utf8StrField == model.utf8StrField &&
                   intField == model.intField &&
                   longField == model.longField &&
                   asciiStrField == model.asciiStrField &&
                   intFieldUnion == model.intFieldUnion &&
                   charField == model.charField &&
                   byteArrField.SequenceEqual(model.byteArrField);
        }

        public override int GetHashCode()
        {
            var hashCode = -960061121;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(utf8StrField);
            hashCode = hashCode * -1521134295 + intField.GetHashCode();
            hashCode = hashCode * -1521134295 + longField.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(asciiStrField);
            hashCode = hashCode * -1521134295 + intFieldUnion.GetHashCode();
            hashCode = hashCode * -1521134295 + charField.GetHashCode();
            hashCode = hashCode * -1521134295 + byteArrField.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"utf8StrField: {utf8StrField}\n"
                + $"intField: {intField}\n"
                + $"longField: {longField}\n"
                + $"asciiStrField: {asciiStrField}\n"
                + $"intFieldUnion: {intFieldUnion}\n"
                + $"charField: {(int)charField}\n"
                + $"byteArrField: {string.Join(",", byteArrField)}\n";
        }
    }
}
