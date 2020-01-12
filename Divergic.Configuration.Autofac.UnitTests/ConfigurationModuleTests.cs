namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::Autofac;
    using ModelBuilder;
    using Xunit;

    public class ConfigurationModuleTests
    {
        private const int DefaultRegistrationCount = 1;

        [Fact]
        public void CanRegisterTypeWithCircularReferenceTest()
        {
            var value = new Parent {Child = new Child()};

            value.Child.Parent = value;

            var builder = new ContainerBuilder();
            var resolver = new InstanceResolver(value);
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 2);
            container.Should().HaveRegistered<Parent>();
            container.Should().HaveRegistered<Child>();
        }

        [Fact]
        public void CanRegisterTypeWithUriPropertyTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new InMemoryResolver<Location>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 5);
            container.Should().HaveRegistered<Location>();
            container.Should().HaveRegistered<Uri>();
        }

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
        [SuppressMessage("Usage", "CA1806:Do not ignore method results", Justification = "The constructor is what is being tested")]
        public void ThrowsExceptionWhenCreatedWithNullResolverTest()
        {
            Action action = () => new ConfigurationModule(null);

            action.Should().Throw<ArgumentNullException>();
        }

        private class Child
        {
            public Parent Parent { get; set; }
        }

        [SuppressMessage("Usage", "CA1812:Class not instantiated", Justification = "The is used by Autofac registrations")]
        private class Location
        {
            public Uri Address { get; set; }
        }

        private class Parent
        {
            public Child Child { get; set; }
        }

        private class InMemoryResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return Model.Create(ConfigType);
            }

            private static Type ConfigType => typeof(T);
        }

        private class InstanceResolver : IConfigurationResolver
        {
            private readonly object _value;

            public InstanceResolver(object value)
            {
                _value = value;
            }

            public object Resolve()
            {
                return _value;
            }

            public Type ConfigType => _value.GetType();
        }

        private class NullPropertyResolverConfig : IConfigurationResolver
        {
            public object Resolve()
            {
                var model = Model.Create<Config>().Set(x => x.Storage = null);

                return model;
            }
        }

        private class NullResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return null;
            }
        }
    }
}