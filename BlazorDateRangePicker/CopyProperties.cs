using System;
using System.Reflection;

namespace BlazorDateRangePicker
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// https://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        internal static void CopyProperties<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : IConfigurableOptions where TDestination : IConfigurableOptions
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate the Properties of the source instance and  
            // populate them from their desination counterparts  
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }

                // Skip properties passed to object directly in the markup
                if (GetDefaultValue(targetProperty.PropertyType) == null && targetProperty.GetValue(destination) != null)
                {
                    continue;
                }

                // Passed all tests, lets set the value
                targetProperty.SetValue(destination, srcProp.GetValue(source));
            }
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
