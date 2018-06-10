namespace Divergic.Configuration.Autofac
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="JsonConfigurationResolver{T}"/>
    /// class provides configuration support for loading the configuration from an appsettings.json file.
    /// </summary>
    /// <typeparam name="T">The type of class to create from the configuration file.</typeparam>
    public class JsonConfigurationResolver<T> : IConfigurationResolver
    {
        /// <inheritdoc />
        public object Resolve()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(JsonFilename, false, true);

            var configurationRoot = builder
                .Build();

            var config = configurationRoot.Get(typeof(T));

            return config;
        }

        /// <summary>
        /// Gets the filename to load configuration values from.
        /// </summary>
        public virtual string JsonFilename => "appsettings.json";
    }
}