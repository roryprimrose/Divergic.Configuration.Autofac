namespace Divergic.Configuration.Autofac
{
    using System.Reflection;
    using global::Autofac;
    using Module = global::Autofac.Module;

    public class ConfigurationModule<T> : Module
        where T : IConfigurationResolver, new()
    {
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
            var properties = configuration.GetType().GetTypeInfo().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(configuration);

                if (value == null)
                {
                    continue;
                }

                var configType = value.GetType();

                if (configType.GetTypeInfo().IsValueType)
                {
                    // Skip value types
                    continue;
                }

                if (configType == typeof(string))
                {
                    // Skip strings which are not value types
                    continue;
                }

                builder.RegisterInstance(value).AsImplementedInterfaces();

                // Recurse into the child properties
                RegisterConfigTypes(builder, value);
            }
        }
    }
}