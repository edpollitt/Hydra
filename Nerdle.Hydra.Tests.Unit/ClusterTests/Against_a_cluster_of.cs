using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.ClusterTests
{
    abstract class Against_a_cluster_of<T>
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
                    component.Setup(c => c.Id).Returns((string) id);
                    return component;
                })
                .ToDictionary(component => component.Object.Id, component => component);

            Sut = new Cluster<T>(Components.Values.Select(mock => mock.Object));
        }
    }
}