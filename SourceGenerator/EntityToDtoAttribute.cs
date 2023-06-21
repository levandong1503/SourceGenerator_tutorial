using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EntityToDtoAttribute : Attribute
    {
        public Type EntityType { get; }

        public EntityToDtoAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }
}
