namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using global::Autofac;
    using Microsoft.Extensions.Configuration;
    using ModelBuilder;
    using Xunit;

    public class HostConfigurationModuleTests
    {
        [Theory]
        [InlineData(typeof(Config))]
        [InlineData(typeof(EnvironmentValues))]
        [InlineData(typeof(FirstJob))]
        [InlineData(typeof(IFirstJob))]
        [InlineData(typeof(ParentConfig))]
        [InlineData(typeof(ChildConfig))]
        [InlineData(typeof(IChildConfig))]
        [InlineData(typeof(Storage))]
        [InlineData(typeof(IStorage))]
        public void CanLoadConfigurationTypesFromHostConfiguration(Type expected)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = configurationBuilder.Build();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<IConfiguration>();
            builder.RegisterModule<HostConfigurationModule<Config>>();

            using var container = builder.Build();

            var actual = container.Resolve(expected);

            actual.Should().NotBeNull();
            actual.Should().BeAssignableTo(expected);
        }

        [Fact]
        public void OverridesPropertiesWithEnvironmentVariablesWhenAttributeDefined()
        {
            var expectedString = Guid.NewGuid().ToString();
            var expectedBool = Model.Create<bool>();
            var expectedGuid = Guid.NewGuid();
            var expectedInt = Environment.TickCount;

            Environment.SetEnvironmentVariable("Custom.BoolData", expectedBool.ToString());
            Environment.SetEnvironmentVariable("Custom.GuidData", expectedGuid.ToString());
            Environment.SetEnvironmentVariable("Custom.IntData", expectedInt.ToString());
            Environment.SetEnvironmentVariable("Custom.StringData", expectedString);

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = configurationBuilder.Build();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<IConfiguration>();
            builder.RegisterModule<HostConfigurationModule<Config>>();

            using var container = builder.Build();

            var actual = container.Resolve<EnvironmentValues>();

            actual.BoolData.Should().Be(expectedBool);
            actual.GuidData.Should().Be(expectedGuid);
            actual.IntData.Should().Be(expectedInt);
            actual.StringData.Should().Be(expectedString);
        }
    }
}