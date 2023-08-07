namespace Divergic.Configuration.Autofac.UnitTests;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

internal class TestService : IHostedService
{
    private readonly ITestOutputHelper _output;

    public TestService(IStorage storage, IChildConfig child, ITestOutputHelper output)
    {
        Storage1 = storage;
        Child = child;
        _output = output;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            _output.WriteLine(new string('_', 50));
            _output.WriteLine(JsonSerializer.Serialize(Storage1));
            _output.WriteLine(JsonSerializer.Serialize(Child));
            _output.WriteLine(string.Empty);
            _output.WriteLine(string.Empty);

            await Task.Delay(150, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public static IChildConfig Child { get; private set; }

    public static IStorage Storage1 { get; private set; }
}