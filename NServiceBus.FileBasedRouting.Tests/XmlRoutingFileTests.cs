using System.Linq;
using System.Xml.Linq;
using NServiceBus.FileBasedRouting.Tests.Contracts;
using NServiceBus.FileBasedRouting.Tests.Contracts.Namespace;
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
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
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
        public void It_can_parse_file_with_single_events()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B) }, configuration.Events);
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
        public void It_can_parse_file_with_events_with_assembly_only()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <events assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(C) }, configuration.Events);
        }

        [Test]
        public void It_can_parse_file_with_commands_with_assembly_and_namespace()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <commands assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
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
        public void It_can_parse_file_with_events_with_assembly_and_namespace()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <events assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B) }, configuration.Events);
        }

        [Test]
        public void It_provides_distinct_result_even_when_commands_are_registered_multiple_times()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <command type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <commands assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
      <commands assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
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
        public void It_provides_distinct_result_even_when_events_are_registered_multiple_times()
        {
            const string xml = @"
 <endpoints>
  <endpoint name=""EndpointName"">
    <handles>
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.A, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <event type = ""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace.B, NServiceBus.FileBasedRouting.Tests.Contracts"" />
      <events assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
      <events assembly = ""NServiceBus.FileBasedRouting.Tests.Contracts"" namespace=""NServiceBus.FileBasedRouting.Tests.Contracts.Namespace"" />
    </handles>
  </endpoint>
 </endpoints>
 ";
            var configurations = GetConfigurations(xml);

            Assert.AreEqual(1, configurations.Length);
            var configuration = configurations[0];
            Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

            CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B) }, configuration.Events);
        }


        static EndpointRoutingConfiguration[] GetConfigurations(string xml)
        {
            var result = new XmlRoutingFileParser(XDocument.Parse(xml));
            return result.Read().ToArray();
        }
    }
}