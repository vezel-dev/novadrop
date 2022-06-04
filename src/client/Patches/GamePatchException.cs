namespace Vezel.Novadrop.Patches;

public class GamePatchException : Exception
{
    public GamePatchException()
        : this("An unknown memory patching error occurred.")
    {
    }

    public GamePatchException(string? message)
        : base(message)
    {
    }

    public GamePatchException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
