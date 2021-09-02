# Centralized file-based routing

## Concept 

Centralized file-based routing is an alternative approach to configure message routing information and event subscribers via an XML file. The routing file can be configured once and then shared across all endpoints for easier routing and publish/subscribe management.


## Configuration

After installing the `NServiceBus.FileBasedRouting` package, enable the feature via the routing configuration:

```c#
var transport = endpointConfiguration.UseTransport<MyTransport>();
var routing = transport.Routing();
routing.UseFileBasedRouting();
```

This will configure the endpoint to look for a `endpoints.xml` file in the endpoints [base directory](https://msdn.microsoft.com/en-us/library/system.appdomain.basedirectory.aspx).

The routing file path can be configured using relative or absolute paths, e.g.:

```c#
var transport = endpointConfiguration.UseTransport<MyTransport>();
var routing = transport.Routing();
routing.UseFileBasedRouting(@"C:\routingFile.xml");
```

It is also possible to provide an Uri to the routing file (supporting `http[s]` and `file` protocols):

```c#
var transport = endpointConfiguration.UseTransport<MyTransport>();
var routing = transport.Routing();
routing.UseFileBasedRouting(new Uri("https://myserver/routing/endpoints.xml"));
```

The routing file provides routing information as is shown in the following example:
    
Create a new XML file named `endpoints.xml` and include it on every endpoint using file based routing. 

```xml
<!-- startcode EndpointsByType -->
<endpoints>
  <endpoint name="endpointA">
    <handles>
      <!-- Define that endpointA can handle the DemoCommand command -->
      <command type="Contracts.Commands.DemoCommand, Contracts" />
      <!-- Define that endpointA can handle the DemoEvent event -->
      <event type="Contracts.Events.DemoEvent, Contracts" />
    </handles>
  </endpoint>
  <endpoint name="endpointb">
    <!-- ... -->
  </endpoint>
</endpoints>
<!--endcode-->
```

The `type` attribute needs to provide the [Assembly Qualified Type Name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx).
Make sure that the routing file is copied to the binaries output folder.

Instead of defining every single message type, it's also possible to configure entire assemblies or namespaces in bulk:

```xml
<endpoints>
  <endpoint name="endpointA">
    <handles>
      <!-- Define that endpointA can handle all commands in assembly "MyApp.Contracts" -->
      <commands assembly="MyApp.Contracts" />
      <!-- Define that endpointA can handle all events in assembly "MyApp.Contracts" within the namespace "Events" -->
      <events assembly="MyApp.Contracts"
              namespace="Events" />
    </handles>
  </endpoint>
</endpoints>
<!--endcode-->
```


### Updating the routing configuration

The routing configuration is read every 30 seconds, therefore the topology can change at runtime (e.g. unsubscribe an endpoint by removing its `event` entry from the `handles` collection). If the routing file is no longer valid after an update, the endpoint continues to use the previously loaded routing file.


### Sharing the routing file

In order to allow centralized configuration of the routing file, the file needs to be shared with the endpoints. This can be done in various ways, e.g.

 * Make the file available via a shared network folder.
 * Distribute the file as a part of deployment process.
 * Include the file in the project/solution and its build artifacts. Note: This approach does not allow for a centralized routing file management out of the box.


## Scaling out

It's possible to use sender-side distribution to scale out messages and events to multiple instances of the same logical endpoint. This is done with the [instance mapping file](/transports/msmq/routing.md).
