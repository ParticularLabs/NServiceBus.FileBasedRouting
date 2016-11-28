using System.IO;
using System.Linq;
using System.Text;
using NServiceBus.FileBasedRouting.Tests.Contracts;
using NServiceBus.FileBasedRouting.Tests.Contracts.Commands;
using NUnit.Framework;

namespace NServiceBus.FileBasedRouting.Tests
{
    public class XmlRoutingFileTests
    {
        [Test]
        public void It_can_parse_file_with_single_commands()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Commands.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Commands.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B) }, configuration.Commands);
        }

        [Test]
        public void It_can_parse_file_with_commands_with_assembly_only()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <commands assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(C) }, configuration.Commands);
        }

        [Test]
        public void It_can_parse_file_with_commands_with_assembly_and_namespace()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <commands assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Commands"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B) }, configuration.Commands);
        }


        static EndpointRoutingConfiguration[] GetConfigurations(string xml)
        {
            var result = new XmlRoutingFile(() =>
            {
                var ms = new MemoryStream();
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.Write(xml);
                    writer.Flush();
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            });

            return result.Read().ToArray();
        }
    }
}