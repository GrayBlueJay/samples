using System.Net;
using System.Runtime.CompilerServices;
using MassTransit;
using Serilog;
using TestSagaBatchProducer;
using Contracts;
using TestSagaBatchProducer.Models;
using Microsoft.Extensions.Options;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(hostConfig =>
    {
        hostConfig.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
        hostConfig.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        hostConfig.AddEnvironmentVariables();
    })
    .UseSerilog((hostContext, loggerConfiguration) =>
    {
        var configuration = hostContext.Configuration;
        loggerConfiguration
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Hostname", Dns.GetHostName())
            .Enrich.WithProperty("ServiceName", configuration["AppSettings:AppName"])
            .ReadFrom.Configuration(configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var appConfig = configuration.GetSection("AppSettings").Get<ConfigurationAppSettings>();
        services.Configure<ConfigurationAppSettings>(options => configuration.GetSection("AppSettings").Bind(options));

        services.AddMassTransit(x =>
        {
            x.AddRequestClient<SubmitBatch>();

            if (appConfig.AmazonSqs != null)
            {
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host(appConfig.AmazonSqs.Region, h =>
                    {
                        h.AccessKey(appConfig.AmazonSqs.AccessKey);
                        h.SecretKey(appConfig.AmazonSqs.SecretKey);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            else if (appConfig.Artemis != null)
            {

                x.UsingActiveMq((context, cfg) =>
                {
                    cfg.Host(appConfig.Artemis.HostAddress,
                        int.Parse(appConfig.Artemis.HostPort), h =>
                        {
                            h.Username(appConfig.Artemis.User);
                            h.Password(appConfig.Artemis.PW);
                        });
                    cfg.EnableArtemisCompatibility();
                    cfg.UseRawJsonSerializer(MassTransit.Serialization.RawSerializerOptions.CopyHeaders);
                    cfg.Durable = true;
                    cfg.PrefetchCount = int.Parse(appConfig.Artemis.PrefetchSize);
                    cfg.ConfigureEndpoints(context);
                });
            }
            else if (appConfig.RabbitMq != null)
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();

                    cfg.Host(appConfig.RabbitMq.HostAddress, appConfig.RabbitMq.VirtualHost, h =>
                    {
                        h.Username(appConfig.RabbitMq.Username);
                        h.Password(appConfig.RabbitMq.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
                throw new ApplicationException("Invalid Bus configuration. Couldn't find Artemis, Amazon SQS, or RabbitMq config");

            x.AddOptions<MassTransitHostOptions>()
                    .Configure(options => { options.WaitUntilStarted = true; });
        });

        services.AddHostedService<Worker>(); 
        services.AddScoped<IBatchSubmitter, BatchSubmitter>();
        
    })
    .Build();

await host.RunAsync();
