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
    public class FixedLengthFileReader<T> : FixedLengthFileIO<T> where T : new()
    {
        private byte[] _buffer;

        public FixedLengthFileReader(FileStream stream) : base(stream)
        {
            _buffer = new byte[8];
        }

        public T Read()
        {
            bool isValueType = typeof(T).IsValueType;
            var rawModel = new T();
            object model = rawModel;

            var fieldList = FixedLengthFieldHelper.GetFixedLengthFieldList<T>();

            if (isValueType)
            {
                model = rawModel as ValueType;
            }

            var basePos = _fileStream.Position;
            var lastPos = _fileStream.Position;

            foreach (var field in fieldList)
            {
                var offset = field.Offset;
                if (offset == -1)
                {
                    if (_fileStream.Position != lastPos)
                    {
                        _fileStream.Seek(lastPos, SeekOrigin.Begin);
                    }
                }
                else
                {
                    _fileStream.Seek(basePos + offset, SeekOrigin.Begin);
                }

                var length = FixedLengthFieldHelper.GetFieldLength(field);

                if (_buffer.Length < length) _buffer = new byte[length];
                _fileStream.Read(_buffer, 0, length);

                if (offset == -1)
                {
                    lastPos = _fileStream.Position;
                }

                var val = FixedLengthFieldHelper.BytesToValue(_buffer, field);
                if (val == null)
                {
                    throw new InvalidDataException($"The value of {field.TypeString} '{field.Name}' cannot be null.");
                }

                field.SetValue(model, val);
            }

            _fileStream.Seek(lastPos, SeekOrigin.Begin);

            if (isValueType)
            {
                rawModel = (T)model;
            }

            return rawModel;
        }

        public async Task<T> ReadAsync()
        {
            return await Task.Run(() =>
            {
                return Read();
            });
        }
    }
}
