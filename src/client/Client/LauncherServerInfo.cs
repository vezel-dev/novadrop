namespace Vezel.Novadrop.Client;

public sealed class LauncherServerInfo
{
    public int Id { get; }

    public int Characters { get; }

    public LauncherServerInfo(int id, int characters)
    {
        _ = id > 0 ? true : throw new ArgumentOutOfRangeException(nameof(id));
        _ = characters >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(characters));

        Id = id;
        Characters = characters;
    }

    public override string ToString()
    {
        return $"{{Id: {Id}, Characters: {Characters}}}";
    }
}
