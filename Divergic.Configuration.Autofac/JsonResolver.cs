namespace Divergic.Configuration.Autofac
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="JsonResolver{T}"/>
    /// class provides configuration support for loading the configuration from a json file.
    /// </summary>
    /// <typeparam name="T">The type of class to create from the configuration file.</typeparam>
    public class JsonResolver<T> : IConfigurationResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResolver{T}"/>.
        /// </summary>
        public JsonResolver()
        {
            JsonFilename = "appsettings.json";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResolver{T}"/>.
        /// </summary>
        /// <param name="filename">The filename of the json file to load.</param>
        public JsonResolver(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException(nameof(filename));
            }

            JsonFilename = filename;
        }

        /// <inheritdoc />
        public object Resolve()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(JsonFilename, false, true);

            ConfigureBuilder(builder);

            var configurationRoot = builder
                .Build();

            var config = configurationRoot.Get(typeof(T));

            return config;
        }

        /// <summary>
        /// Configures the builder for resolving configuration data.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected virtual void ConfigureBuilder(IConfigurationBuilder builder)
        {
        }

        /// <summary>
        /// Gets the filename to load configuration values from.
        /// </summary>
        public virtual string JsonFilename { get; }
    }
}