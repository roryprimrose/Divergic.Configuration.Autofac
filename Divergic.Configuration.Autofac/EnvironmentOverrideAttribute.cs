namespace Divergic.Configuration.Autofac
{
    using System;

    /// <summary>
    /// The <see cref="EnvironmentOverrideAttribute"/>
    /// class is used to allow an environment variable to override any loaded configuration value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EnvironmentOverrideAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentOverrideAttribute"/> class.
        /// </summary>
        /// <param name="variable">The environment variable name.</param>
        public EnvironmentOverrideAttribute(string variable)
        {
            Variable = variable;
        }

        /// <summary>
        /// Gets the environment variable defined on the attribute.
        /// </summary>
        public string Variable { get; }
    }
}