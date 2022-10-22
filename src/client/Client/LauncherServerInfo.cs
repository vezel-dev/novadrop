namespace Vezel.Novadrop.Client;

public sealed class LauncherServerInfo
{
    public int Id { get; }

    public int Characters { get; }

    public LauncherServerInfo(int id, int characters)
    {
        Check.Range(id > 0, id);
        Check.Range(characters >= 0, characters);

        Id = id;
        Characters = characters;
    }

    public override string ToString()
    {
        return $"{{Id: {Id}, Characters: {Characters}}}";
    }
}
