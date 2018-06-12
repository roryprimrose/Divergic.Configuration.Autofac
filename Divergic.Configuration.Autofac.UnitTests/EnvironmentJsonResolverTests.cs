namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class EnvironmentJsonResolverTests
    {
        [Fact]
        public void ResolvesConfigurationWhenEnvironmentFilenameNotFoundTest()
        {
            var sut = new EnvironmentJsonResolver<Config>("localsettings.json");

            var actual = sut.Resolve().As<Config>();

            actual.Storage.Database.Should().NotStartWith("local ");
            actual.Storage.BlobStorage.Should().StartWith("local ");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowsExceptionWhenCreatedWithInvalidEnvironmentFilenameTest(string environmentFilename)
        {
            Action action = () => new EnvironmentJsonResolver<Config>(environmentFilename);

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("appsettings.json", null)]
        [InlineData("appsettings.json", "")]
        [InlineData("appsettings.json", "   ")]
        [InlineData(null, "appsettings.development.json")]
        [InlineData("", "appsettings.development.json")]
        [InlineData("   ", "appsettings.development.json")]
        public void ThrowsExceptionWhenCreatedWithInvalidParametersTest(string filename, string environmentFilename)
        {
            Action action = () => new EnvironmentJsonResolver<Config>(filename, environmentFilename);

            action.Should().Throw<ArgumentException>();
        }
    }
}