using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFileUnitTest.Models
{
    public struct InvalidFileModel2
    {
        /// <summary>
        /// Offset: 0
        /// Length: 1
        /// </summary>
        [FixedLengthField]
        public string strField;
    }
}
