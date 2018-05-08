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
        /// Encoding: UTF-8 (Default)
        /// </summary>
        [FixedLengthField(1)]
        [FieldLength(100)]
        public string utf8StrField { get; set; }

        /// <summary>
        /// Offset: 100
        /// Length: 10
        /// </summary>
        [FixedLengthField(2)]
        [FieldLength(10)]
        public int intField;

        /// <summary>
        /// Offset: 110
        /// Length: 8
        /// </summary>
        [FixedLengthField(3)]
        [FieldLength(8)]
        public long longField;

        /// <summary>
        /// Offset: 118
        /// Length: 8
        /// Encoding: ASCII
        /// </summary>
        [FixedLengthField(4)]
        [FieldLength(8)]
        [FieldEncoding("ASCII")]
        public string asciiStrField;

        /// <summary>
        /// Offset: 0
        /// Length: 4
        /// </summary>
        [UnionField(0)]
        public int intUnionField { get; set; }

        /// <summary>
        /// Offset: 119
        /// Length: 3
        /// </summary>
        [UnionField(119)]
        [FieldLength(3)]
        public string strUnionField;

        /// <summary>
        /// Offset: 126
        /// Length: 2
        /// </summary>
        [FixedLengthField(5)]
        public char charField;

        /// <summary>
        /// Offset: 128
        /// Length: 2
        /// </summary>
        [FixedLengthField(6)]
        [FieldLength(2)]
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
                   intUnionField == model.intUnionField &&
                   strUnionField == model.strUnionField &&
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
            hashCode = hashCode * -1521134295 + intUnionField.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(strUnionField);
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
                + $"intFieldUnion: {intUnionField}\n"
                + $"strFieldUnion: {strUnionField}\n"
                + $"charField: {(int)charField}\n"
                + $"byteArrField: {string.Join(",", byteArrField ?? new byte[] { })}\n";
        }
    }
}
