# CXFixedLengthFile

Fixed Length File Reader and Writer.

## Usage

Use Attribute `FixedLengthField` to annotate the **field** of your class/struct.

`FixedLengthField(int length, [Properties: int offset = -1, string encoding = "UTF-8"])`

- `length` specifies the length of bytes that the field occupies in file  
- `offset` specifies the beginning position of the field, which makes the field like a union field (only used for reading file)  
- `encoding` specifies the encoding to read and write the string (only used for string field)  

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
- `byte[]`
- `string`

### Example

#### Model

``` C#
public struct FileModel
{
    /// <summary>
    /// Offset: 0
    /// Length: 100
    /// </summary>
    [FixedLengthField(100)]
    public string utf8StrField;

    /// <summary>
    /// Offset: 100
    /// Length: 10
    /// </summary>
    [FixedLengthField(10)]
    public int intField;

    /// <summary>
    /// Offset: 110
    /// Length: 8
    /// </summary>
    [FixedLengthField]
    public long longField;

    /// <summary>
    /// Offset: 118
    /// Length: 8
    /// </summary>
    [FixedLengthField(8, encoding = "ascii")]
    public string asciiStrField;

    /// <summary>
    /// Offset: 0
    /// Length: 4
    /// </summary>
    [FixedLengthField(offset = 0)]
    public int intFieldUnion;

    /// <summary>
    /// Offset: 126
    /// Length: 2
    /// </summary>
    [FixedLengthField]
    public char charField;

    /// <summary>
    /// Offset: 128
    /// Length: 2
    /// </summary>
    [FixedLengthField(2)]
    public byte[] byteArrField;
}
```

#### Read

``` C#
using (var stream = File.OpenRead(fileName))
{
    var reader = new FixedLengthFileReader(stream);
	var model = reader.Read<FileModel>();
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
    intFieldUnion = 1,
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
    byteArrField = new byte[] { 1, 2 },
};

var fileName = Path.GetTempFileName();

using (var stream = File.OpenWrite(fileName))
{
    var writer = new FixedLengthFileWriter(stream);
    writer.Write(model1);
	writer.Write(model2);
}
```

Hongxu Xu (R) 2018