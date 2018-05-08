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

        private (int length, Func<byte[], object>) ReadField(Type fieldType,
            string fieldName,
            string fieldOrProp,
            FieldLengthAttribute fieldLengthAttr,
            FieldEncodingAttribute fieldEncodingAttr)
        {
            int length;
            Func<byte[], object> func;

            if (typeof(short) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(short);
                func = data => BitConverter.ToInt16(data, 0);
            }
            else if (typeof(int) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(int);
                func = data => BitConverter.ToInt32(data, 0);
            }
            else if (typeof(long) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                func = data => BitConverter.ToInt64(data, 0);
            }
            else if (typeof(bool) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(bool);
                func = data => BitConverter.ToBoolean(data, 0);
            }
            else if (typeof(float) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(float);
                func = data => BitConverter.ToSingle(data, 0);
            }
            else if (typeof(double) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(double);
                func = data => BitConverter.ToDouble(data, 0);
            }
            else if (typeof(ushort) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(ushort);
                func = data => BitConverter.ToUInt16(data, 0);
            }
            else if (typeof(uint) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(uint);
                func = data => BitConverter.ToUInt32(data, 0);
            }
            else if (typeof(ulong) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(ulong);
                func = data => BitConverter.ToUInt64(data, 0);
            }
            else if (typeof(char) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(char);
                func = data => BitConverter.ToChar(data, 0);
            }
            else if (typeof(byte) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(byte);
                func = data => data.FirstOrDefault();
            }
            else if (typeof(byte[]) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {fieldOrProp} '{fieldName}'.");
                }
                func = data => data.Take(length).ToArray();
            }
            else if (typeof(string) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {fieldOrProp} '{fieldName}'.");
                }
                var encoding = fieldEncodingAttr?.GetEncoding() ?? Encoding.UTF8;
                func = data => encoding.GetString(data, 0, length).TrimEnd('\0');
            }
            else if (typeof(DateTime) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                func = data => DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            }
            else if (typeof(TimeSpan) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                func = data => TimeSpan.FromTicks(BitConverter.ToInt64(data, 0));
            }
            else
            {
                throw new NotSupportedException(
                    $"Type {fieldType.Name} of {fieldOrProp} '{fieldName}' is not supported.");
            }

            return (length, func);
        }

        public T Read<T>() where T : new()
        {
            bool isValueType = typeof(T).IsValueType;
            var rawModel = new T();
            object model = rawModel;

            var fieldList = FieldHelper.GetFieldList<T>(rawModel);

            if (isValueType)
            {
                model = rawModel as ValueType;
            }

            var basePos = _fileStream.Position;
            var lastPos = _fileStream.Position;

            foreach (var field in fieldList)
            {
                var offset = field.offset;
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

                (var length, var func) = ReadField(field.type, field.name, field.fieldOrProp,
                    field.fieldLengthAttr, field.fieldEncodingAttr);

                if (_buffer.Length < length) _buffer = new byte[length];
                _fileStream.Read(_buffer, 0, length);

                if (offset == -1)
                {
                    lastPos = _fileStream.Position;
                }

                var val = func(_buffer);
                if (val == null)
                {
                    throw new InvalidDataException($"The value of {field.fieldOrProp} '{field.name}' cannot be null.");
                }

                field.field?.SetValue(model, val);
                field.prop?.SetValue(model, val);
            }

            _fileStream.Seek(lastPos, SeekOrigin.Begin);

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
