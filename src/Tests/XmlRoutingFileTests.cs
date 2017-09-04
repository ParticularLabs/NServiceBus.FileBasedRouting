using System.Linq;
using NUnit.Framework;
using System.Xml.Linq;
using NServiceBus.FileBasedRouting;
using Tests.Contracts;
using Tests.Contracts.Commands;

public class XmlRoutingFileTests
{
    [Test]
    public void It_can_parse_file_with_single_commands()
    {
        const string xml = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <command type = ""Tests.Contracts.Commands.A, Tests.Contracts"" />
    <command type = ""Tests.Contracts.Commands.B, Tests.Contracts"" />
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
    <commands assembly = ""Tests.Contracts"" />
</handles>
</endpoint>
</endpoints>
";
        var configurations = GetConfigurations(xml);

        Assert.AreEqual(1, configurations.Length);
        var configuration = configurations[0];
        Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

        CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(C), typeof(D) }, configuration.Commands);
    }

    [Test]
    public void It_can_parse_file_with_commands_with_assembly_and_namespace()
    {
        const string xml = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <commands assembly = ""Tests.Contracts"" namespace=""Tests.Contracts.Commands"" />
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
    public void It_can_parse_file_with_commands_with_assembly_and_empty_namespace()
    {
        const string xml = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <commands assembly = ""Tests.Contracts"" namespace="""" />
</handles>
</endpoint>
</endpoints>
";
        var configurations = GetConfigurations(xml);

        Assert.AreEqual(1, configurations.Length);
        var configuration = configurations[0];
        Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);

        CollectionAssert.AreEquivalent(new[] { typeof(D) }, configuration.Commands);
    }

    [Test]
    public void It_provides_distinct_result_even_when_types_are_registered_multiple_times()
    {
        const string xml = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <command type = ""Tests.Contracts.Commands.A, Tests.Contracts"" />
    <command type = ""Tests.Contracts.Commands.A, Tests.Contracts"" />
    <command type = ""Tests.Contracts.Commands.B, Tests.Contracts"" />
    <command type = ""Tests.Contracts.Commands.B, Tests.Contracts"" />
    <commands assembly = ""Tests.Contracts"" namespace=""Tests.Contracts.Commands"" />
    <commands assembly = ""Tests.Contracts"" namespace=""Tests.Contracts.Commands"" />
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
    public void It_does_not_throw_if_assembly_cannot_be_found()
    {
        const string xml = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <commands assembly = ""FooBar"" />
</handles>
</endpoint>
</endpoints>
";
        var configurations = GetConfigurations(xml);

        Assert.AreEqual(1, configurations.Length);
        var configuration = configurations[0];
        Assert.AreEqual("EndpointName", configuration.LogicalEndpointName);
    }


    static EndpointRoutingConfiguration[] GetConfigurations(string xml)
    {
        var result = new XmlRoutingFileParser();
        return result.Parse(XDocument.Parse(xml)).ToArray();
    }
}
