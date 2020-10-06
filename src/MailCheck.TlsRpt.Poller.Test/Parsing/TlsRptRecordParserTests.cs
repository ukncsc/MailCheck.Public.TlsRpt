using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Parsing
{
    [TestFixture]
    public class TlsRptRecordParserTests
    {
        private const string TagKey = "abc";

        private TlsRptRecordParser _tlsRptRecordParser;
        private ITagParser _tagParser;

        [SetUp]
        public void SetUp()
        {
            _tagParser = new VersionParser(); 
            _tlsRptRecordParser = new TlsRptRecordParser(new []{_tagParser});
        }

        [Test]
        public void ValidTagParsedCorrectly()
        {
            string record = "v=TLSRPTv1";

            TlsRptRecordInfo tlsRptRecordInfo = new TlsRptRecordInfo(string.Empty, new List<string> {record});

            EvaluationResult<TlsRptRecord> result = _tlsRptRecordParser.Parse(tlsRptRecordInfo);

            Assert.That(result.Item.Tags.Count, Is.EqualTo(1));
            Assert.That(result.Item.Tags.First(), Is.TypeOf<VersionTag>());
            Assert.That(result.Errors, Is.Empty);
        }

        [Test]
        public void MalformedTagGeneratesMalformedTagError()
        {
            string record = "v=TLSRPTv1=TLSRPTv1";

            TlsRptRecordInfo tlsRptRecordInfo = new TlsRptRecordInfo(string.Empty, new List<string> { record });

            EvaluationResult<TlsRptRecord> result = _tlsRptRecordParser.Parse(tlsRptRecordInfo);

            Assert.That(result.Item.Tags.Count, Is.EqualTo(1));
            Assert.That(result.Item.Tags.First(), Is.TypeOf<MalformedTag>());
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First(), Is.TypeOf<MalformedTagError>());
        }

        [Test]
        public void UnknownTagGeneratesUnknownTagError()
        {
            string record = "rua=mailto:abc@cba.com";

            TlsRptRecordInfo tlsRptRecordInfo = new TlsRptRecordInfo(string.Empty, new List<string> { record });

            EvaluationResult<TlsRptRecord> result = _tlsRptRecordParser.Parse(tlsRptRecordInfo);

            Assert.That(result.Item.Tags.Count, Is.EqualTo(1));
            Assert.That(result.Item.Tags.First(), Is.TypeOf<UnknownTag>());
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First(), Is.TypeOf<UnknownTagError>());
        }

        [Test]
        public void MaxOccurrencesExceededGeneratesMaxOccurrencesExceededError()
        {
            string record = "v=TLSRPTv1;v=TLSRPTv1";

            TlsRptRecordInfo tlsRptRecordInfo = new TlsRptRecordInfo(string.Empty, new List<string> { record });

            EvaluationResult<TlsRptRecord> result = _tlsRptRecordParser.Parse(tlsRptRecordInfo);

            Assert.That(result.Item.Tags.Count, Is.EqualTo(2));
            Assert.That(result.Item.Tags[0], Is.TypeOf<VersionTag>());
            Assert.That(result.Item.Tags[1], Is.TypeOf<VersionTag>());
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Is.TypeOf<MaxOccurrencesExceededError>());
        }

        [TestCase(" v=TLSRPTv1;", TestName = "Leading white space parsed correctly")]
        [TestCase("v=TLSRPTv1 ;", TestName = "Leading white space on delimiter parsed correctly")]
        [TestCase("v=TLSRPTv1; ", TestName = "Trailing white space on delimiter parsed correctly")]
        public void Test(string record)
        {
            TlsRptRecordInfo tlsRptRecordInfo = new TlsRptRecordInfo(string.Empty, new List<string> { record });

            EvaluationResult<TlsRptRecord> result = _tlsRptRecordParser.Parse(tlsRptRecordInfo);

            Assert.That(result.Item.Tags.Count, Is.EqualTo(1));
            Assert.That(result.Item.Tags.First(), Is.TypeOf<VersionTag>());
            Assert.That(result.Errors, Is.Empty);
        }
    }
}
