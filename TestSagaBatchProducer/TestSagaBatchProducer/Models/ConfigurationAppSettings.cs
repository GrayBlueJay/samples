namespace TestSagaBatchProducer.Models;
public class ConfigurationAppSettings
{
    public string AppName { get; set; }
    public ArtemisSettings Artemis { get; set; }
    public AmazonSqsSettings AmazonSqs { get; set; }
    public RabbitMqSettings RabbitMq { get; set; }
}

public class ArtemisSettings
{
    public string HostAddress { get; set; }
    public string HostPort { get; set; }
    public string User { get; set; }
    public string PW { get; set; }
    public string PrefetchSize { get; set; }
}

public class AmazonSqsSettings
{
    public string Region { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}

public class RabbitMqSettings
{
    public string HostAddress { get; set; }
    public int Port { get; private set; }
    public string VirtualHost { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}