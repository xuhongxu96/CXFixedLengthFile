using CXFixedLengthFile;
using CXFixedLengthFileUnitTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CXFixedLengthFileUnitTest
{
    [TestClass]
    public class FixedLengthFileUnitTest
    {
        [TestMethod]
        public void TestWriteException()
        {
            var errModel1 = new FileModel
            {
                utf8StrField = "",
                asciiStrField = "abcdefghi",
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
                    var writer = new FixedLengthFileWriter<FileModel>(stream);
                    writer.Write(errModel1);
                });
                Assert.IsTrue(exception.Message.Contains("asciiStrField"));
                Assert.IsTrue(exception.Message.Contains("exceeds"));
            }

            var errModel2 = new FileModel
            {
                asciiStrField = "abc",
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
                    var writer = new FixedLengthFileWriter<FileModel>(stream);
                    writer.Write(errModel2);
                });
                Assert.IsTrue(exception.Message.Contains("utf8StrField"));
                Assert.IsTrue(exception.Message.Contains("null"));
            }

            var errModel3 = new InvalidFileModel
            {
                intField = 1,
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
                    var writer = new FixedLengthFileWriter<InvalidFileModel>(stream);
                    writer.Write(errModel3);
                });
                Assert.IsTrue(exception.Message.Contains("intField"));
                Assert.IsTrue(exception.Message.Contains("exceeds"));
            }

            var errModel4 = new InvalidFileModel2
            {
                strField = "",
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
                    var writer = new FixedLengthFileWriter<InvalidFileModel2>(stream);
                    writer.Write(errModel4);
                });
                Assert.IsTrue(exception.Message.Contains("strField"));
                Assert.IsTrue(exception.Message.Contains("Should specific length of string"));
            }

            var errModel5 = new InvalidFileModel3
            {
                exceptionField = new Exception(),
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    var writer = new FixedLengthFileWriter<InvalidFileModel3>(stream);
                    writer.Write(errModel5);
                });
            }
        }

        [TestMethod]
        public void TestWrite()
        {
            var model1 = new FileModel
            {
                utf8StrField = "这是一个",
                intField = int.MaxValue,
                longField = long.MinValue,
                asciiStrField = "abcdefgh",
                intUnionField = 1,
                charField = char.MaxValue,
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.Now,
            };

            var model2 = new FileModel
            {
                utf8StrField = "测试文本",
                intField = 1,
                longField = long.MaxValue,
                asciiStrField = "abc",
                charField = char.MinValue,
                byteArrField = new byte[] { 0, 255 },
                dateTimeField = DateTime.Now,
            };

            var model3 = new FileModel
            {
                utf8StrField = "啦",
                intField = 1,
                longField = int.MinValue,
                asciiStrField = "",
                charField = '哈',
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.MinValue,
            };

            var model4 = new FileModel
            {
                utf8StrField = "啦",
                // intField = 0,
                // longField = 0,
                asciiStrField = "123",
                charField = '哈',
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.MaxValue,
            };

            var models = new FileModel[] { model1, model2, model3, model4 };

            var fileName = Path.GetTempFileName();

            using (var stream = File.OpenWrite(fileName))
            {
                var writer = new FixedLengthFileWriter<FileModel>(stream);

                foreach (var model in models)
                {
                    writer.Write(model);
                }
            }

            using (var stream = File.OpenRead(fileName))
            {
                var buffer = new byte[100];
                string utf8StrField;
                int intField;
                long longField;
                string asciiStrField;
                char charField;
                DateTime dateTimeField;

                foreach (var model in models)
                {
                    // utf8StrField: 100 
                    stream.Read(buffer, 0, 100);
                    utf8StrField = Encoding.UTF8.GetString(buffer, 0, 100).TrimEnd('\0');
                    Assert.AreEqual(model.utf8StrField, utf8StrField);

                    // longField: 8
                    stream.Read(buffer, 0, 8);
                    longField = BitConverter.ToInt64(buffer, 0);
                    Assert.AreEqual(model.longField, longField);

                    // intField: 10
                    stream.Read(buffer, 0, 10);
                    intField = BitConverter.ToInt32(buffer, 0);
                    Assert.AreEqual(model.intField, intField);

                    // asciiStrField: 8
                    stream.Read(buffer, 0, 8);
                    asciiStrField = Encoding.ASCII.GetString(buffer, 0, 8).TrimEnd('\0');
                    Assert.AreEqual(model.asciiStrField, asciiStrField);

                    // charField: 2
                    stream.Read(buffer, 0, 2);
                    charField = BitConverter.ToChar(buffer, 0);
                    Assert.AreEqual(model.charField, charField);

                    // byteField: 2
                    stream.Read(buffer, 0, 2);
                    Assert.IsTrue(buffer.Take(2).SequenceEqual(model.byteArrField));

                    // dateTimeField: 8
                    stream.Read(buffer, 0, 8);
                    dateTimeField = DateTime.FromBinary(BitConverter.ToInt64(buffer, 0));
                    Assert.AreEqual(model.dateTimeField, dateTimeField);
                }
            }
        }

        private int Utf8StrToInt(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            if (data.Length < 4)
            {
                data = data.Concat(new byte[4 - data.Length]).ToArray();
            }
            return BitConverter.ToInt32(data, 0);
        }

        [TestMethod]
        public void TestRead()
        {
            var model1 = new FileModel
            {
                utf8StrField = "这是一个",
                intField = int.MaxValue,
                longField = long.MinValue,
                asciiStrField = "abcdefgh",
                intUnionField = Utf8StrToInt("这是一个"),
                strUnionField = "bcd",
                charField = char.MaxValue,
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.Now,
            };

            var model2 = new FileModel
            {
                utf8StrField = "测试文本",
                intField = 1,
                longField = long.MaxValue,
                asciiStrField = "abc",
                intUnionField = Utf8StrToInt("测试文本"),
                strUnionField = "bc",
                charField = char.MinValue,
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.Now,
            };

            var model3 = new FileModel
            {
                utf8StrField = "啦",
                intField = 1,
                longField = int.MinValue,
                asciiStrField = "",
                intUnionField = Utf8StrToInt("啦"),
                strUnionField = "",
                charField = '哈',
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.MinValue,
            };

            var model4 = new FileModel
            {
                utf8StrField = "abcdefg",
                // intField = 0,
                // longField = 0,
                asciiStrField = "123",
                intUnionField = Utf8StrToInt("abcdefg"),
                strUnionField = "23",
                charField = '哈',
                byteArrField = new byte[] { 1, 2 },
                dateTimeField = DateTime.MaxValue,
            };

            var models = new FileModel[] { model1, model2, model3, model4 };

            var fileName = Path.GetTempFileName();

            using (var stream = File.OpenWrite(fileName))
            {
                var writer = new FixedLengthFileWriter<FileModel>(stream);

                foreach (var model in models)
                {
                    writer.Write(model);
                }
            }

            using (var stream = File.OpenRead(fileName))
            {
                var reader = new FixedLengthFileReader<FileModel>(stream);

                foreach (var model in models)
                {
                    var outModel = reader.Read();
                    Assert.AreEqual(model, outModel);
                }
            }
        }
    }
}
