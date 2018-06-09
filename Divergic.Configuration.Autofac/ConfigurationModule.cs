namespace Divergic.Configuration.Autofac
{
    using global::Autofac;
    using Module = global::Autofac.Module;

    /// <summary>
    /// The <see cref="ConfigurationModule{T}"/>
    /// class provides Autofac registration support for nested configuration types.
    /// </summary>
    /// <typeparam name="T">The type of resolver that provides the root configuration.</typeparam>
    public class ConfigurationModule<T> : Module
        where T : IConfigurationResolver, new()
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            var resolver = new T();
            var config = resolver.Resolve();

            RegisterConfigTypes(builder, config);

            builder.RegisterInstance(config).AsImplementedInterfaces();
        }

        private static void RegisterConfigTypes(ContainerBuilder builder, object configuration)
        {
            if (configuration == null)
            {
                return;
            }

            // Register all the properties of the configuration as their interfaces This must be done
            // after registering assembly types and modules because type scanning may have already
            // registered the configuration classes as their interfaces which means Autofac will
            // return the default classes rather than these configuration instances that have values populated.
            var properties = configuration.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType.IsValueType)
                {
                    // Skip value types
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    // Skip strings
                    continue;
                }

                var value = property.GetValue(configuration);

                if (value == null)
                {
                    // There is no value on the property, skip to the next one
                    continue;
                }

                builder.RegisterInstance(value).AsImplementedInterfaces();

                // Recurse into the child properties
                RegisterConfigTypes(builder, value);
            }
        }
    }
}