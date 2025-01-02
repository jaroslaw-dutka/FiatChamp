namespace FiatChamp.Fiat;

public interface IFiatResponse
{
    bool CheckForError();

    void ThrowOnError(string message);
}