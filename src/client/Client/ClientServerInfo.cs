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
        _ = id > 0 ? true : throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(title);
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
