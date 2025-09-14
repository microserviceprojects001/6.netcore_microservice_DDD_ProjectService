using System.Net;

namespace Contact.API.Configuration;

public class ServerDiscoveryConfig
{
    public bool UseHttps { get; set; }

    public string UserServiceName { get; set; }
    public string ContactServiceName { get; set; }
    public string IdentityServiceName { get; set; }
    public ConsulConfig Consul { get; set; }
}

public class ConsulConfig
{
    public string HttpEndpoint { get; set; }
    public DnsEndpointConfig DnsEndpoint { get; set; }
}

public class DnsEndpointConfig
{
    public string Address { get; set; }
    public int Port { get; set; }

    public IPEndPoint ToIPEndPoint()
    {
        return new IPEndPoint(IPAddress.Parse(Address), Port);
    }
}