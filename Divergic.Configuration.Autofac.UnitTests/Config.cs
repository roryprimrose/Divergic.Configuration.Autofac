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

        ParentConfig Parent { get; }

        Storage Storage { get; }
    }

    public class ParentConfig
    {
        public ChildConfig Child { get; set; }

        public string Value { get; set; }
    }

    public interface IChildConfig
    {
        string First { get; }

        Guid Second { get; }

        bool Third { get; }
    }

    public class ChildConfig : IChildConfig
    {
        public string First { get; set; }

        public Guid Second { get; set; }

        public bool Third { get; set; }
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
        public string StringData { get; set; } = "Custom.Redirected";
    }

    public class Config : IConfig
    {
        public EnvironmentValues Environment { get; set; } = new();

        public FirstJob FirstJob { get; set; }

        public ParentConfig Parent { get; set; }

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