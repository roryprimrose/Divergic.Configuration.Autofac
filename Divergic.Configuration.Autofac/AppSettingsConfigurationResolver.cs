namespace Divergic.Configuration.Autofac
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="AppSettingsConfigurationResolver{T}"/>
    /// class provides configuration support for loading the configuration from an appsettings.json file.
    /// </summary>
    /// <typeparam name="T">The type of class to create from the configuration file.</typeparam>
    public class AppSettingsConfigurationResolver<T> : IConfigurationResolver
    {
        /// <inheritdoc />
        public object Resolve()
        {
            // Add the configuration support
            var builder = CreateBuilder();

            var configurationRoot = builder
                .Build();

            var config = configurationRoot.Get(ConfigType);

            return config;
        }

        /// <summary>
        /// Creates the configuration builder.
        /// </summary>
        /// <returns>The configuration builder.</returns>
        protected virtual IConfigurationBuilder CreateBuilder()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);
        }

        /// <inheritdoc />
        public Type ConfigType => typeof(T);
    }
}