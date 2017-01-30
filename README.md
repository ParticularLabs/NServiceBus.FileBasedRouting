# NServiceBus.FileBasedRouting
An alternative approach to configure message routing information and event subscribers via XML file. The routing file can be configured once and then shared across all endpoints for easier routing and publish/subscribe management.


## Configuration

Enable the file-based routing via routing configuration:

```
var transport = endpointConfiguration.UseTransport<MsmqTransport>();
var routing = transport.Routing();
transport.Routing().UseFileBasedRouting();
```

This will configure the endpoint to look for a `endpoints.xml` file in the endpoints [base directory](https://msdn.microsoft.com/en-us/library/system.appdomain.basedirectory(v=vs.110).aspx).

The routing file path can be configured using relative or absolute paths, e.g.:

```
transport.Routing().UseFileBasedRouting(@"..\..\..\endpoints.xml");
```

It is also possible to provide an `Uri` to the routing file:

```
transport.Routing().UseFileBasedRouting(new Uri("https://your-server.com/endpoints.xml"));
```
    
The routing file provides routing information like shown in the following example:
    
Create a new XML file named `endpoints.xml` and include it on every endpoint using file based routing. 

```
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
```

The `type` attribute needs to provide the [Assembly Qualified Type Name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname(v=vs.110).aspx).
Make sure that the routing file is copied to the binaries output folder.

Instead of defining every single message type, it's also possible to configure entire assemblies or namespaces in bulk:

```
<endpoints>
  <endpoint name="endpointA">
    <handles>
      <!-- Define that endpointA can handle all commands in assembly "MyApp.Contracts" -->
      <commands assembly="MyApp.Contracts" />
      <!-- Define that endpointA can handle all events in assembly "MyApp.Contracts" within the namespace "Events" -->
      <events assembly="MyApp.Contracts" namespace="Events" />
    </handles>
  </endpoint>
</endpoints>
```


### Updating the routing configuration

The routing configuration is read every 30 seconds. You can therefore change the routing at runtime (e.g. unsubscribe an endpoint by removing its `event` entry from the `handles` collection). If the routing file is no longer valid after an update, the endpoint continues to use the previously loaded routing file.


### Sharing the routing file

In order to allow centralized configuration of the routing file, the file needs to be shared with the endpoints. This can be done in various ways, e.g.
* Make the file available via a shared network folder.
* Distribute the file as a part of your deployment process.
* Include the file on your project/solution and its build artifacts. Note: This approach does not allow for a centralized routing file management out of the box.


## Scaling out

It's possible to use sender-side distribution to scale out messages and events to multiple instances of the same logical endpoint. This is done with the instance mapping file documented here:https://docs.particular.net/nservicebus/msmq/routing?version=Core_6

In short: create a new xml file named `instance-mapping.xml` and include it on every endpoint. Make sure to copy the file over to the binaries folder.

```
<endpoints>
  <endpoint name="endpointB">
    <instance discriminator="1" machine="machine1" />
    <instance discriminator="2" machine="machine1" />
    <instance machine="machine2" />
  </endpoint>
</endpoints>
```
