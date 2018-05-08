using CXFixedLengthFile.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CXFixedLengthFile
{
    class FieldHelper
    {
        public class Field
        {
            public int order = -1;
            public int offset = -1;
            public string name = null;
            public Type type = null;
            public object value = null;
            public FieldLengthAttribute fieldLengthAttr = null;
            public FieldEncodingAttribute fieldEncodingAttr = null;

            public string fieldOrProp = null;

            public FieldInfo field = null;
            public PropertyInfo prop = null;
        }

        private static (Field field, bool skip, bool hasOtherAttr)
            SetFieldPerAttr(object[] attrs, string fieldOrProp, string fieldName, Type fieldType)
        {
            var currentField = new Field();
            var skip = true;
            var hasOtherAttr = false;
            foreach (var attr in attrs)
            {
                if (attr is UnionFieldAttribute unionAttr)
                {
                    currentField.offset = unionAttr.GetOffset();
                    if (currentField.order != -1)
                    {
                        throw new InvalidDataException($"You can only annotate one of the " +
                            $"UnionField and FixedLengthField to {fieldOrProp} '{fieldName}'.");
                    }
                    skip = false;
                }
                else if (attr is FixedLengthFieldAttribute fieldAttr)
                {
                    currentField.order = fieldAttr.GetOrder();
                    if (currentField.offset != -1)
                    {
                        throw new InvalidDataException($"You can only annotate one of the " +
                            $"UnionField and FixedLengthField to {fieldOrProp} '{fieldName}'.");
                    }
                    skip = false;
                }
                else if (attr is FieldLengthAttribute lengthAttr)
                {
                    currentField.fieldLengthAttr = lengthAttr;
                    hasOtherAttr = true;
                }
                else if (attr is FieldEncodingAttribute encodingAttr)
                {
                    if (fieldType != typeof(string))
                    {
                        throw new InvalidDataException($"FieldEncoding is only for string, " +
                            $"but {fieldOrProp} '{fieldName}' is not a string.");
                    }
                    currentField.fieldEncodingAttr = encodingAttr;
                    hasOtherAttr = true;
                }
            }

            return (currentField, skip, hasOtherAttr);
        }

        public static List<Field> GetFieldList<T>(T model, bool skipOffset = false)
        {
            var fieldList = new List<Field>();

            foreach (var field in typeof(T).GetFields())
            {
                (var currentField, var skip, var hasOtherAttr)
                    = SetFieldPerAttr(field.GetCustomAttributes(true), "field", field.Name, field.FieldType);

                if (!skip)
                {
                    if (skipOffset && currentField.offset != -1)
                    {
                        continue;
                    }
                    currentField.name = field.Name;
                    currentField.type = field.FieldType;
                    currentField.value = field.GetValue(model);

                    currentField.field = field;
                    currentField.fieldOrProp = "field";

                    fieldList.Add(currentField);
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
                    if (skipOffset && currentField.offset != -1)
                    {
                        continue;
                    }
                    currentField.name = prop.Name;
                    currentField.type = prop.PropertyType;
                    currentField.value = prop.GetValue(model);

                    currentField.prop = prop;
                    currentField.fieldOrProp = "property";

                    fieldList.Add(currentField);
                }
                else if (hasOtherAttr)
                {
                    throw new InvalidDataException($"Property '{prop.Name}' has FieldLengthAttribute " +
                        $"or FieldEncodingAttribute FixedLengthUnionField, " +
                        $"but doesn't have FixedLengthFieldAttribute or UnionFieldAttribute.");
                }
            }

            fieldList.Sort((x, y) =>
            {
                if (x.order == -1) return 1;
                else if (y.order == -1) return -1;
                else return x.order.CompareTo(y.order);
            });

            return fieldList;
        }
    }
}
