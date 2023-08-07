namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Autofac;
    using global::Autofac.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class HostConfigurationModuleTests
    {
        private readonly ITestOutputHelper _output;

        public HostConfigurationModuleTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
        public async Task CanPopulateConfigurationForHostedApplication()
        {
            using var tokenSource = new CancellationTokenSource();

            var sut = new TestApplication(_output);

            var applicationTask = sut.Run(tokenSource.Token);
            var timeoutTask = Task.Delay(1000, tokenSource.Token);

            await Task.WhenAny(applicationTask, timeoutTask).ConfigureAwait(false);

            tokenSource.Cancel();

            try
            {
                // Wait for the application to close
                await Task.WhenAll(applicationTask, timeoutTask).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                // This was expected as the token was cancelled
            }

            TestService.Child.Should().NotBeNull();
            TestService.Child.First.Should().NotBeNullOrEmpty();
            TestService.Child.Second.Should().NotBeEmpty();
            TestService.Child.Third.Should().BeTrue();
            TestService.Storage1.BlobStorage.Should().NotBeNullOrEmpty();
            TestService.Storage1.Database.Should().NotBeNullOrEmpty();
            TestService.Storage1.TableStorage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void LoadIgnoresStringConfigTypes()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<HostConfigurationModule<string>>();

            using var container = builder.Build();

            container.IsRegistered<string>().Should().BeFalse();
        }

        [Fact]
        public void LoadIgnoresValueTypesOnModuleDefinition()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<HostConfigurationModule<bool>>();

            using var container = builder.Build();

            container.IsRegistered<bool>().Should().BeFalse();
        }

        [Fact]
        public void LoadRegistersConfigTypesOnModuleDefinition()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            var config = configurationBuilder.Build();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<IConfiguration>();
            builder.RegisterModule<HostConfigurationModule<Config>>();

            using var container = builder.Build();
            
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

            try
            {
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
            finally
            {
                Environment.SetEnvironmentVariable("Custom.BoolData", null);
                Environment.SetEnvironmentVariable("Custom.GuidData", null);
                Environment.SetEnvironmentVariable("Custom.IntData", null);
                Environment.SetEnvironmentVariable("Custom.StringData", null);
            }
        }

        private class TestApplication
        {
            private readonly ITestOutputHelper _output;

            public TestApplication(ITestOutputHelper output)
            {
                _output = output;
            }

            public async Task Run(CancellationToken cancellationToken)
            {
                var builder = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) => { services.Configure<Config>(context.Configuration); })
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>(c =>
                    {
                        c.RegisterInstance(_output).As<ITestOutputHelper>();
                        c.RegisterType<TestService>().As<IHostedService>();
                        c.RegisterModule<HostConfigurationModule<Config>>();
                    });

                await builder.Build().RunAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}