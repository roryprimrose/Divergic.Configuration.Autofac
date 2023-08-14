namespace Divergic.Configuration.Autofac
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="EnvironmentJsonResolver{T}"/>
    /// class is used to resolve configuration from a JSON settings file including an environment JSON file.
    /// </summary>
    /// <typeparam name="T">The type of class to create from the configuration file.</typeparam>
    public class EnvironmentJsonResolver<T> : JsonResolver<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentJsonResolver{T}"/>.
        /// </summary>
        /// <param name="environmentFilename">The filename of the environment specific json configuration to load.</param>
        /// <exception cref="ArgumentException">The <paramref name="environmentFilename"/> parameter is <c>null</c>.</exception>
        public EnvironmentJsonResolver(string environmentFilename)
        {
            if (string.IsNullOrWhiteSpace(environmentFilename))
            {
                throw new ArgumentException(nameof(environmentFilename));
            }

            EnvironmentJsonFilename = environmentFilename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentJsonResolver{T}"/>.
        /// </summary>
        /// <param name="filename">The filename of the json configuration to load.</param>
        /// <param name="environmentFilename">The filename of the environment specific json configuration to load.</param>
        /// <exception cref="ArgumentException">The <paramref name="environmentFilename"/> parameter is <c>null</c>.</exception>
        public EnvironmentJsonResolver(string filename, string environmentFilename)
            : base(filename)
        {
            if (string.IsNullOrWhiteSpace(environmentFilename))
            {
                throw new ArgumentException(nameof(environmentFilename));
            }

            EnvironmentJsonFilename = environmentFilename;
        }

        /// <inheritdoc />
        protected override void ConfigureBuilder(IConfigurationBuilder builder)
        {
            base.ConfigureBuilder(builder);

            builder.AddJsonFile(EnvironmentJsonFilename, true, true);
        }

        /// <summary>
        /// Gets the filename to load configuration values from.
        /// </summary>
        public virtual string EnvironmentJsonFilename { get; }
    }
}