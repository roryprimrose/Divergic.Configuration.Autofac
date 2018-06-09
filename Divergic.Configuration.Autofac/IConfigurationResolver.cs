namespace Divergic.Configuration.Autofac
{
    using System;

    /// <summary>
    /// The <see cref="IConfigurationResolver"/>
    /// interface defines the members for resolving a configuration value.
    /// </summary>
    public interface IConfigurationResolver
    {
        /// <summary>
        /// Resolves the root configuration value.
        /// </summary>
        /// <returns>The configuration value.</returns>
        object Resolve();

        /// <summary>
        /// Identifies the root configuration type.
        /// </summary>
        Type ConfigType { get; }
    }
}