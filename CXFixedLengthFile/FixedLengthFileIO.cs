using CXFixedLengthFile.Attributes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CXFixedLengthFile
{
    public class FixedLengthFileIO<T>
    {
        protected FileStream _fileStream = null;
        protected int _modelSize = FixedLengthFieldHelper.GetModelSize<T>();

        public FixedLengthFileIO(FileStream stream)
        {
            _fileStream = stream;
        }

        public void SeekModel(int index)
        {
            var offset = index * _modelSize;
            _fileStream.Seek(offset, SeekOrigin.Begin);
        }

        public long ModelCount => _fileStream.Length / _modelSize;
    }
}
