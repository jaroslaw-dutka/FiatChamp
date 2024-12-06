public abstract class FiatBaseResponse
{
    public abstract bool CheckForError();

    public abstract void ThrowOnError(string message);
}