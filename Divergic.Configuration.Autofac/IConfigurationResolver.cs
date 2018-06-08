namespace Divergic.Configuration.Autofac
{
    using System;

    public interface IConfigurationResolver
    {
        object Resolve();

        Type ConfigType { get; }
    }
}