namespace MVE;

public class LevelLoadFailedException : Exception
{
    public LevelLoadFailedException(string section, string details, Exception? innerException = null)
        : base($"[{section}] {details}", innerException)
    {

    }
}
