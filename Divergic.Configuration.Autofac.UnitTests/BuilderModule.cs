namespace Divergic.Configuration.Autofac.UnitTests;

using System.Reflection;
using ModelBuilder;

internal class BuilderModule : IConfigurationModule
{
    public void Configure(IBuildConfiguration configuration)
    {
        // This is a workaround due to a bug in ModelBuilder where write only properties are not processed correct - see https://github.com/roryprimrose/ModelBuilder/issues/282
        configuration.AddIgnoreRule((PropertyInfo property) => property.Name == nameof(Protected.PrivateWriteOnly));
    }
}