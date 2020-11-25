using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.TlsRpt.Poller.Domain;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Dns
{
    [TestFixture]
    public class DnsClientTests
    {
        private const string TlsRptRecord = "v=TLSRPTv1;rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk";

        private Poller.Dns.DnsClient _dnsClient;
        private ILookupClient _lookupClient;

        [SetUp]
        public void SetUp()
        {
            _lookupClient = A.Fake<ILookupClient>();
            _dnsClient = new Poller.Dns.DnsClient(_lookupClient, A.Fake<ILogger<Poller.Dns.DnsClient>>());
        }

        [Test]
        public async Task CorrecltlyObtainsTlsRptRecord()
        {
            IDnsQueryResponse response = CreateResponse(new List<string> { TlsRptRecord });

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(string.Empty);

            Assert.That(tlsRptRecordInfos.RecordsInfos.Count, Is.EqualTo(1));
            Assert.That(tlsRptRecordInfos.RecordsInfos[0].Record, Is.EqualTo(TlsRptRecord));
        }

        [Test]
        public async Task ReturnsErrorIfDnsQueryErrored()
        {
            const string error = "Dns call borked";

            IDnsQueryResponse response = CreateError(error);

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(string.Empty);

            Assert.That(tlsRptRecordInfos.RecordsInfos.Count, Is.EqualTo(0));
            Assert.That(tlsRptRecordInfos.Error.Message, Is.EqualTo(error));
        }

        [Test]
        public async Task DoenstReturnErrorForNxDomainResponse()
        {
            const string error = "Non-Existent Domain";

            IDnsQueryResponse response = CreateError(error);

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(string.Empty);

            Assert.That(tlsRptRecordInfos.RecordsInfos.Count, Is.EqualTo(0));
            Assert.That(tlsRptRecordInfos.Error, Is.Null);
        }

        [Test]
        public async Task DoesntReturnErrorForServerFailureResponse()
        {
            const string error = "Server Failure";

            IDnsQueryResponse response = CreateError(error);

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None)).Returns(response);

            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(string.Empty);

            Assert.That(tlsRptRecordInfos.RecordsInfos.Count, Is.EqualTo(0));
            Assert.That(tlsRptRecordInfos.Error, Is.Null);
        }

        [Test]
        public void ExceptionsArePropagated()
        {
            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, QueryClass.IN, CancellationToken.None))
                .Throws(new InvalidOperationException());

            Assert.ThrowsAsync<InvalidOperationException>(() => _dnsClient.GetTlsRptRecords(String.Empty));
        }

        private IDnsQueryResponse CreateResponse(params List<string>[] records)
        {
            List<DnsResourceRecord> txtRecords = records
                .Select(_ => new TxtRecord(new ResourceRecordInfo(string.Empty, ResourceRecordType.TXT, QueryClass.IN, 3600, 100), _.ToArray(), _.ToArray()))
                .Cast<DnsResourceRecord>()
                .ToList();

            IDnsQueryResponse response = A.Fake<IDnsQueryResponse>();

            A.CallTo(() => response.Answers).Returns(new ReadOnlyCollection<DnsResourceRecord>(txtRecords));

            return response;
        }

        private IDnsQueryResponse CreateError(string error)
        {
            IDnsQueryResponse response = A.Fake<IDnsQueryResponse>();
            A.CallTo(() => response.HasError).Returns(true);
            A.CallTo(() => response.ErrorMessage).Returns(error);

            A.CallTo(() => response.Answers).Returns(new ReadOnlyCollection<DnsResourceRecord>(new List<DnsResourceRecord>()));
            return response;
        }
    }
}
