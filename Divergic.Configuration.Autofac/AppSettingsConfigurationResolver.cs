namespace Divergic.Configuration.Autofac
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class AppSettingsConfigurationResolver<T> : IConfigurationResolver
    {
        public object Resolve()
        {
            // Add the configuration support
            var builder = CreateBuilder();

            var configurationRoot = builder
                .Build();

            var config = configurationRoot.Get(ConfigType);

            return config;
        }

        protected virtual IConfigurationBuilder CreateBuilder()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);
        }

        public Type ConfigType => typeof(T);
    }
}