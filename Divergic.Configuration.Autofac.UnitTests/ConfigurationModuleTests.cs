namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::Autofac;
    using ModelBuilder;
    using Xunit;

    public class ConfigurationModuleTests
    {
        private const int DefaultRegistrationCount = 1;

        [Fact]
        public void CanRegisterTypeWhenPropertyThrowsException()
        {
            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<ExceptionProperty>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<ExceptionProperty>();
        }

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
        public void CanRegisterTypeWithReadOnlyProperties()
        {
            // Try to get the read only property to be processed by using an environment override
            Environment.SetEnvironmentVariable(nameof(ReadOnlyProperty) + "." + nameof(ReadOnlyProperty.Timeout), "3");

            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<ReadOnlyProperty>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<ReadOnlyProperty>();
        }

        [Fact]
        public void CanRegisterTypeWithUriPropertyTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<Location>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 5);
            container.Should().HaveRegistered<Location>();
            container.Should().HaveRegistered<Uri>();
        }

        [Fact]
        public void CanRegisterTypeWithWriteOnlyProperties()
        {
            // Try to get the read only property to be processed by using an environment override
            Environment.SetEnvironmentVariable(nameof(WriteOnlyProperty) + "." + nameof(WriteOnlyProperty.Timeout),
                "3");

            var builder = new ContainerBuilder();
            var config = new WriteOnlyProperty();
            var resolver = new InstanceResolver(config);
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<WriteOnlyProperty>();
        }

        [Fact]
        public void DoesNotRegisterNullNestedPropertyValueTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new NullPropertyResolverConfig();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 5);
            container.Should().HaveRegistered<IConfig>();
            container.Should().HaveRegistered<Config>();
            container.Should().HaveRegistered<IFirstJob>();
            container.Should().HaveRegistered<FirstJob>();
            container.Should().HaveRegistered<EnvironmentValues>();
            container.Should().NotHaveRegistered<IStorage>();
            container.Should().NotHaveRegistered<Storage>();
        }

        [Fact]
        public void DoesNotRegisterStringTypeConfigTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<string>();
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
            var resolver = new ModelBuilderResolver<int>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount);
            container.Should().NotHaveRegistered<int>();
        }

        [Fact]
        public void IgnoresEnvironmentMappingWhenNoEnvironmentVariableFound()
        {
            var builder = new ContainerBuilder();
            var config = new EnvironmentValuesWithoutAttributes();
            var resolver = new InstanceResolver(config);
            var expectedString = config.StringData;

            Environment.SetEnvironmentVariable(expectedString, null);

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<EnvironmentValuesWithoutAttributes>();

            var actual = container.Resolve<EnvironmentValuesWithoutAttributes>();

            actual.StringData.Should().Be(expectedString);
        }

        [Fact]
        public void IgnoresEnvironmentOverrideWhenAttributeDefinedAndEnvironmentVariableValueIsInvalid()
        {
            var builder = new ContainerBuilder();
            var config = Model.Create<EnvironmentValues>();
            var resolver = new InstanceResolver(config);
            var expectedBool = config.BoolData;
            var expectedGuid = config.GuidData;
            var expectedInt = config.IntData;
            var expectedString = config.StringData;

            Environment.SetEnvironmentVariable("Custom.BoolData", Guid.NewGuid().ToString());
            Environment.SetEnvironmentVariable("Custom.GuidData", "this is not a guid");
            Environment.SetEnvironmentVariable("Custom.IntData", "definitely not an int");

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<EnvironmentValues>();

            var actual = container.Resolve<EnvironmentValues>();

            actual.BoolData.Should().Be(expectedBool);
            actual.GuidData.Should().Be(expectedGuid);
            actual.IntData.Should().Be(expectedInt);
            actual.StringData.Should().Be(expectedString);
        }

        [Fact]
        public void IgnoresEnvironmentOverrideWhenAttributeDefinedButNoEnvironmentVariableFound()
        {
            var builder = new ContainerBuilder();
            var config = Model.Create<EnvironmentValues>();
            var resolver = new InstanceResolver(config);
            var expectedBool = config.BoolData;
            var expectedGuid = config.GuidData;
            var expectedInt = config.IntData;
            var expectedString = config.StringData;

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<EnvironmentValues>();

            var actual = container.Resolve<EnvironmentValues>();

            actual.BoolData.Should().Be(expectedBool);
            actual.GuidData.Should().Be(expectedGuid);
            actual.IntData.Should().Be(expectedInt);
            actual.StringData.Should().Be(expectedString);
        }

        [Fact]
        public void IgnoresEnvironmentOverrideWhenNoAttributeDefined()
        {
            var builder = new ContainerBuilder();
            var config = Model.Create<Storage>();
            var resolver = new InstanceResolver(config);
            var expectedBlob = config.BlobStorage;
            var expectedDatabase = config.Database;
            var expectedTable = config.TableStorage;

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 2);
            container.Should().HaveRegistered<Storage>();

            var actual = container.Resolve<IStorage>();

            actual.BlobStorage.Should().Be(expectedBlob);
            actual.Database.Should().Be(expectedDatabase);
            actual.TableStorage.Should().Be(expectedTable);
        }

        [Fact]
        public void MapsPropertyWithEnvironmentVariablesWhenPropertyValueMatchesEnvironmentVariable()
        {
            var builder = new ContainerBuilder();
            var config = new EnvironmentValuesWithoutAttributes();
            var resolver = new InstanceResolver(config);
            var expectedString = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable(config.StringData, expectedString);

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<EnvironmentValuesWithoutAttributes>();

            var actual = container.Resolve<EnvironmentValuesWithoutAttributes>();

            actual.StringData.Should().Be(expectedString);
        }

        [Fact]
        public void OverridesPropertiesWithEnvironmentVariablesWhenAttributeDefined()
        {
            var builder = new ContainerBuilder();
            var config = Model.Create<EnvironmentValues>();
            var resolver = new InstanceResolver(config);
            var expectedString = Guid.NewGuid().ToString();
            var expectedBool = Model.Create<bool>();
            var expectedGuid = Guid.NewGuid();
            var expectedInt = Environment.TickCount;

            Environment.SetEnvironmentVariable("Custom.BoolData", expectedBool.ToString());
            Environment.SetEnvironmentVariable("Custom.GuidData", expectedGuid.ToString());
            Environment.SetEnvironmentVariable("Custom.IntData", expectedInt.ToString());
            Environment.SetEnvironmentVariable("Custom.StringData", expectedString);

            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            // There are several registrations which aren't expected because of Uri properties being resolved
            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 1);
            container.Should().HaveRegistered<EnvironmentValues>();

            var actual = container.Resolve<EnvironmentValues>();

            actual.BoolData.Should().Be(expectedBool);
            actual.GuidData.Should().Be(expectedGuid);
            actual.IntData.Should().Be(expectedInt);
            actual.StringData.Should().Be(expectedString);
        }

        [Fact]
        public void RegistersConfigurationWithClassHavingIndexerPropertyTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<DataSet>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 3);

            container.Should().HaveRegistered<ICollection<Location>>();
            container.Should().HaveRegistered<Collection<Location>>();

            var config = container.Resolve<ICollection<Location>>();

            container.Resolve<Collection<Location>>().Should().BeSameAs(config);
        }

        [Fact]
        public void RegistersConfigurationWithNestedTypesTest()
        {
            var builder = new ContainerBuilder();
            var resolver = new ModelBuilderResolver<Config>();
            var sut = new ConfigurationModule(resolver);

            builder.RegisterModule(sut);

            var container = builder.Build();

            container.ComponentRegistry.Registrations.Should().HaveCount(DefaultRegistrationCount + 7);

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
            var resolver = new ModelBuilderResolver<FirstJob>();
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
            var resolver = new ModelBuilderResolver<NoDefinition>();
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
        [SuppressMessage("Usage", "CA1806:Do not ignore method results",
            Justification = "The constructor is what is being tested")]
        public void ThrowsExceptionWhenCreatedWithNullResolverTest()
        {
            Action action = () => new ConfigurationModule(null);

            action.Should().Throw<ArgumentNullException>();
        }

        private class Child
        {
            public Parent Parent { get; set; }
        }

        [SuppressMessage("Usage", "CA1812:Class not instantiated",
            Justification = "The is used by Autofac registrations")]
        private class DataSet
        {
            public Collection<Location> Locations { get; set; }
        }

        private class ExceptionProperty
        {
            public TimeSpan Timeout => TimeSpan.FromSeconds(int.Parse(TimeoutInSeconds));

            public string TimeoutInSeconds { get; set; } = "5";
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

        [SuppressMessage("Usage", "CA1812:Class not instantiated",
            Justification = "The is used by Autofac registrations")]
        private class Location
        {
            public Uri Address { get; set; }
        }

        private class ModelBuilderResolver<T> : IConfigurationResolver
        {
            public object Resolve()
            {
                return Model.Create(ConfigType);
            }

            private static Type ConfigType => typeof(T);
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

        private class Parent
        {
            public Child Child { get; set; }
        }

        private class ReadOnlyProperty
        {
            [EnvironmentOverride(nameof(ReadOnlyProperty) + "." + nameof(Timeout))]
            public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutInSeconds);

            public int TimeoutInSeconds { get; set; } = 5;
        }

        private class WriteOnlyProperty
        {
            public TimeSpan StoredValue { get; private set; } = TimeSpan.Zero;

            [EnvironmentOverride(nameof(WriteOnlyProperty) + "." + nameof(Timeout))]
            public TimeSpan Timeout { set => StoredValue = value; }

            public int TimeoutInSeconds { get; set; } = 5;
        }
    }
}