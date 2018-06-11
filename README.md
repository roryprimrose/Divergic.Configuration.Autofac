# Introduction

The Divergic.Configuration.Autofac package provides an Autofac module for registering nested configuration types. This is helpful when wanting dependency injection of configuration types loaded from JSON configuration in ASP.Net core.

# Configuration Resolution

The module relies on an ```IConfigurationResolver``` to load the root configuration class. It will then recursively register all properties found on the resolved configuration and register them using ```AsSelf``` as well as ```AsImplementedInterfaces``` where implemented interfaces are found. 

# Json Resolution

The ```JsonResolver<T>``` resolver will load the root configuration (of type ```T```) from an appsettings.json file by default. This can be used by ```ConfigurationModule``` like the following.

```csharp
var builder = new ContainerBuilder();

builder.RegisterModule<ConfigurationModule<JsonResolver<Config>>>();

var container = builder.Build();
```

Perhaps the default appsettings.json filename does not work in a particular scenario. The ```JsonResolver``` class can accept a different filename.

```csharp
var builder = new ContainerBuilder();
var resolver = new JsonResolver<Config>("hostsettings.json");
var module = new ConfigurationModule(resolver);

builder.RegisterModule(module);

var container = builder.Build();
```

Need environment configuration override support? This would work in ASP.Net core.

```csharp
var env = builderContext.HostingEnvironment;
var builder = new ContainerBuilder();

var resolver = new EnvironmentJsonResolver<Config>("appsettings.json", $"appsettings.{env.EnvironmentName}.json");
var module = new ConfigurationModule(resolver);

builder.RegisterModule(module);

var container = builder.Build();
```

# Custom Resolution

Need to resolve configuration from something other an a JSON file? The ```ConfigurationModule``` will accept any ```IConfigurationResolver```. You can write any custom resolver and provide it to the module.

# Example

Consider the following JSON configuration.

```json
{
  "Storage": {
    "Database": "database connection string",
    "BlobStorage": "blob storage connection",
    "TableStorage": "table storage connection"
  },
  "FirstJob": {
    "Name": "My job",
    "TriggerInSeconds": 60
  }
}
```

This configuration can be represented using the following configuration classes.

```csharp
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
```

The module will register each of the nested properties on the ```Config``` class with the Autofac container. Given the example above, the following  resolutions on the container will be valid.

```csharp
var builder = new ContainerBuilder();

builder.RegisterModule<ConfigurationModule<JsonResolver<Config>>>();

var container = builder.Build();

container.Resolve<IConfig>();
container.Resolve<Config>();
container.Resolve<IStorage>();
container.Resolve<Storage>();
container.Resolve<IFirstJob>();
container.Resolve<FirstJob>();
```

Each of these resolutions can now participate in dependency injection for any other class.