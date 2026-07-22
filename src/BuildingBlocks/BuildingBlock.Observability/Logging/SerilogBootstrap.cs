using Microsoft.Extensions.Configuration;

using Serilog;

namespace BuildingBlock.Observability.Logging;

public static class SerilogBootstrap
{
    public static LoggerConfiguration ConfigureAppLogging(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string serviceName)
    {
        var seqUrl = configuration["Logging:Seq:Url"] ?? "http://seq:5341";
        var elasticsearchUrl = configuration["Logging:Elasticsearch:Url"];

        loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl);

        if (!string.IsNullOrWhiteSpace(elasticsearchUrl))
        {
            loggerConfiguration.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                IndexFormat = $"{serviceName}-logs-{{0:yyyy.MM.dd}}",
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                // Scope the auto-registered template's name/pattern to this service so multiple
                // services don't fight over the shared default "serilog-events-template" name.
                TemplateName = $"{serviceName}-logs-template",
                TemplateCustomSettings = new Dictionary<string, string>
                {
                    ["index.lifecycle.name"] = "logs-ilm-policy"
                }
            });
        }

        return loggerConfiguration;
    }
}
