namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class JsonResolverTests
    {
        [Fact]
        public void ResolveLoadsInformationFromAppSettingsFileTest()
        {
            var sut = new JsonResolver<Config>();

            var actual = sut.Resolve() as Config;

            actual.Should().NotBeNull();
            actual.Storage.BlobStorage.Should().NotBeNullOrEmpty();
            actual.Storage.Database.Should().NotBeNullOrEmpty();
            actual.Storage.TableStorage.Should().NotBeNullOrEmpty();
            actual.FirstJob.Name.Should().NotBeNullOrEmpty();
            actual.FirstJob.TriggerInSeconds.Should().NotBe(0);
            actual.FirstJob.Trigger.Should().NotBe(TimeSpan.Zero);
        }

        [Fact]
        public void ResolveLoadsInformationFromSpecifiedFileTest()
        {
            var sut = new JsonResolver<Config>("localsettings.json");

            var actual = sut.Resolve() as Config;

            actual.Storage.BlobStorage.Should().StartWith("local ");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ThrowsExceptionWhenCreatedWithInvalidFilenameTest(string filename)
        {
            Action action = () => new JsonResolver<Config>(filename);

            action.Should().Throw<ArgumentException>();
        }
    }
}