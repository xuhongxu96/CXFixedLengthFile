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
                var writer = new FixedLengthFileWriter(stream);
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
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
                var writer = new FixedLengthFileWriter(stream);
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
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
                var writer = new FixedLengthFileWriter(stream);
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
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
                var writer = new FixedLengthFileWriter(stream);
                var exception = Assert.ThrowsException<InvalidDataException>(() =>
                {
                    writer.Write(errModel4);
                });
                Assert.IsTrue(exception.Message.Contains("strField"));
                Assert.IsTrue(exception.Message.Contains("Should specific length of string field"));
            }

            var errModel5 = new InvalidFileModel3
            {
                exceptionField = new Exception(),
            };

            using (var stream = File.OpenWrite(Path.GetTempFileName()))
            {
                var writer = new FixedLengthFileWriter(stream);
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    writer.Write(errModel5);
                });
            }
        }

        [TestMethod]
        public void TestWrite()
        {
            var model1 = new FileModel
            {
                utf8StrField = "����һ��",
                intField = int.MaxValue,
                longField = long.MinValue,
                asciiStrField = "abcdefgh",
                intFieldUnion = 1,
                charField = char.MaxValue,
                byteArrField = new byte[] { 1, 2 },
            };

            var model2 = new FileModel
            {
                utf8StrField = "�����ı�",
                intField = 1,
                longField = long.MaxValue,
                asciiStrField = "abc",
                charField = char.MinValue,
                byteArrField = new byte[] { 1, 2 },
            };

            var model3 = new FileModel
            {
                utf8StrField = "��",
                intField = 1,
                longField = int.MinValue,
                asciiStrField = "",
                charField = '��',
                byteArrField = new byte[] { 1, 2 },
            };

            var model4 = new FileModel
            {
                utf8StrField = "��",
                // intField = 0,
                // longField = 0,
                asciiStrField = "123",
                charField = '��',
                byteArrField = new byte[] { 1, 2 },
            };

            var models = new FileModel[] { model1, model2, model3, model4 };

            var fileName = Path.GetTempFileName();

            using (var stream = File.OpenWrite(fileName))
            {
                var writer = new FixedLengthFileWriter(stream);

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

                foreach (var model in models)
                {
                    // utf8StrField: 100 
                    stream.Read(buffer, 0, 100);
                    utf8StrField = Encoding.UTF8.GetString(buffer, 0, 100).TrimEnd('\0');
                    Assert.AreEqual(model.utf8StrField, utf8StrField);

                    // intField: 10
                    stream.Read(buffer, 0, 10);
                    intField = BitConverter.ToInt32(buffer, 0);
                    Assert.AreEqual(model.intField, intField);

                    // longField: 8
                    stream.Read(buffer, 0, 8);
                    longField = BitConverter.ToInt64(buffer, 0);
                    Assert.AreEqual(model.longField, longField);

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
                utf8StrField = "����һ��",
                intField = int.MaxValue,
                longField = long.MinValue,
                asciiStrField = "abcdefgh",
                intFieldUnion = Utf8StrToInt("����һ��"),
                charField = char.MaxValue,
                byteArrField = new byte[] {1, 2},
            };

            var model2 = new FileModel
            {
                utf8StrField = "�����ı�",
                intField = 1,
                longField = long.MaxValue,
                asciiStrField = "abc",
                intFieldUnion = Utf8StrToInt("�����ı�"),
                charField = char.MinValue,
                byteArrField = new byte[] { 1, 2 },
            };

            var model3 = new FileModel
            {
                utf8StrField = "��",
                intField = 1,
                longField = int.MinValue,
                asciiStrField = "",
                intFieldUnion = Utf8StrToInt("��"),
                charField = '��',
                byteArrField = new byte[] { 1, 2 },
            };

            var model4 = new FileModel
            {
                utf8StrField = "abcdefg",
                // intField = 0,
                // longField = 0,
                asciiStrField = "123",
                intFieldUnion = Utf8StrToInt("abcdefg"),
                charField = '��',
                byteArrField = new byte[] { 1, 2 },
            };

            var models = new FileModel[] { model1, model2, model3, model4 };

            var fileName = Path.GetTempFileName();

            using (var stream = File.OpenWrite(fileName))
            {
                var writer = new FixedLengthFileWriter(stream);

                foreach (var model in models)
                {
                    writer.Write(model);
                }
            }

            using (var stream = File.OpenRead(fileName))
            {
                var reader = new FixedLengthFileReader(stream);

                foreach (var model in models)
                {
                    var outModel = reader.Read<FileModel>();
                    Assert.AreEqual(model, outModel);
                }
            }
        }
    }
}