using System;
using System.Collections.Generic;

namespace UD_WForms.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _services[typeof(T)] = service;
        }

        public static T GetService<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                return (T)_services[typeof(T)];
            }

            // Если сервис не зарегистрирован, создаем экземпляр по умолчанию
            try
            {
                var service = Activator.CreateInstance<T>();
                Register(service);
                return service;
            }
            catch
            {
                throw new InvalidOperationException($"Сервис {typeof(T).Name} не зарегистрирован и не может быть создан автоматически");
            }
        }

        public static bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}