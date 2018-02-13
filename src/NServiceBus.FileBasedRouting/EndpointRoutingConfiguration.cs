using System;

namespace NServiceBus.FileBasedRouting
{
    class EndpointRoutingConfiguration
    {
        public string Dennis { get; set; }

        public string LogicalEndpointName { get; set; }

        public Type[] Commands { get; set; }

        public Type[] Events { get; set; }
    }
}