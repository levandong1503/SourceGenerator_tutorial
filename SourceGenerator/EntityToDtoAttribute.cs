using System;

namespace SourceGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityToDtoAttribut<T> : Attribute
{
    public Type EntityType => typeof(T);
}