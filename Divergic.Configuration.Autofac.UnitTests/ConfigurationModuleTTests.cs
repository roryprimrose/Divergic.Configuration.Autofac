namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using global::Autofac;
    using ModelBuilder;
    using Xunit;

    public class ConfigurationModuleTTests
    {
        [Fact]
        public void RegistersConfigurationWithNestedTypesTest()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ConfigurationModule<InMemoryResolver<Config>>>();

            var container = builder.Build();
            
            container.Should().HaveRegistered<IConfig>();
            container.Should().HaveRegistered<Config>();
            container.Should().HaveRegistered<IFirstJob>();
            container.Should().HaveRegistered<FirstJob>();
            container.Should().HaveRegistered<EnvironmentValues>();
            container.Should().HaveRegistered<ParentConfig>();
            container.Should().HaveRegistered<ChildConfig>();
            container.Should().HaveRegistered<IChildConfig>();
            container.Should().HaveRegistered<IStorage>();
            container.Should().HaveRegistered<Storage>();
            container.Should().HaveRegistered<IProtected>();
            container.Should().HaveRegistered<Protected>();

            var config = container.Resolve<IConfig>();

            container.Resolve<Config>().Should().BeSameAs(config);

            var storage = container.Resolve<IStorage>();

            container.Resolve<Storage>().Should().BeSameAs(storage);

            storage.Should().BeEquivalentTo(config.Storage);

            var job = container.Resolve<IFirstJob>();

            container.Resolve<FirstJob>().Should().BeSameAs(job);

            job.Should().BeEquivalentTo(config.FirstJob);
        }

        private class InMemoryResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return Model.Create(ConfigType);
            }

            private static Type ConfigType => typeof(T);
        }
    }
}