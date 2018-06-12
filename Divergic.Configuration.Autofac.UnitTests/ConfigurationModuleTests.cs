namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using global::Autofac;
    using ModelBuilder;
    using Xunit;

    public class ConfigurationModuleTests
    {
        private const int DefaultRegistrationCount = 1;

        [Fact]
        public void DoesNotRegisterNullNestedPropertyValueTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new NullPropertyResolverConfig();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 4);
            container.Should().HaveRegistered<IConfig>();
            container.Should().HaveRegistered<Config>();
            container.Should().HaveRegistered<IFirstJob>();
            container.Should().HaveRegistered<FirstJob>();
            container.Should().NotHaveRegistered<IStorage>();
            container.Should().NotHaveRegistered<Storage>();
        }

        [Fact]
        public void DoesNotRegisterStringTypeConfigTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<string>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount);
            container.Should().NotHaveRegistered<string>();
        }

        [Fact]
        public void DoesNotRegisterValueTypeConfigTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<int>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount);
            container.Should().NotHaveRegistered<int>();
        }

        [Fact]
        public void RegistersConfigurationWithNestedTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<Config>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 6);

            container.Should().HaveRegistered<IConfig>();
            container.Should().HaveRegistered<Config>();
            container.Should().HaveRegistered<IStorage>();
            container.Should().HaveRegistered<Storage>();
            container.Should().HaveRegistered<IFirstJob>();
            container.Should().HaveRegistered<FirstJob>();

            var config = container.Resolve<IConfig>();

            container.Resolve<Config>().Should().BeSameAs(config);

            var storage = container.Resolve<IStorage>();

            container.Resolve<Storage>().Should().BeSameAs(storage);

            storage.Should().BeEquivalentTo(config.Storage);

            var job = container.Resolve<IFirstJob>();

            container.Resolve<FirstJob>().Should().BeSameAs(job);

            job.Should().BeEquivalentTo(config.FirstJob);
        }

        [Fact]
        public void RegistersConfigurationWithoutNestedTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<FirstJob>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 2);
            container.Should().HaveRegistered<IFirstJob>();
            container.Should().HaveRegistered<FirstJob>();
        }

        [Fact]
        public void RegistersInstanceWithoutInterfaceTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<NoDefinition>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<NoDefinition>();
        }

        [Fact]
        public void SkipsRegistrationWhenNullConfigurationFoundTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new NullResolver<Config>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount);
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullResolverTest()
        {
            Action action = () => new ConfigurationModule(null);

            action.Should().Throw<ArgumentNullException>();
        }

        private class InMemoryResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return Model.Create(ConfigType);
            }

            public Type ConfigType => typeof(T);
        }

        private class NullPropertyResolverConfig : IConfigurationResolver
        {
            public object Resolve()
            {
                var model = Model.Create<Config>().Set(x => x.Storage = null);

                return model;
            }

            public Type ConfigType => typeof(Config);
        }

        private class NullResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return null;
            }

            public Type ConfigType => typeof(T);
        }
    }
}