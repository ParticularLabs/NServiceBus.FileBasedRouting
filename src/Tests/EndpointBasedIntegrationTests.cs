using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.FileBasedRouting;
using NServiceBus.Routing;
using NUnit.Framework;
using Tests.Contracts.Commands;

public class EndpointBasedIntegrationTests
{
    const string XmlA = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <command type = ""Tests.Contracts.Commands.A, Tests.Contracts"" />
</handles>
</endpoint>
</endpoints>
";

    const string XmlB = @"
<endpoints>
<endpoint name=""EndpointName"">
<handles>
    <command type = ""Tests.Contracts.Commands.B, Tests.Contracts"" />
</handles>
</endpoint>
</endpoints>
";

    const string EndpointName = "EndpointName";

    [Test]
    public async Task Should_read_default_file()
    {
        using (Use("endpoints.xml", XmlA))
        {
            var routing = await GetRouting(null);

            var routeA = routing(typeof(A));
            Assert.AreEqual(EndpointName, routeA.Endpoint);

            var routeB = routing(typeof(B));
            Assert.IsNull(routeB);
        }
    }

    [Test]
    public async Task Should_read_custom_file()
    {
        const string name = "custom.xml";
        using (Use(name, XmlB))
        {
            var routing = await GetRouting(name);

            var routeA = routing(typeof(A));
            Assert.IsNull(routeA);

            var routeB = routing(typeof(B));
            Assert.AreEqual(EndpointName, routeB.Endpoint);
        }
    }

    static async Task<Func<Type, UnicastRoute>> GetRouting(string filePath)
    {
        var endpointConfiguration = new EndpointConfiguration("test");
        var routing = endpointConfiguration.UseTransport<LearningTransport>().Routing();

        if (filePath == null)
        {
            routing.UseFileBasedRouting();
        }
        else
        {
            routing.UseFileBasedRouting(filePath);
        }

        await Endpoint.Create(endpointConfiguration); // to init all features
        var routingTable = endpointConfiguration.GetSettings().Get<UnicastRoutingTable>();
        var getRouteFor = typeof(UnicastRoutingTable).GetMethod("GetRouteFor",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        return t => (UnicastRoute) getRouteFor.Invoke(routingTable, new object[]
        {
            t
        });
    }

    static IDisposable Use(string fileName, string xml)
    {
        // the feature uses the relative path
        var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        File.WriteAllText(file, xml);

        return new Disposable(() => File.Delete(file));
    }

    class Disposable : IDisposable
    {
        readonly Action dispose;

        public Disposable(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            dispose();
        }
    }
}