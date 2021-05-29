namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;

    public class NoDefinition
    {
        public string Value { get; set; }
    }

    public interface IConfig
    {
        EnvironmentValues Environment { get; }

        FirstJob FirstJob { get; }

        Storage Storage { get; }
    }

    public class EnvironmentValues
    {
        [EnvironmentOverride("Custom.BoolData")]
        public bool BoolData { get; set; }

        [EnvironmentOverride("Custom.GuidData")]
        public Guid GuidData { get; set; }

        [EnvironmentOverride("Custom.IntData")]
        public int IntData { get; set; }

        [EnvironmentOverride("Custom.StringData")]
        public string StringData { get; set; }
    }

    public class EnvironmentValuesWithoutAttributes
    {
        public string StringData { get; set; } = "Custom.StringData";
    }

    public class Config : IConfig
    {
        public EnvironmentValues Environment { get; set; }

        public FirstJob FirstJob { get; set; }

        public Storage Storage { get; set; }
    }

    public interface IFirstJob
    {
        string Name { get; }

        TimeSpan Trigger { get; }
    }

    public class FirstJob : IFirstJob
    {
        public string Name { get; set; }

        public TimeSpan Trigger => TimeSpan.FromSeconds(TriggerInSeconds);

        public int TriggerInSeconds { get; set; }
    }

    public interface IStorage
    {
        string BlobStorage { get; }

        string Database { get; }

        string TableStorage { get; }
    }

    public class Storage : IStorage
    {
        public string BlobStorage { get; set; }

        public string Database { get; set; }

        public string TableStorage { get; set; }
    }
}