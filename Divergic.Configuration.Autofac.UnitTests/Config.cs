namespace Divergic.Configuration.Autofac.UnitTests
{
    using System;

    public interface IConfig
    {
        FirstJob FirstJob { get; }

        Storage Storage { get; }
    }

    public class Config : IConfig
    {
        public FirstJob FirstJob { get; set; }

        public Storage Storage { get; set; }
    }

    public interface IFirstJob
    {
        string Name { get; }

        TimeSpan Trigger { get; }

        int TriggerInSeconds { get; }
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