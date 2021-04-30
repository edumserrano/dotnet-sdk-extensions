using System;
using System.Reflection;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class ReflectionExtensions
    {
        public static object GetInstanceField(this object instance, string fieldName)
        {
            var type = instance.GetType();
            return GetInstanceField(type, instance, fieldName);
        }

        public static T GetInstanceField<T>(this object instance, string fieldName)
        {
            var type = instance.GetType();
            return (T)GetInstanceField(type, instance, fieldName);
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            var bindFlags = BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static object GetInstanceProperty(this object instance, string propertyName)
        {
            var type = instance.GetType();
            return GetInstanceProperty(type, instance, propertyName);
        }

        public static T GetInstanceProperty<T>(this object instance, string propertyName)
        {
            var type = instance.GetType();
            return (T)GetInstanceProperty(type, instance, propertyName);
        }

        public static object GetInstanceProperty(Type type, object instance, string propertyName)
        {
            var bindFlags = BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Static;
            var property = type.GetProperty(propertyName, bindFlags);
            return property.GetValue(instance);
        }
    }
}