namespace FiatChamp.Fiat;

public interface IFiatApiConfigFactory
{
    FiatApiConfig Create(FiatSettings settings);
}