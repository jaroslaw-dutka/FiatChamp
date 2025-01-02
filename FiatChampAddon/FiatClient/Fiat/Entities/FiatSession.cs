using Amazon.Runtime;

namespace FiatChamp.Fiat.Entities
{
    public class FiatSession
    {
        public string UserId { get; set; }
        public ImmutableCredentials AwsCredentials { get; set; }
    }
}
