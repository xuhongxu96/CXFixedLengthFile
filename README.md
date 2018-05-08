# CXFixedLengthFile

Fixed Length File Reader and Writer.

## Usage

Use Attribute `FixedLengthField`, `UnionField`, `FieldLength` and `FieldEncoding` to annotate the **fields** and **properties** of your struct or class.

### Field Type Attributes

- FixedLengthField
- UnionField

If you want to make some fields or properties in your struct or class map to fixed-length bytes in file, you should annotate them with **ANY** of the two attributes. You **CANNOT** annotate both of them to the same field or property.

#### FixedLengthField

`FixedLengthField(int order)`

- `order`: The order of the field in file

Because `GetFields` and `GetProperties` in .NET Reflection cannot guarantee the order of the results, so you MUST specify the order of the field. And you can set the order as 1, 5, 3, 4, 2, ...

`FixedLengthFileReader` and `FixedLengthFileWriter` will map the fields and properties with **this attribute** to the specific fixed-length bytes in file.

#### UnionField

`UnionField(int offset)`

- `offset`: The offset or beginning position of the field in file

Union field acts like C/C++ Union.

`FixedLengthFileReader` will map the fields and properties with **this attribute** to the specific fixed-length bytes from specific beginning position (offset) in file.

`FixedLengthFileWriter` will do nothing with the fields and properties with **this attribute**.

### Field Property Attributes

- FieldLength
- FieldEncoding (*Only for string*)

Generally, both of these two attributes are **optional**.

#### FieldLength

`FieldLength(int length)`

- `length`: The length of the bytes this field or property will occupy

The default length is `sizeof(Field Type or Property Type)`, so you can omit this attribute in most cases.  
For type `string` and `byte[]`, you **MUST** specify the length of the field or property.

The length you specify **CANNOT** be smaller than the size of the type of the field or property.  
For example, you can annotate `FieldLength(4)` or `FieldLength(10)` to `int intField`, but `FieldLength(3)` is not allowed.

If the length is bigger than the size of the type or value of the field or property, the rest bytes will be '\0'.

#### FieldEncoding

`FieldEncoding(string encoding)`

- `encoding`: The encoding to encode and decode the bytes of a string

The default encoding is `UTF-8`, so it is **optional**.

You can **ONLY** annotate this attribute to a `string` field or property.

### Supported Types

- `char`
- `byte`
- `short`
- `int`
- `long`
- `ushort`
- `uint`
- `ulong`
- `bool`
- `float`
- `double`
- `byte[]` (*Must specify FieldLength*)
- `string` (*Must specify FieldLength*)

### Example

#### Model

``` C#
public struct FileModel
{
    /// <summary>
    /// Offset: 0
    /// Length: 100
    /// Encoding: UTF-8 (Default)
    /// </summary>
    [FixedLengthField(1)]
    [FieldLength(100)]
    public string utf8StrField { get; set; }

    /// <summary>
    /// Offset: 108
    /// Length: 10
    /// </summary>
    [FixedLengthField(3)]
    [FieldLength(10)]
    public int intField;

    /// <summary>
    /// Offset: 100
    /// Length: 8
    /// </summary>
    [FixedLengthField(2)]
    [FieldLength(8)]
    public long longField;

    /// <summary>
    /// Offset: 118
    /// Length: 8
    /// Encoding: ASCII
    /// </summary>
    [FixedLengthField(4)]
    [FieldLength(8)]
    [FieldEncoding("ASCII")]
    public string asciiStrField;

    /// <summary>
    /// Offset: 0
    /// Length: 4
    /// </summary>
    [UnionField(0)]
    public int intUnionField { get; set; }

    /// <summary>
    /// Offset: 119
    /// Length: 3
    /// </summary>
    [UnionField(119)]
    [FieldLength(3)]
    public string strUnionField;

    /// <summary>
    /// Offset: 126
    /// Length: 2
    /// </summary>
    [FixedLengthField(5)]
    public char charField;

    /// <summary>
    /// Offset: 128
    /// Length: 2
    /// </summary>
    [FixedLengthField(6)]
    [FieldLength(2)]
    public byte[] byteArrField;
}
```

#### Write

``` C#
var model1 = new FileModel
{
    utf8StrField = "这是一个",
    intField = int.MaxValue,
    longField = long.MinValue,
    asciiStrField = "abcdefgh",
    intUnionField = 1,
    charField = char.MaxValue,
    byteArrField = new byte[] { 1, 2 },
};

var model2 = new FileModel
{
    utf8StrField = "测试文本ABCD",
    intField = 1,
    longField = long.MaxValue,
    asciiStrField = "abc",
    charField = char.MinValue,
    byteArrField = new byte[] { 0, 255 },
};

var fileName = Path.GetTempFileName();

using (var stream = File.OpenWrite(fileName))
{
	var writer = new FixedLengthFileWriter<FileModel>(stream);
	writer.Write(model1);
	writer.Write(model2);
}
```

#### Read

``` C#
using (var stream = File.OpenRead(fileName))
{
	var reader = new FixedLengthFileReader<FileModel>(stream);
	var model1 = reader.Read();
	var model2 = reader.Read();
}
```

Hongxu Xu (R) 2018