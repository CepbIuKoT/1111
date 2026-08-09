using System;
using NorthernLands.Core.Services;
using NUnit.Framework;

namespace NorthernLands.Tests.EditMode
{
    public sealed class ServiceRegistryTests
    {
        [Test]
        public void RegisteredServiceCanBeResolved()
        {
            var registry = new ServiceRegistry();
            var service = new FakeService();

            registry.Register(service);

            Assert.That(registry.Get<FakeService>(), Is.SameAs(service));
        }

        [Test]
        public void DuplicateServiceRegistrationIsRejected()
        {
            var registry = new ServiceRegistry();
            registry.Register(new FakeService());

            Assert.Throws<InvalidOperationException>(() => registry.Register(new FakeService()));
        }

        private sealed class FakeService
        {
        }
    }
}
