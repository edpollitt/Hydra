using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using Nerdle.Hydra.InfrastructureAbstractions;
using Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowStateManagerTests.Helpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    abstract class _on_a_dynamic_cluster_of<T>
    {
        protected const string Primary = "primary";
        protected const string Secondary = "secondary";
        protected const string Tertiary = "tertiary";

        protected IDictionary<string, Mock<IFailable<T>>> Components;

        protected ReaderWriterLockSlim RwLock;
        protected CountingSyncManagerProxy SyncManagerProxy;
        protected DynamicCluster<T> Sut;

        [SetUp]
        public void BeforeEach()
        {
            RwLock = new ReaderWriterLockSlim();
            SyncManagerProxy = new CountingSyncManagerProxy(new SyncManager(RwLock, TimeSpan.Zero));

            Components = new[] { Primary, Secondary, Tertiary }
                .Select(id =>
                {
                    var component = new Mock<IFailable<T>>();
                    component.Setup(c => c.ComponentId).Returns(id);
                    return component;
                })
                .ToDictionary(component => component.Object.ComponentId, component => component);

            Sut = new DynamicCluster<T>(Components.Values.Select(mock => mock.Object), SyncManagerProxy);
        }
    }
}