namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using global::Autofac;
    using Xunit;

    public class ConfigurationModuleTests
    {
        [Fact]
        public void Test1()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ConfigurationModule<JsonConfigurationResolver<Config>>>();

            var container = builder.Build();

            var config = container.Resolve<IConfig>();

            config.Storage.BlobStorage.Should().NotBeNullOrEmpty();
            config.Storage.Database.Should().NotBeNullOrEmpty();
            config.Storage.TableStorage.Should().NotBeNullOrEmpty();
            config.FirstJob.Name.Should().NotBeNullOrEmpty();
            config.FirstJob.TriggerInSeconds.Should().NotBe(0);
            config.FirstJob.Trigger.Should().NotBe(TimeSpan.Zero);

            var storage = container.Resolve<IStorage>();

            storage.Should().BeEquivalentTo(config.Storage);

            var job = container.Resolve<IFirstJob>();

            job.Should().BeEquivalentTo(config.FirstJob);
        }
    }
}