namespace FiatChamp.Fiat.Model;

public interface IFiatResponse
{
    bool CheckForError();

    void ThrowOnError(string message);
}