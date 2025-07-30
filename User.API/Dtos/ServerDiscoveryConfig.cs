using System.Net;
namespace User.API.Dtos;

public class ServerDiscoveryConfig
{
    public bool UseHttps { get; set; }
    public string ServerName { get; set; }
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