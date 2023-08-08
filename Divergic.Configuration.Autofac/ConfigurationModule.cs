namespace Divergic.Configuration.Autofac
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using global::Autofac;
    using Module = global::Autofac.Module;

    /// <summary>
    /// The <see cref="ConfigurationModule"/>
    /// class is used to register nested configuration types.
    /// </summary>
    public class ConfigurationModule : Module
    {
        private readonly IConfigurationResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationModule"/>.
        /// </summary>
        /// <param name="resolver">The configuration resolver.</param>
        public ConfigurationModule(IConfigurationResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = _resolver.Resolve();

            if (configuration == null)
            {
                return;
            }

            var configType = configuration.GetType();

            if (configType.IsValueType)
            {
                return;
            }

            if (configType == typeof(string))
            {
                return;
            }

            var referenceTracker = new List<object>();

            RegisterConfigTypes(builder, configuration, referenceTracker);
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

        private static void RegisterConfigTypes(
            ContainerBuilder builder,
            object configuration,
            ICollection<object> referenceTracker)
        {
            if (configuration == null)
            {
                return;
            }

            if (referenceTracker.Any(x => ReferenceEquals(configuration, x)))
            {
                // We found a circular reference
                return;
            }

            var configType = configuration.GetType();

            if (configType.IsValueType)
            {
                // Skip value types
                return;
            }

            if (configType == typeof(string))
            {
                // Skip strings
                return;
            }

            if (configType.GetInterfaces().Any())
            {
                builder.RegisterInstance(configuration).AsImplementedInterfaces();
            }

            builder.RegisterInstance(configuration).AsSelf();

            referenceTracker.Add(configuration);

            // Register all the properties of the configuration as their interfaces This must be done
            // after registering assembly types and modules because type scanning may have already
            // registered the configuration classes as their interfaces which means Autofac will
            // return the default classes rather than these configuration instances that have values populated.
            var properties = configuration.GetType().GetProperties();

            foreach (var property in properties)
            {
                // A property with a private get has CanRead return true
                if (property.CanRead == false || property.GetMethod?.IsPublic == false)
                {
                    // Ignore any property that we can't read
                    continue;
                }

                var parameters = property.GetIndexParameters();

                if (parameters.Length > 0)
                {
                    // This is an indexer property which we don't support
                    // Skip over it and continue to find property values that we can try to register
                    continue;
                }

                object value;

                try
                {
                    value = property.GetValue(configuration);
                }
                catch (Exception)
                {
                    // We failed to read the property so we can't process it
                    // We also don't want to crash the application so ignore this failure
                    continue;
                }

                if (property.CanWrite)
                {
                    // Attempt to assign the property value to an environment variable identified by the attribute
                    AssignEnvironmentOverride(configuration, property);

                    if (value is string stringValue)
                    {
                        // The value of the property may point to an environment variable
                        // Attempt to assign the property value to the environment variable
                        AssignEnvironmentVariable(configuration, property, stringValue);
                    }
                }

                // Recurse into the child properties
                RegisterConfigTypes(builder, value, referenceTracker);
            }
        }
    }
}