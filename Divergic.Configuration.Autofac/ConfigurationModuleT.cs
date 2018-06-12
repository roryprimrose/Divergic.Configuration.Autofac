namespace Divergic.Configuration.Autofac
{
    /// <summary>
    /// The <see cref="ConfigurationModule{T}"/>
    /// class provides Autofac registration support for nested configuration types.
    /// </summary>
    /// <typeparam name="T">The type of resolver that provides the root configuration.</typeparam>
    public class ConfigurationModule<T> : ConfigurationModule
        where T : IConfigurationResolver, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationModule{T}"/>.
        /// </summary>
        public ConfigurationModule()
            : base(new T())
        {
        }
    }
}