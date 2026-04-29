using System;
using System.Collections.Generic;

public sealed class SaveContext
{
    private readonly Dictionary<Type, object> _servicesByType = new();

    public void Register(object service)
    {
        if (service == null)
        {
            return;
        }

        _servicesByType[service.GetType()] = service;
    }

    public bool TryGet<T>(out T service) where T : class
    {
        foreach (object candidate in _servicesByType.Values)
        {
            if (candidate is T typed)
            {
                service = typed;
                return true;
            }
        }

        service = null;
        return false;
    }
}
