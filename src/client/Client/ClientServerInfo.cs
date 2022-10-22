namespace Vezel.Novadrop.Client;

public sealed class ClientServerInfo
{
    public int Id { get; }

    public string Category { get; }

    public string Name { get; }

    public string Title { get; }

    public string Queue { get; }

    public string Population { get; }

    public bool IsAvailable { get; }

    public string UnavailableMessage { get; }

    public string? Host { get; }

    public IPAddress? Address { get; }

    public int Port { get; }

    public ClientServerInfo(
        int id,
        string category,
        string name,
        string title,
        string queue,
        string population,
        bool available,
        string unavailableMessage,
        string? host,
        IPAddress? address,
        int port)
    {
        Check.Range(id > 0, id);
        Check.Null(category);
        Check.Null(name);
        Check.Null(name);
        Check.Null(title);
        Check.Null(queue);
        Check.Null(population);
        Check.Null(unavailableMessage);
        Check.Argument((host, address) is (not null, null) or (null, not null));
        Check.Argument(address?.AddressFamily is null or AddressFamily.InterNetwork, address);
        Check.Argument(port is >= IPEndPoint.MinPort and <= IPEndPoint.MaxPort, port);

        Id = id;
        Category = category;
        Name = name;
        Title = title;
        Queue = queue;
        Population = population;
        IsAvailable = available;
        UnavailableMessage = unavailableMessage;
        Host = host;
        Address = address;
        Port = port;
    }

    public override string ToString()
    {
        return $"{{Id: {Id}, Name: {Name}, Endpoint: {Host ?? Address!.ToString()}:{Port}}}";
    }
}
