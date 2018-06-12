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
        private readonly string _environmentFilename;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentJsonResolver{T}"/>.
        /// </summary>
        /// <param name="environmentFilename">The filename of the environment specific json configuration to load.</param>
        public EnvironmentJsonResolver(string environmentFilename)
        {
            if (string.IsNullOrWhiteSpace(environmentFilename))
            {
                throw new ArgumentException(nameof(environmentFilename));
            }

            _environmentFilename = environmentFilename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentJsonResolver{T}"/>.
        /// </summary>
        /// <param name="filename">The filename of the json configuration to load.</param>
        /// <param name="environmentFilename">The filename of the environment specific json configuration to load.</param>
        public EnvironmentJsonResolver(string filename, string environmentFilename)
            : base(filename)
        {
            if (string.IsNullOrWhiteSpace(environmentFilename))
            {
                throw new ArgumentException(nameof(environmentFilename));
            }

            _environmentFilename = environmentFilename;
        }

        /// <inheritdoc />
        protected override void ConfigureBuilder(IConfigurationBuilder builder)
        {
            base.ConfigureBuilder(builder);

            builder.AddJsonFile(_environmentFilename, true, true);
        }
    }
}