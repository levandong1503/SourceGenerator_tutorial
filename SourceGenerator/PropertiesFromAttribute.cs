using System;

namespace Nws.AbpSourceGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PropertiesFromAttribute<T> : Attribute
{
    public Type EntityType => typeof(T);
    public string[] Ignores { set; get; }
    public string[] Onlies { set; get; }
    public PropertiesFromAttribute(string[] ignores = null, string[] onlies = null)
    {
        this.Ignores = ignores;
        this.Onlies = onlies;
    }
}
