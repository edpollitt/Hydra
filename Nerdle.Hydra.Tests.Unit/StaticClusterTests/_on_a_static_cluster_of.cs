using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StaticClusterTests
{
    abstract class _on_a_static_cluster_of<T>
    {
        protected const string Primary = "primary";
        protected const string Secondary = "secondary";
        protected const string Tertiary = "tertiary";

        protected IDictionary<string, Mock<IFailable<T>>> Components;

        protected Cluster<T> Sut;

        [SetUp]
        public void BeforeEach()
        {
            Components = new[] { Primary, Secondary, Tertiary }
                .Select(id =>
                {
                    var component = new Mock<IFailable<T>>();
                    component.Setup(c => c.ComponentId).Returns(id);
                    return component;
                })
                .ToDictionary(component => component.Object.ComponentId, component => component);

            Sut = new StaticCluster<T>(Components.Values.Select(mock => mock.Object));
        }
    }
}