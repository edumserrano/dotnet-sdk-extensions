using System;
using System.Reflection;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class ReflectionExtensions
    {
        public static object? GetInstanceField(this object instance, string fieldName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();
            return instance.GetInstanceField(type, fieldName);
        }

        // pass in the type if you need to access a field that is not from the type instance.GetType()
        // but from the base/derived class for instance
        public static object? GetInstanceField(this object instance, Type type, string fieldName)
        {
            return GetInstanceField(type, instance, fieldName);
        }

        public static T? GetInstanceField<T>(this object instance, string fieldName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();
            return instance.GetInstanceField<T>(type, fieldName);
        }

        // pass in the type if you need to access a field that is not from the type instance.GetType()
        // but from the base/derived class for instance
        public static T? GetInstanceField<T>(this object instance, Type type, string fieldName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return (T?)GetInstanceField(type, instance, fieldName);
        }

        public static object? GetInstanceField(Type type, object instance, string fieldName)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            const BindingFlags bindFlags = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            if (field is null)
            {
                throw new InvalidOperationException($"GetInstanceField: field {fieldName} does not exist.");
            }

            return field.GetValue(instance);
        }

        public static object? GetInstanceProperty(this object instance, string propertyName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();
            return instance.GetInstanceProperty(type, propertyName);
        }

        // pass in the type if you need to access a field that is not from the type instance.GetType()
        // but from the base/derived class for instance
        public static object? GetInstanceProperty(this object instance, Type type, string propertyName)
        {
            return GetInstanceProperty(type, instance, propertyName);
        }

        public static T? GetInstanceProperty<T>(this object instance, string propertyName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();
            return instance.GetInstanceProperty<T>(type, propertyName);
        }

        // pass in the type if you need to access a field that is not from the type instance.GetType()
        // but from the base/derived class for instance
        public static T? GetInstanceProperty<T>(this object instance, Type type, string propertyName)
        {
            return (T?)GetInstanceProperty(type, instance, propertyName);
        }

        public static object? GetInstanceProperty(Type type, object instance, string propertyName)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            const BindingFlags bindFlags = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static;
            var property = type.GetProperty(propertyName, bindFlags);
            if (property is null)
            {
                throw new InvalidOperationException($"GetInstanceProperty: property {propertyName} does not exist.");
            }

            return property.GetValue(instance);
        }
    }
}
