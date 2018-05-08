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
    public class FixedLengthFileReader
    {
        private FileStream _fileStream = null;
        private byte[] _buffer;

        public FixedLengthFileReader(FileStream stream)
        {
            _fileStream = stream;
            _buffer = new byte[8];
        }

        private (int length, Func<byte[], object>) ReadField(FieldInfo field, FixedLengthFieldAttribute fieldAttr)
        {
            int length;
            Func<byte[], object> func;

            if (typeof(short) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(short));
                func = data => BitConverter.ToInt16(data, 0);
            }
            else if (typeof(int) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(int));
                func = data => BitConverter.ToInt32(data, 0);
            }
            else if (typeof(long) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(long));
                func = data => BitConverter.ToInt64(data, 0);
            }
            else if (typeof(bool) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(bool));
                func = data => BitConverter.ToBoolean(data, 0);
            }
            else if (typeof(float) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(float));
                func = data => BitConverter.ToSingle(data, 0);
            }
            else if (typeof(double) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(double));
                func = data => BitConverter.ToDouble(data, 0);
            }
            else if (typeof(ushort) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(ushort));
                func = data => BitConverter.ToUInt16(data, 0);
            }
            else if (typeof(uint) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(uint));
                func = data => BitConverter.ToUInt32(data, 0);
            }
            else if (typeof(ulong) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(ulong));
                func = data => BitConverter.ToUInt64(data, 0);
            }
            else if (typeof(char) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(char));
                func = data => BitConverter.ToChar(data, 0);
            }
            else if (typeof(byte) == field.FieldType)
            {
                length = fieldAttr.GetLength(sizeof(byte));
                func = data => data.FirstOrDefault();
            }
            else if (typeof(byte[]) == field.FieldType)
            {
                length = fieldAttr.GetLength(-1);
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string field '{field.Name}'.");
                }
                func = data => data.Take(length).ToArray();
            }
            else if (typeof(string) == field.FieldType)
            {
                length = fieldAttr.GetLength(-1);
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string field '{field.Name}'.");
                }
                func = data => Encoding.GetEncoding(fieldAttr.encoding).GetString(data, 0, length).TrimEnd('\0');
            }
            else
            {
                throw new NotSupportedException(
                    $"Type {field.FieldType.Name} of field '{field.Name}' is not supported.");
            }

            return (length, func);
        }

        public T Read<T>() where T : new()
        {
            bool isValueType = typeof(T).IsValueType;
            var rawModel = new T();
            object model = rawModel;

            if (isValueType)
            {
                model = rawModel as ValueType;
            }

            var basePos = _fileStream.Position;
            var lastPos = _fileStream.Position;

            foreach (var field in typeof(T).GetFields())
            {
                foreach (var attr in field.GetCustomAttributes(true))
                {
                    if (attr is FixedLengthFieldAttribute fieldAttr)
                    {
                        var offset = fieldAttr.offset;

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

                        (var length, var func) = ReadField(field, fieldAttr);

                        if (_buffer.Length < length) _buffer = new byte[length];
                        _fileStream.Read(_buffer, 0, length);

                        if (offset == -1)
                        {
                            lastPos = _fileStream.Position;
                        }

                        var val = func(_buffer);
                        if (val == null)
                        {
                            throw new InvalidDataException($"Field '{field.Name}' cannot be null.");
                        }

                        field.SetValue(model, val);

                        break;
                    }
                }
            }

            if (isValueType)
            {
                rawModel = (T)model;
            }

            return rawModel;
        }

        public async Task<T> ReadAsync<T>() where T : new()
        {
            return await Task.Run(() =>
            {
                return Read<T>();
            });
        }
    }
}
