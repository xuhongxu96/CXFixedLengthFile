using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CXFixedLengthFile
{
    public static class FixedLengthFieldHelper
    {
        public class FixedLengthField
        {
            public bool IsField => Field != null;
            public bool IsProperty => Property != null;

            public FieldInfo Field { get; set; } = null;
            public PropertyInfo Property { get; set; } = null;

            public string TypeString => IsField ? "field" : "property";

            public int Order { get; set; } = -1;
            public int Offset { get; set; } = -1;

            public string Name => IsField ? Field?.Name : Property?.Name;
            public Type Type => IsField ? Field?.FieldType : Property?.PropertyType;

            public object GetValue(object model) => IsField ? Field?.GetValue(model) : Property?.GetValue(model);

            public void SetValue(object model, object val)
            {
                if (IsField)
                {
                    Field?.SetValue(model, val);
                }
                else
                {
                    Property?.SetValue(model, val);
                }
            }
            public FieldLengthAttribute FieldLengthAttr { get; set; } = null;
            public FieldEncodingAttribute FieldEncodingAttr { get; set; } = null;
        }

        private static (FixedLengthField field, bool skip, bool hasOtherAttr)
            SetFieldPerAttr(object[] attrs, string fieldOrProp, string fieldName, Type fieldType)
        {
            var currentField = new FixedLengthField();
            var skip = true;
            var hasOtherAttr = false;
            foreach (var attr in attrs)
            {
                if (attr is UnionFieldAttribute unionAttr)
                {
                    currentField.Offset = unionAttr.GetOffset();
                    if (currentField.Order != -1)
                    {
                        throw new InvalidDataException($"You can only annotate one of the " +
                            $"UnionField and FixedLengthField to {fieldOrProp} '{fieldName}'.");
                    }
                    skip = false;
                }
                else if (attr is FixedLengthFieldAttribute fieldAttr)
                {
                    currentField.Order = fieldAttr.GetOrder();
                    if (currentField.Offset != -1)
                    {
                        throw new InvalidDataException($"You can only annotate one of the " +
                            $"UnionField and FixedLengthField to {fieldOrProp} '{fieldName}'.");
                    }
                    skip = false;
                }
                else if (attr is FieldLengthAttribute lengthAttr)
                {
                    currentField.FieldLengthAttr = lengthAttr;
                    hasOtherAttr = true;
                }
                else if (attr is FieldEncodingAttribute encodingAttr)
                {
                    if (fieldType != typeof(string))
                    {
                        throw new InvalidDataException($"FieldEncoding is only for string, " +
                            $"but {fieldOrProp} '{fieldName}' is not a string.");
                    }
                    currentField.FieldEncodingAttr = encodingAttr;
                    hasOtherAttr = true;
                }
            }

            return (currentField, skip, hasOtherAttr);
        }

        public static IEnumerable<FixedLengthField> GetFixedLengthFields<T>()
        {
            foreach (var field in typeof(T).GetFields())
            {
                (var currentField, var skip, var hasOtherAttr)
                    = SetFieldPerAttr(field.GetCustomAttributes(true), "field", field.Name, field.FieldType);

                if (!skip)
                {
                    currentField.Field = field;
                    yield return currentField;
                }
                else if (hasOtherAttr)
                {
                    throw new InvalidDataException($"Field '{field.Name}' has FieldLengthAttribute " +
                        $"or FieldEncodingAttribute FixedLengthUnionField, " +
                        $"but doesn't have FixedLengthFieldAttribute or UnionFieldAttribute.");
                }
            }

            foreach (var prop in typeof(T).GetProperties())
            {
                (var currentField, var skip, var hasOtherAttr)
                    = SetFieldPerAttr(prop.GetCustomAttributes(true), "property", prop.Name, prop.PropertyType);

                if (!skip)
                {
                    currentField.Property = prop;
                    yield return currentField;
                }
                else if (hasOtherAttr)
                {
                    throw new InvalidDataException($"Property '{prop.Name}' has FieldLengthAttribute " +
                        $"or FieldEncodingAttribute FixedLengthUnionField, " +
                        $"but doesn't have FixedLengthFieldAttribute or UnionFieldAttribute.");
                }
            }
        }

        public static List<FixedLengthField> GetFixedLengthFieldList<T>(bool skipOffset = false)
        {
            var fields = GetFixedLengthFields<T>();

            if (skipOffset)
            {
                fields = fields.Where(o => o.Offset == -1);
            }

            var fieldList = fields.ToList();

            fieldList.Sort((x, y) =>
            {
                if (x.Order == -1) return 1;
                else if (y.Order == -1) return -1;
                else return x.Order.CompareTo(y.Order);
            });

            return fieldList;
        }

        public static int GetFieldLength(FixedLengthField field)
        {           
            if (typeof(short) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(short);
            }
            else if (typeof(int) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(int);
            }
            else if (typeof(long) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(long);
            }
            else if (typeof(bool) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(bool);
            }
            else if (typeof(float) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(float);
            }
            else if (typeof(double) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(double);
            }
            else if (typeof(ushort) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(ushort);
            }
            else if (typeof(uint) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(uint);
            }
            else if (typeof(ulong) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(ulong);
            }
            else if (typeof(char) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(char);
            }
            else if (typeof(byte) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(byte);
            }
            else if (typeof(byte[]) == field.Type)
            {
                var length = field.FieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {field.TypeString} '{field.Name}'.");
                }
                return length;
            }
            else if (typeof(string) == field.Type)
            {
                var length = field.FieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {field.TypeString} '{field.Name}'.");
                }
                return length;
            }
            else if (typeof(DateTime) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(long);
            }
            else if (typeof(TimeSpan) == field.Type)
            {
                return field.FieldLengthAttr?.GetLength() ?? sizeof(long);
            }
            else
            {
                throw new NotSupportedException(
                   $"Type {field.Type.Name} of {field.TypeString} '{field.Name}' is not supported.");
            }
        }

        public static object BytesToValue(byte[] data, FixedLengthField field)
        {
            if (typeof(short) == field.Type)
            {
                return BitConverter.ToInt16(data, 0);
            }
            else if (typeof(int) == field.Type)
            {
                return BitConverter.ToInt32(data, 0);
            }
            else if (typeof(long) == field.Type)
            {
                return BitConverter.ToInt64(data, 0);
            }
            else if (typeof(bool) == field.Type)
            {
                return BitConverter.ToBoolean(data, 0);
            }
            else if (typeof(float) == field.Type)
            {
                return BitConverter.ToSingle(data, 0);
            }
            else if (typeof(double) == field.Type)
            {
                return BitConverter.ToDouble(data, 0);
            }
            else if (typeof(ushort) == field.Type)
            {
                return BitConverter.ToUInt16(data, 0);
            }
            else if (typeof(uint) == field.Type)
            {
                return BitConverter.ToUInt32(data, 0);
            }
            else if (typeof(ulong) == field.Type)
            {
                return BitConverter.ToUInt64(data, 0);
            }
            else if (typeof(char) == field.Type)
            {
                return BitConverter.ToChar(data, 0);
            }
            else if (typeof(byte) == field.Type)
            {
                return data[0];
            }
            else if (typeof(byte[]) == field.Type)
            {
                var length = field.FieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {field.TypeString} '{field.Name}'.");
                }
                return data.Take(length).ToArray();
            }
            else if (typeof(string) == field.Type)
            {
                var length = field.FieldLengthAttr?.GetLength() ?? -1;
                if (length == -1)
                {
                    throw new InvalidDataException($"Should specific length of string {field.TypeString} '{field.Name}'.");
                }
                var encoding = field.FieldEncodingAttr?.GetEncoding() ?? Encoding.UTF8;
                return encoding.GetString(data, 0, length).TrimEnd('\0');
            }
            else if (typeof(DateTime) == field.Type)
            {
                return DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            }
            else if (typeof(TimeSpan) == field.Type)
            {
                return TimeSpan.FromTicks(BitConverter.ToInt64(data, 0));
            }
            else
            {
                throw new NotSupportedException(
                   $"Type {field.Type.Name} of {field.TypeString} '{field.Name}' is not supported.");
            }
        }

        public static byte[] ValueToBytes(object value, FixedLengthField field)
        {
            if (typeof(short) == field.Type)
            {
                return BitConverter.GetBytes((short)value);
            }
            else if (typeof(int) == field.Type)
            {
                return BitConverter.GetBytes((int)value);
            }
            else if (typeof(long) == field.Type)
            {
                return BitConverter.GetBytes((long)value);
            }
            else if (typeof(bool) == field.Type)
            {
                return BitConverter.GetBytes((bool)value);
            }
            else if (typeof(float) == field.Type)
            {
                return BitConverter.GetBytes((float)value);
            }
            else if (typeof(double) == field.Type)
            {
                return BitConverter.GetBytes((double)value);
            }
            else if (typeof(ushort) == field.Type)
            {
                return BitConverter.GetBytes((ushort)value);
            }
            else if (typeof(uint) == field.Type)
            {
                return BitConverter.GetBytes((uint)value);
            }
            else if (typeof(ulong) == field.Type)
            {
                return BitConverter.GetBytes((ulong)value);
            }
            else if (typeof(char) == field.Type)
            {
                return BitConverter.GetBytes((char)value);
            }
            else if (typeof(byte) == field.Type)
            {
                return new byte[] { (byte)value };
            }
            else if (typeof(byte[]) == field.Type)
            {
                return (byte[])value;
            }
            else if (typeof(string) == field.Type)
            {
                var encoding = field.FieldEncodingAttr?.GetEncoding() ?? Encoding.UTF8;
                return encoding.GetBytes((string)value);
            }
            else if (typeof(DateTime) == field.Type)
            {
                return BitConverter.GetBytes(((DateTime)value).ToBinary());
            }
            else if (typeof(TimeSpan) == field.Type)
            {
                return BitConverter.GetBytes(((TimeSpan)value).Ticks);
            }
            else
            {
                throw new NotSupportedException(
                    $"Type {field.Type.Name} of {field.TypeString} '{field.Name}' is not supported.");
            }
        }

        public static int GetModelSize<T>()
        {
            var result = 0;
            foreach (var field in GetFixedLengthFields<T>().Where(o => o.Offset == -1))
            {
                result += GetFieldLength(field);
            }
            return result;
        }
    }
}
