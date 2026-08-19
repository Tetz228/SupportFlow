using Microsoft.AspNetCore.Mvc.Testing;

namespace SupportFlow.IntegrationTests;

public sealed class SupportFlowApiFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString = "Host=127.0.0.1;Database=supportflow-tests;Username=test;Password=test";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SupportFlow"] = TestConnectionString
            });
        });

        return base.CreateHost(builder);
    }
}
