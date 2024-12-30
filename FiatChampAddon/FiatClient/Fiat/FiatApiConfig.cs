using Amazon;
using FiatChamp.Fiat.Model;

namespace FiatChamp.Fiat;

public class FiatApiConfig
{
    public string LoginApiKey { get; set; }
    public string ApiKey { get; set; }
    public string AuthApiKey { get; set; }
    public string LoginUrl { get; set; }
    public string TokenUrl { get; set; }
    public string ApiUrl { get; set; }
    public string AuthUrl { get; set; }
    public string Locale { get; set; }
    public RegionEndpoint AwsEndpoint { get; set; }

    public FiatApiConfig()
    {
    }

    public FiatApiConfig(FiatSettings settings)
    {
        if (settings.Brand == FcaBrand.Ram)
        {
            LoginApiKey = "3_7YjzjoSb7dYtCP5-D6FhPsCciggJFvM14hNPvXN9OsIiV1ujDqa4fNltDJYnHawO";
            ApiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            AuthApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            LoginUrl = "https://login-us.ramtrucks.com";
            TokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-02.fcagcv.com";
            AuthUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            Locale = "en_us";
            AwsEndpoint = RegionEndpoint.USEast1;
        }
        else if (settings.Brand == FcaBrand.Dodge)
        {
            LoginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
            ApiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            AuthApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            LoginUrl = "https://login-us.dodge.com";
            TokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-02.fcagcv.com";
            AuthUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            Locale = "en_us";
            AwsEndpoint = RegionEndpoint.USEast1;
        }
        else if (settings is { Brand: FcaBrand.Fiat, Region: FcaRegion.Europe })
        {
            LoginApiKey = "3_mOx_J2dRgjXYCdyhchv3b5lhi54eBcdCTX4BI8MORqmZCoQWhA0mV2PTlptLGUQI";
            ApiKey = "2wGyL6PHec9o1UeLPYpoYa1SkEWqeBur9bLsi24i";
            AuthApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys";
            LoginUrl = "https://loginmyuconnect.fiat.com";
            TokenUrl = "https://authz.sdpr-01.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-01.fcagcv.com";
            AuthUrl = "https://mfa.fcl-01.fcagcv.com";
            Locale = "de_de";
            AwsEndpoint = RegionEndpoint.EUWest1;
        }
        else if (settings is { Brand: FcaBrand.Fiat, Region: FcaRegion.America })
        {
            LoginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
            ApiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            AuthApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            LoginUrl = "https://login-us.fiat.com";
            TokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-02.fcagcv.com";
            AuthUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            Locale = "en_us";
            AwsEndpoint = RegionEndpoint.USEast1;
        }
        else if (settings is { Brand: FcaBrand.Jeep, Region: FcaRegion.Europe })
        {
            LoginApiKey = "3_ZvJpoiZQ4jT5ACwouBG5D1seGEntHGhlL0JYlZNtj95yERzqpH4fFyIewVMmmK7j";
            ApiKey = "2wGyL6PHec9o1UeLPYpoYa1SkEWqeBur9bLsi24i";
            AuthApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys";
            LoginUrl = "https://login.jeep.com";
            TokenUrl = "https://authz.sdpr-01.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-01.fcagcv.com";
            AuthUrl = "https://mfa.fcl-01.fcagcv.com";
            Locale = "de_de";
            AwsEndpoint = RegionEndpoint.EUWest1;
        }
        else if (settings is { Brand: FcaBrand.Jeep, Region: FcaRegion.America })
        {
            LoginApiKey = "3_5qxvrevRPG7--nEXe6huWdVvF5kV7bmmJcyLdaTJ8A45XUYpaR398QNeHkd7EB1X";
            ApiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            LoginUrl = "https://login-us.jeep.com";
            TokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            ApiUrl = "https://channels.sdpr-02.fcagcv.com";
            AuthApiKey = "fNQO6NjR1N6W0E5A6sTzR3YY4JGbuPv48Nj9aZci";
            AuthUrl = "https://mfa.fcl-02.fcagcv.com";
            AwsEndpoint = RegionEndpoint.USEast1;
            Locale = "en_us";
        }
        else
            throw new ArgumentOutOfRangeException(nameof(settings.Brand));
    }
}