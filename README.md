# Introduction

The Divergic.Configuration.Autofac NuGet package provides an Autofac module for registering nested application configuration types. This is helpful when wanting dependency injection of strong typed application configuration.

[![GitHub license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/roryprimrose/Divergic.Configuration.Autofac/blob/master/LICENSE)&nbsp;&nbsp;&nbsp;[![Nuget](https://img.shields.io/nuget/v/Divergic.Configuration.Autofac.svg)&nbsp;&nbsp;&nbsp;![Nuget](https://img.shields.io/nuget/dt/Divergic.Configuration.Autofac.svg)](https://www.nuget.org/packages/Divergic.Configuration.Autofac)

[![Actions Status](https://github.com/roryprimrose/Divergic.Configuration.Autofac/workflows/CI/badge.svg)](https://github.com/roryprimrose/Divergic.Configuration.Autofac/actions)

# Installation

The package can be installed from NuGet using ```Install-Package Divergic.Configuration.Autofac```.

# Usage
There are two ways this package can be used to load application configuration.
1. Automatically resolve from host configuration
1. Provide specific configuration resolution

## Host configuration
Most modern .Net applications will have their configuration based on `IConfiguration` that is loaded automatically via `WebApplication.CreateBuilder(args)` or `Host.CreateDefaultBuilder()`. In these cases the application configuration is loaded into `ServicesCollection` which can be ported over to Autofac using the [Autofac.Extensions.DependencyInjection](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html#asp-net-core-3-0-and-generic-hosting) package.

This package supports automatically binding your application configuration to your custom application configuration types and configuring the Autofac container with those types.

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

Adding `HostConfigurationModule<Config>` to the container configuration will cause all the configuration types (including interfaces) to be registered with Autofac. Autofac will return those configuration types bound from the application configuration when they are resolved.

```csharp
using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(configure =>
            {
                configure.RegisterModule<HostConfigurationModule<Config>>();
            });

        var host = builder.Build();

        await host.StartAsync().ConfigureAwait(false);

        Console.WriteLine("Completed");
    }
}
```

## Configuration Resolution

The `ConfigurationModule<T>` class relies on an ```IConfigurationResolver``` to load the root configuration class. It will then recursively register all properties found on the root configuration using ```AsSelf``` as well as ```AsImplementedInterfaces``` where implemented interfaces are found. 

### Json Resolution

The ```JsonResolver<T>``` resolver will load the root configuration (of type ```T```) from an appsettings.json file by default. This can be used by ```ConfigurationModule``` like the following.

```csharp
var builder = new ContainerBuilder();

builder.RegisterModule<ConfigurationModule<JsonResolver<Config>>>();

var container = builder.Build();
```

The ```JsonResolver``` class can accept a different filename if the default appsettings.json is not suitable.

```csharp
var builder = new ContainerBuilder();
var resolver = new JsonResolver<Config>("hostsettings.json");
var module = new ConfigurationModule(resolver);

builder.RegisterModule(module);

var container = builder.Build();
```

Need environment configuration override support? Use the `EnvironmentJsonResolver` to merge configuration for a specific environment.

```csharp
var env = builderContext.HostingEnvironment;
var builder = new ContainerBuilder();

var resolver = new EnvironmentJsonResolver<Config>("appsettings.json", $"appsettings.{env.EnvironmentName}.json");
var module = new ConfigurationModule(resolver);

builder.RegisterModule(module);

var container = builder.Build();
```

### Custom Resolution

Need to resolve a root configuration object from something other than a JSON file? The ```ConfigurationModule``` will accept any ```IConfigurationResolver```. You can write any custom resolver and provide it to the module.

### Example

The `ConfigurationModule<T>` class will register each of the nested properties on the ```Config``` class with the Autofac container. Given the example above, the following resolutions on the container will be valid.

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

# Environment Variable Override

Sometimes you need to be able to change a configuration value without having to redeploy the application or change the application configuration file. Just changing the application configuration file can require a redeployment to deliver the change to a target system. This can be done by using the `EnvironmentOverride` attribute. A common scenario here is Azure Functions which uses Environment Variables via configuration settings in the Azure portal. 

The `ConfigurationModule` class will detect the `EnvironmentOverride` attribute being defined on a property and will attempt to resolve the value. If the environment variable defined by the attribute exists, has a value and that value can be converted to the property type then the configuration object will be updated with the value of the environment variable before it is registered in Autofac. If any of these conditions are not met then the existing value from the configuration resolver will remain on the object.


```csharp
public class Storage : IStorage
{
    [EnvironmentOverride("Application.BlobStorageConnection")]
    public string BlobStorage { get; set; }

    [EnvironmentOverride("Application.DatabaseConnection")]
    public string Database { get; set; }

    [EnvironmentOverride("Application.TableStorageConnection")]
    public string TableStorage { get; set; }
}
```
The advantage of this design is that application configuration can be resolved with a pre-allocation of an environment override that only comes into play when the environment variable has been set.

## Environment Variable Mapping

The module can detect if a string property is a reference to an environment variable. In this case it will resolve the configuration value from the environment variable using the same rules as the Environment Variable Override above.
