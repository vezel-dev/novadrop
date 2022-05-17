namespace Vezel.Novadrop;

// TODO: Use System.Diagnostics.UnreachableException in .NET 7.
[SuppressMessage("", "CA1032")]
[SuppressMessage("", "CA1064")]
sealed class UnreachableException : Exception
{
    public UnreachableException()
        : base("Unreachable code executed.")
    {
    }
}
