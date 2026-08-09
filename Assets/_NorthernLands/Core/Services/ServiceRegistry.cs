using System;
using System.Collections.Generic;

namespace NorthernLands.Core.Services
{
    /// <summary>
    /// Small explicit service container. It deliberately avoids scene-object searches
    /// so dependencies remain visible and testable.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly List<IGameService> _lifecycleOrder = new();

        public void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceType = typeof(T);
            if (_services.ContainsKey(serviceType))
                throw new InvalidOperationException($"Service {serviceType.Name} is already registered.");

            _services.Add(serviceType, service);
            if (service is IGameService gameService)
                _lifecycleOrder.Add(gameService);
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new KeyNotFoundException($"Service {typeof(T).Name} is not registered.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var value))
            {
                service = (T)value;
                return true;
            }

            service = null;
            return false;
        }

        public void InitializeAll()
        {
            foreach (var service in _lifecycleOrder)
                service.Initialize();
        }

        public void ShutdownAll()
        {
            for (var index = _lifecycleOrder.Count - 1; index >= 0; index--)
                _lifecycleOrder[index].Shutdown();

            _lifecycleOrder.Clear();
            _services.Clear();
        }
    }
}
