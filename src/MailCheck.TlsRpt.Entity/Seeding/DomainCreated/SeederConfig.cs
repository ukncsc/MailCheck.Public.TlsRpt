namespace MailCheck.TlsRpt.Entity.Seeding.DomainCreated
{
    internal interface ISeederConfig
    {
        string SnsTopicToSeedArn { get; }
    }

    internal class SeederConfig : ISeederConfig
    {
        public SeederConfig(string queueToSeedUrl)
        {
            SnsTopicToSeedArn = queueToSeedUrl;
        }

        public string SnsTopicToSeedArn { get; }
    }
}