using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CXFixedLengthFile
{
    public class FixedLengthFileWriter<T> : FixedLengthFileIO<T>
    {
        public FixedLengthFileWriter(FileStream stream) : base(stream)
        { }

        public void Write(T model)
        {
            var fieldList = FixedLengthFieldHelper.GetFixedLengthFieldList<T>(true);

            foreach (var field in fieldList)
            {
                var value = field.GetValue(model);
                if (value == null)
                {
                    throw new InvalidDataException($"The value of {field.TypeString} '{field.Name}' cannot be null.");
                }

                var length = FixedLengthFieldHelper.GetFieldLength(field);
                var buffer = FixedLengthFieldHelper.ValueToBytes(value, field);

                if (length < buffer.Length)
                {
                    throw new InvalidDataException($"The value size of {field.TypeString} '{field.Name}' exceeds " +
                        $"the specific {field.TypeString} length: {length}.");
                }

                _fileStream.Write(buffer, 0, buffer.Length);

                if (length > buffer.Length)
                {
                    Debug.WriteLine($"The value size of {field.TypeString} '{field.Name}' is smaller than " +
                        $"the specific {field.TypeString} length {length}. Written bytes will be padded.");

                    // pad with '\0'
                    buffer = new byte[length - buffer.Length];
                    _fileStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public async void WriteAsync(T model)
        {
            await Task.Run(() =>
            {
                Write(model);
            });
        }
    }
}
