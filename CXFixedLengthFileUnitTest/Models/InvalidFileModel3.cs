using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CXFixedLengthFileUnitTest.Models
{
    public struct InvalidFileModel3
    {
        [FixedLengthField]
        public Exception exceptionField;
    }
}
