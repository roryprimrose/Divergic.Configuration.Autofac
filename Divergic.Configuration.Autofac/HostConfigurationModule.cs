namespace Divergic.Configuration.Autofac
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using global::Autofac;
    using Microsoft.Extensions.Configuration;
    using Module = global::Autofac.Module;

    /// <summary>
    /// The <see cref="HostConfigurationModule{T}"/>
    /// class is used to register configuration types based on configuration already configured in the host application via <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register in the container.</typeparam>
    public class HostConfigurationModule<T> : Module
    {
        /// <summary>
        /// Gets whether the specified type should be resolved and registered in the Autofac container.
        /// </summary>
        /// <param name="configType">The type to evaluate.</param>
        /// <returns><c>true</c> if the type should be registered in Autofac; otherwise <c>false</c>.</returns>
        protected virtual bool IsSupportedType(Type configType)
        {
            if (configType.IsValueType)
            {
                // Skip value types
                return false;
            }

            if (configType == typeof(string))
            {
                // Skip strings
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            var configType = typeof(T);

            if (configType.IsValueType)
            {
                return;
            }

            if (configType == typeof(string))
            {
                return;
            }

            builder.Register(c =>
            {
                var config = c.Resolve<IConfiguration>();

                var value = (T)config.Get(configType);

                AssignEnvironmentOverride(value);

                return value;
            }).AsSelf().AsImplementedInterfaces();

            RegisterConfigTypes(builder, configType);
        }

        private static void AssignEnvironmentOverride(object configuration)
        {
            if (configuration == null)
            {
                return;
            }

            var properties = configuration.GetType().GetProperties();

            for (var index = 0; index < properties.Length; index++)
            {
                var property = properties[index];

                if (property.CanWrite == false)
                {
                    continue;
                }

                AssignEnvironmentOverride(configuration, property);
            }
        }

        private static void AssignEnvironmentOverride(object configuration, PropertyInfo property)
        {
            // Check if there is an environment variable override defined on the property
            var attribute = property.GetCustomAttributes().OfType<EnvironmentOverrideAttribute>().FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            var key = attribute.Variable;

            AssignEnvironmentVariable(configuration, property, key);
        }

        private static void AssignEnvironmentVariable(object configuration, PropertyInfo property, string key)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (value == null)
            {
                return;
            }

            try
            {
                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                object converted;

                if (converter.CanConvertFrom(typeof(string)))
                {
                    converted = converter.ConvertFromString(value);
                }
                else
                {
                    // Try a straight change type
                    // Attempt to convert the value to the target type
                    converted = Convert.ChangeType(value, property.PropertyType);
                }

                // If we got this far then the conversion from the environment variable string was ok to the target property type
                // Set the value back on the property
                property.SetValue(configuration, converted);
            }
            catch (Exception)
            {
                // We don't want to break the application if we don't have good data to play with
            }
        }

        private void RegisterConfigTypes(
            ContainerBuilder builder,
            Type parentType)
        {
            var configType = parentType;

            // Register all the properties of the configuration as their interfaces This must be done
            // after registering assembly types and modules because type scanning may have already
            // registered the configuration classes as their interfaces which means Autofac will
            // return the default classes rather than these configuration instances that have values populated.
            var properties = configType.GetProperties();

            foreach (var property in properties)
            {
                // A property with a private get has CanRead return true
                if (property.CanRead == false || property.GetMethod?.IsPublic == false)
                {
                    // Ignore any property that we can't read
                    continue;
                }

                var isSupportedType = IsSupportedType(property.PropertyType);

                if (isSupportedType == false)
                {
                    // We don't support registrations of this type
                    continue;
                }

                var parameters = property.GetIndexParameters();

                if (parameters.Length > 0)
                {
                    // This is an indexer property which we don't support
                    // Skip over it and continue to find property values that we can try to register
                    continue;
                }

                try
                {
                    var interfaces = property.PropertyType.GetInterfaces();

                    builder.Register(c =>
                    {
                        var config = c.Resolve(configType);

                        var value = property.GetValue(config);

                        AssignEnvironmentOverride(value);

                        return value;
                    }).As(property.PropertyType).As(interfaces);
                }
                catch (Exception)
                {
                    // We failed to read the property so we can't process it
                    // We also don't want to crash the application so ignore this failure
                    continue;
                }

                // Recurse into the child properties
                RegisterConfigTypes(builder, property.PropertyType);
            }
        }
    }
}