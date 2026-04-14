using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public sealed class DiContainer : IContainer
{
    private readonly Dictionary<Type, List<Binding>> _map = new();
    private readonly HashSet<IDisposable> _ownedDisposables = new();
    private readonly Stack<Type> _buildStack = new();
    private bool _disposed;

    private readonly DiContainer _parent;

    public DiContainer(DiContainer parent = null) => _parent = parent;

    public BindingBuilder Bind<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        return Register(typeof(TAbstraction), _ => Create(typeof(TImplementation)));
    }

    public BindingBuilder Bind<TAbstraction>(Func<IContainer, TAbstraction> factory)
    {
        return Register(typeof(TAbstraction), container => factory(container));
    }

    public BindingBuilder BindInstance<T>(T instance)
    {
        var binding = new Binding
        {
            Abstraction = typeof(T),
            Factory = _ => instance,
            Lifetime = Lifetime.Singleton,
            SingletonInstance = instance,
            IsExternInstance = true
        };
        AddBinding(binding);
        Inject(instance);

        return new BindingBuilder(binding);
    }

    private BindingBuilder Register(Type abstraction, Func<IContainer, object> factory)
    {
        Binding binding = new()
        {
            Abstraction = abstraction,
            Factory = factory,
            Lifetime = Lifetime.Transient
        };
        AddBinding(binding);
        return new BindingBuilder(binding);
    }

    private void AddBinding(Binding binding)
    {
        if (!_map.TryGetValue(binding.Abstraction, out List<Binding> list))
        {
            list = new();
            _map[binding.Abstraction] = list;
        }
        list.Add(binding);
    }

    public T Resolve<T>() => (T)Resolve(typeof(T));

    public bool TryResolve<T>(out T value)
    {
        try
        {
            value = Resolve<T>();
            return true;

        }
        catch
        {
            value = default;
            return false;
        }
    }

    public object Resolve(Type type)
    {
        EnsureNotDisposed();

        if (TryGetBinding(type, out Binding binding))
        {
            return GetFromBinding(binding);
        }

        if (!type.IsAbstract && !type.IsInterface)
        {
            return Create(type);
        }

        if (_parent != null)
        {
            return _parent.Resolve(type);
        }

        throw new InvalidOperationException($"No binding found for type {type}.");
    }

    private bool TryGetBinding(Type type, out Binding binding)
    {
        if (_map.TryGetValue(type, out List<Binding> list) && list.Count > 0)
        {
            binding = list[0];
            return true;
        }
        binding = null;
        return false;
    }

    private object GetFromBinding(Binding binding)
    {
        if (binding.Lifetime == Lifetime.Singleton)
        {
            if (binding.SingletonInstance == null)
            {
                binding.SingletonInstance = binding.Factory(this);
                TrackIfDisposable(binding.SingletonInstance, binding.IsExternInstance);

                Inject(binding.SingletonInstance);
            }
            return binding.SingletonInstance;
        }

        object instance = binding.Factory(this);
        TrackIfDisposable(instance, externInstance: false);
        Inject(instance);
        return instance;
    }

    internal object Create(Type concreteType)
    {
        EnsureNotDisposed();

        if (_buildStack.Contains(concreteType))
        {
            string cycle = string.Join(" -> ", _buildStack.Reverse().Append(concreteType).Select(type => type.Name));

            throw new InvalidOperationException($"Cyclic dependency detected: {cycle}");
        }

        _buildStack.Push(concreteType);
        try
        {
            ConstructorInfo ctor = SelectConstructor(concreteType);
            object[] args = ctor.GetParameters()
                .Select(parameterInfo => ResolveParameter(parameterInfo))
                .ToArray();

            object obj = Activator.CreateInstance(concreteType, args);
            Inject(obj);
            TrackIfDisposable(obj, externInstance: false);
            return obj;
        }
        finally
        {
            _buildStack.Pop();
        }
    }

    private static ConstructorInfo SelectConstructor(Type type)
    {
        ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Concat(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
            .ToArray();

        ConstructorInfo[] marked = ctors.Where(ctor => ctor.GetCustomAttributes(typeof(InjectAttribute), true).Any()).ToArray();

        if (marked.Length > 1)
        {
            throw new InvalidOperationException($"{type.Name} has multiple constructors marked with [Inject].");
        }
        if (marked.Length == 1)
        {
            return marked[0];
        }

        return ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault() 
            ?? throw new InvalidOperationException($"{type.Name} has no constructor.");
    }

    private object ResolveParameter(ParameterInfo parameterInfo)
    {
        try
        {
            return Resolve(parameterInfo.ParameterType);
        }
        catch (Exception)
        {
            if (parameterInfo.HasDefaultValue) return parameterInfo.DefaultValue;
            throw;
        }
    }

    public void Inject(object target)
    {
        if (target == null)
        {
            return;
        }

        Type type = target.GetType();

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (FieldInfo fieldInfo in type.GetFields(flags))
        {
            InjectAttribute inj = fieldInfo.GetCustomAttribute<InjectAttribute>();
            
            if (inj == null)
            {
                continue;
            }

            TryAssign(() => Resolve(fieldInfo.FieldType), inj.Optional, v => fieldInfo.SetValue(target, v), fieldInfo.FieldType, type, "field", fieldInfo.Name);
        }

        foreach (PropertyInfo propertyInfo in type.GetProperties(flags))
        {
            if (!propertyInfo.CanWrite)
            {
                continue;
            }

            InjectAttribute inj = propertyInfo.GetCustomAttribute<InjectAttribute>();
            
            if (inj == null)
            {
                continue;
            }

            TryAssign(() => Resolve(propertyInfo.PropertyType), inj.Optional, v => propertyInfo.SetValue(target, v), propertyInfo.PropertyType, type, "property", propertyInfo.Name);
        }
    }

    private static void TryAssign(Func<object> resolver, bool optional, Action<object> setter, Type depType, Type ownerType, string kind, string member)
    {
        try
        {
            setter(resolver());
        }
        catch
        {
            if (!optional)
            {
                throw new InvalidOperationException($"{ownerType.Name}.{member} ({kind}) requires {depType.Name}, but it couldn't be resolved.");
            }
        }
    }

    public void InjectGameObject(GameObject root, bool includeInactive = true)
    {
        if (root == null)
        {
            return;
        }

        foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(includeInactive))
        {
            if (mb == null)
            {
                continue;
            }

            Inject(mb);
        }
    }

    public IContainer CreateChildScope() => new DiContainer(this);

    private void TrackIfDisposable(object obj, bool externInstance)
    {
        if (externInstance)
        {
            return;
        }
        if (obj is IDisposable disposable)
        {
            _ownedDisposables.Add(disposable);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (IDisposable disposable in _ownedDisposables.Reverse())
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        _ownedDisposables.Clear();
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DiContainer));
        }
    }
}