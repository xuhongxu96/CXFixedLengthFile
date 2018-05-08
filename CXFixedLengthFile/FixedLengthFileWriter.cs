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
    public class FixedLengthFileWriter
    {
        private FileStream _fileStream = null;

        public FixedLengthFileWriter(FileStream stream)
        {
            _fileStream = stream;
        }

        private (int length, byte[] buffer) GetFieldBuffer<T>(T model, FieldInfo field, FixedLengthFieldAttribute fieldAttr)
        {
            int length;
            byte[] buffer;

            var val = field.GetValue(model);
            if (val == null)
            {
                throw new InvalidDataException($"The value of field '{field.Name}' cannot be null.");
            }

            if (typeof(short) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(short));
                buffer = BitConverter.GetBytes((short)val);
            }
            else if (typeof(int) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(int));
                buffer = BitConverter.GetBytes((int)val);
            }
            else if (typeof(long) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(long));
                buffer = BitConverter.GetBytes((long)val);
            }
            else if (typeof(bool) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(bool));
                buffer = BitConverter.GetBytes((bool)val);
            }
            else if (typeof(float) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(float));
                buffer = BitConverter.GetBytes((float)val);
            }
            else if (typeof(double) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(double));
                buffer = BitConverter.GetBytes((double)val);
            }
            else if (typeof(ushort) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(ushort));
                buffer = BitConverter.GetBytes((ushort)val);
            }
            else if (typeof(uint) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(uint));
                buffer = BitConverter.GetBytes((uint)val);
            }
            else if (typeof(ulong) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(ulong));
                buffer = BitConverter.GetBytes((ulong)val);
            }
            else if (typeof(char) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(char));
                buffer = BitConverter.GetBytes((char)val);
            }
            else if (typeof(byte) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(byte));
                buffer = new byte[] { (byte)val };
            }
            else if (typeof(byte[]) == field.FieldType)
            {
                length = fieldAttr.GetLength(-1);
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string field '{field.Name}'.");
                }
                buffer = (byte[])val;
            }
            else if (typeof(string) == field.FieldType)
            {
                length = fieldAttr.GetLength(-1);
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string field '{field.Name}'.");
                }
                buffer = Encoding.GetEncoding(fieldAttr.encoding)
                    .GetBytes((string)val);
            }
            else
            {
                throw new NotSupportedException(
                    $"Type {field.FieldType.Name} of field '{field.Name}' is not supported.");
            }

            if (length < buffer.Length)
            {
                throw new InvalidDataException($"The value size of field '{field.Name}' exceeds " +
                    $"the specific field length: {length}.");
            }

            return (length, buffer);
        }

        public void Write<T>(T model)
        {
            foreach (var field in typeof(T).GetFields())
            {
                foreach (var attr in field.GetCustomAttributes(true))
                {
                    if (attr is FixedLengthFieldAttribute fieldAttr)
                    {
                        var offset = fieldAttr.offset;

                        if (offset != -1)
                        {
                            break;
                        }

                        (var length, var buffer) = GetFieldBuffer(model, field, fieldAttr);

                        _fileStream.Write(buffer, 0, buffer.Length);

                        if (length > buffer.Length)
                        {
                            Debug.WriteLine($"The value size of field '{field.Name}' is smaller than " +
                                $"the specific field length{length}. Written bytes will be padded.");

                            buffer = new byte[length - buffer.Length];
                            // for (int i = 0; i < buffer.Length; ++i) buffer[i] = 0;
                            _fileStream.Write(buffer, 0, buffer.Length);
                        }

                        break;
                    }
                }
            }
        }

        public async void WriteAsync<T>(T model)
        {
            await Task.Run(() =>
            {
                Write(model);
            });
        }
    }
}
