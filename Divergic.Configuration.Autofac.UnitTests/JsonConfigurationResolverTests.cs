namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class JsonConfigurationResolverTests
    {
        [Fact]
        public void ResolveLoadsInformationFromExpectedFileTest()
        {
            var sut = new JsonConfigurationResolver<Config>();

            var actual = sut.Resolve() as Config;

            actual.Should().NotBeNull();
            actual.Storage.BlobStorage.Should().NotBeNullOrEmpty();
            actual.Storage.Database.Should().NotBeNullOrEmpty();
            actual.Storage.TableStorage.Should().NotBeNullOrEmpty();
            actual.FirstJob.Name.Should().NotBeNullOrEmpty();
            actual.FirstJob.TriggerInSeconds.Should().NotBe(0);
            actual.FirstJob.Trigger.Should().NotBe(TimeSpan.Zero);
        }
    }
}