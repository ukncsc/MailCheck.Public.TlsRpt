using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Deserialisation;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Serialisation
{
    [TestFixture]
    public class PayloadDeserializationTests
    {
        private const string Payload = "{\"tlsRptRecords\":{\"domain\":\"ncsc.gov.uk\",\"records\":[{\"domain\":\"ncsc.gov.uk\",\"recordsParts\":[\"v=TLSRPTv1;rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk\"],\"tags\":[{\"value\":\"TLSRPTv1\",\"type\":\"VersionTag\",\"rawValue\":\"v=TLSRPTv1\"},{\"uris\":[{\"type\":\"MailToUri\",\"value\":\"mailto:tls-rua@mailcheck.service.ncsc.gov.uk\"}],\"type\":\"RuaTag\",\"rawValue\":\"rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk\"}]}],\"messageSize\":123},\"errors\":[],\"id\":\"ncsc.gov.uk\",\"correlationId\":null,\"causationId\":\"8a0ee9e9-13ae-4e7e-8580-c8a059aa056a\",\"messageId\":null,\"timestamp\":\"0001-01-01T00:00:00\"}";

        [Test]
        public void CanDeserialise()
        {
            TlsRptPollResult tlsRptPollResult = JsonConvert.DeserializeObject<TlsRptPollResult>(Payload, SerialisationConfig.Settings);

            Assert.That(tlsRptPollResult, Is.Not.Null);
        }
    }
}
