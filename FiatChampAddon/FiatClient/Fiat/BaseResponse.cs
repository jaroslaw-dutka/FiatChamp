public abstract class BaseResponse
{
    public abstract bool CheckForError();

    public abstract void ThrowOnError(string message);
}