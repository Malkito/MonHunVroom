# Service Locator

Adapted from https://github.com/adammyhre/Unity-Service-Locator.

Add `ServiceLocator` with `ServiceLocatorScene` to a scene root, or `ServiceLocator` with `ServiceLocatorGlobal` for global services. Add `ServiceLocator` to a GameObject for local services.

```csharp
ServiceLocator.For(this).Register<IMyService>(service);
ServiceLocator.For(this).TryGet<IMyService>(out var service);
```

`For(this)` checks the GameObject hierarchy, then the scene locator, then the global locator. `Get<T>()` throws when no service is registered; `TryGet<T>()` returns false.
