namespace Vezel.Novadrop.Client;

public sealed class ClientServerInfo
{
    public int Id { get; }

    public string Category { get; }

    public string RawName { get; }

    public string Name { get; }

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
        string rawName,
        string name,
        string queue,
        string population,
        bool available,
        string unavailableMessage,
        string? host,
        IPAddress? address,
        int port)
    {
        _ = id > 0 ? true : throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(rawName);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(unavailableMessage);
        _ = (host != null || address != null) && (host == null || address == null) ?
            true : throw new ArgumentException(null);
        _ = address?.AddressFamily is null or AddressFamily.InterNetwork ?
            true : throw new ArgumentException(null, nameof(address));
        _ = port is >= IPEndPoint.MinPort and <= IPEndPoint.MaxPort ?
            true : throw new ArgumentOutOfRangeException(nameof(port));

        Id = id;
        Category = category;
        RawName = rawName;
        Name = name;
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
        return $"{{Id: {Id}, Name: {RawName}, Endpoint: {Host ?? Address!.ToString()}:{Port}}}";
    }
}
