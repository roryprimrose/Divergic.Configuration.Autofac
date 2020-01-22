namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Usage", "CA1806:Do not ignore method results", Justification = "The constructor is what is being tested")]
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
        [SuppressMessage("Usage", "CA1806:Do not ignore method results", Justification = "The constructor is what is being tested")]
        public void ThrowsExceptionWhenCreatedWithInvalidParametersTest(string filename, string environmentFilename)
        {
            Action action = () => new EnvironmentJsonResolver<Config>(filename, environmentFilename);

            action.Should().Throw<ArgumentException>();
        }
    }
}