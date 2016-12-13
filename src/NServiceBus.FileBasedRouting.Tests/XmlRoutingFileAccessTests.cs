namespace NServiceBus.FileBasedRouting.Tests
{
    using System;
    using System.IO;
    using System.Xml;
    using NUnit.Framework;

    public class XmlRoutingFileAccessTests
    {
        [Test]
        public void Should_throw_when_file_not_found()
        {
            var fileAccess = new XmlRoutingFileAccess("non-existing.file");

            Assert.Throws<FileNotFoundException>(() => fileAccess.Read());
        }

        [Test]
        public void Should_return_loaded_document()
        {
            var fileName = "hello-world.xml";
            var fileAccess = new XmlRoutingFileAccess(fileName);

            File.WriteAllText(fileName, "<greeting>Hello World!</greeting>");
            try
            {
                var document = fileAccess.Read();
                Assert.IsNotNull(document);
                Assert.AreEqual("Hello World!", document.Root.Value);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Test]
        public void Should_load_valid_xml_content()
        {
            var fileName = "hello-world.html";
            var fileAccess = new XmlRoutingFileAccess(fileName);

            File.WriteAllText(fileName, "<h1>Hello World!</h1>");
            try
            {
                var document = fileAccess.Read();
                Assert.IsNotNull(document);
                Assert.AreEqual("Hello World!", document.Root.Value);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Test]
        public void Should_throw_when_file_contains_no_xml_content()
        {
            var fileName = "hello-world.txt";
            var fileAccess = new XmlRoutingFileAccess(fileName);

            File.WriteAllText(fileName, "Hello World!");
            try
            {

                var exception = Assert.Throws<Exception>(() => fileAccess.Read());

                StringAssert.Contains("The configured routing file is no valid XML file.", exception.Message);
                Assert.IsInstanceOf<XmlException>(exception.InnerException);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}