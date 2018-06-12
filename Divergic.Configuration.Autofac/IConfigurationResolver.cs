namespace Divergic.Configuration.Autofac
{
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
    }
}