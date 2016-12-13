using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NServiceBus.Routing;

namespace NServiceBus.FileBasedRouting
{
    class UnicastSubscriberTable
    {
        static readonly UnicastRouteGroup[] emptyResult =
        {
        };

        public UnicastRouteGroup[] GetRoutesFor(Type messageType)
        {
            UnicastRouteGroup[] unicastRoutes;
            return routeTable.TryGetValue(messageType, out unicastRoutes)
                ? unicastRoutes
                : emptyResult;
        }

        public void AddOrReplaceRoutes(string sourceKey, IList<RouteTableEntry> entries)
        {
            // The algorithm uses ReaderWriterLockSlim. First entries are read. If then exists they are compared with passed entries and skipped if equal.
            // Otherwise, the write path is used. It's possible than one thread will execute all the work
            var existing = GetExistingRoutes(sourceKey);
            if (existing != null && existing.SequenceEqual(entries))
            {
                return;
            }

            readerWriterLock.EnterWriteLock();
            try
            {
                routeGroups[sourceKey] = entries;
                routeTable = CalculateNewRouteTable();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        IList<RouteTableEntry> GetExistingRoutes(string sourceKey)
        {
            IList<RouteTableEntry> existing;
            readerWriterLock.EnterReadLock();
            try
            {
                routeGroups.TryGetValue(sourceKey, out existing);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
            return existing;
        }

        Dictionary<Type, UnicastRouteGroup[]> CalculateNewRouteTable()
        {
            var newRouteTable = new Dictionary<Type, List<UnicastRoute>>();
            foreach (var entry in routeGroups.Values.SelectMany(g => g))
            {
                List<UnicastRoute> typeRoutes;
                if (!newRouteTable.TryGetValue(entry.MessageType, out typeRoutes))
                {
                    typeRoutes = new List<UnicastRoute>();
                    newRouteTable[entry.MessageType] = typeRoutes;
                }
                typeRoutes.Add(entry.Route);
            }
            return newRouteTable.ToDictionary(kvp => kvp.Key, kvp => GroupByEndpoint(kvp.Value));
        }

        static UnicastRouteGroup[] GroupByEndpoint(List<UnicastRoute> routes)
        {
            return routes.GroupBy(r => r.Endpoint)
                .Select(g => new UnicastRouteGroup(g.Key, g.ToArray()))
                .ToArray();
        }

        volatile Dictionary<Type, UnicastRouteGroup[]> routeTable = new Dictionary<Type, UnicastRouteGroup[]>();
        Dictionary<string, IList<RouteTableEntry>> routeGroups = new Dictionary<string, IList<RouteTableEntry>>();
        ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
    }
}