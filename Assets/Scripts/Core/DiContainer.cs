using System;
using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Core
{
    public class DiContainer
    {
        private readonly Dictionary<Type, object> _bindings = new();

        public void Register<T>(T instance)
        {
            _bindings[typeof(T)] = instance;
        }

        public T Resolve<T>()
        {
            Type type = typeof(T);

            if (_bindings.TryGetValue(type, out object instance))
                return (T)instance;

            Debug.LogError($"[DiContainer] Тип не зарегистрирован: {type.Name}");
            throw new InvalidOperationException($"[DiContainer] Тип не зарегистрирован: {type.Name}");
        }

        public T TryResolve<T>() where T : class
        {
            _bindings.TryGetValue(typeof(T), out object instance);
            return instance as T;
        }
    }
}
