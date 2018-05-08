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
    public class FixedLengthFileWriter
    {
        private FileStream _fileStream = null;

        public FixedLengthFileWriter(FileStream stream)
        {
            _fileStream = stream;
        }

        private (int length, byte[] buffer) GetFieldBuffer<T>(T model,
            Type fieldType,
            string fieldOrProp,
            string fieldName,
            object fieldValue,
            FieldLengthAttribute fieldLengthAttr = null,
            FieldEncodingAttribute fieldEncodingAttr = null)
        {
            int length;
            byte[] buffer;

            if (fieldValue == null)
            {
                throw new InvalidDataException($"The value of {fieldOrProp} '{fieldName}' cannot be null.");
            }

            if (typeof(short) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(short);
                buffer = BitConverter.GetBytes((short)fieldValue);
            }
            else if (typeof(int) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(int);
                buffer = BitConverter.GetBytes((int)fieldValue);
            }
            else if (typeof(long) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                buffer = BitConverter.GetBytes((long)fieldValue);
            }
            else if (typeof(bool) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(bool);
                buffer = BitConverter.GetBytes((bool)fieldValue);
            }
            else if (typeof(float) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(float);
                buffer = BitConverter.GetBytes((float)fieldValue);
            }
            else if (typeof(double) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(double);
                buffer = BitConverter.GetBytes((double)fieldValue);
            }
            else if (typeof(ushort) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(ushort);
                buffer = BitConverter.GetBytes((ushort)fieldValue);
            }
            else if (typeof(uint) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(uint);
                buffer = BitConverter.GetBytes((uint)fieldValue);
            }
            else if (typeof(ulong) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(ulong);
                buffer = BitConverter.GetBytes((ulong)fieldValue);
            }
            else if (typeof(char) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(char);
                buffer = BitConverter.GetBytes((char)fieldValue);
            }
            else if (typeof(byte) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(byte);
                buffer = new byte[] { (byte)fieldValue };
            }
            else if (typeof(byte[]) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {fieldOrProp} '{fieldName}'.");
                }
                buffer = (byte[])fieldValue;
            }
            else if (typeof(string) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {fieldOrProp} '{fieldName}'.");
                }
                var encoding = fieldEncodingAttr?.GetEncoding() ?? Encoding.UTF8;
                buffer = encoding.GetBytes((string)fieldValue);
            }
            else if (typeof(DateTime) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                buffer = BitConverter.GetBytes(((DateTime)fieldValue).ToBinary());
            }
            else if (typeof(TimeSpan) == fieldType)
            {
                length = fieldLengthAttr?.GetLength() ?? sizeof(long);
                buffer = BitConverter.GetBytes(((TimeSpan)fieldValue).Ticks);
            }
            else
            {
                throw new NotSupportedException(
                    $"Type {fieldType.Name} of {fieldOrProp} '{fieldName}' is not supported.");
            }

            if (length < buffer.Length)
            {
                throw new InvalidDataException($"The value size of {fieldOrProp} '{fieldName}' exceeds " +
                    $"the specific {fieldOrProp} length: {length}.");
            }

            return (length, buffer);
        }

        public void Write<T>(T model)
        {
            var fieldList = FieldHelper.GetFieldList(model, true);

            foreach (var field in fieldList)
            {
                (var length, var buffer) = GetFieldBuffer(model,
                    field.type, field.name, field.fieldOrProp, field.value, 
                    field.fieldLengthAttr, field.fieldEncodingAttr);

                _fileStream.Write(buffer, 0, buffer.Length);

                if (length > buffer.Length)
                {
                    Debug.WriteLine($"The value size of {field.fieldOrProp} '{field.name}' is smaller than " +
                        $"the specific {field.fieldOrProp} length {length}. Written bytes will be padded.");

                    buffer = new byte[length - buffer.Length];
                    // for (int i = 0; i < buffer.Length; ++i) buffer[i] = 0;
                    _fileStream.Write(buffer, 0, buffer.Length);
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
