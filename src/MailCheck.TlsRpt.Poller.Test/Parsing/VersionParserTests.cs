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
    public class VersionParserTests
    {
        private VersionParser _versionParser;

        [SetUp]
        public void SetUp()
        {
            _versionParser = new VersionParser();
        }

        [Test]
        public void ValidTagGeneratesResultWithNoErrors()
        {
            string value = "TLSRPTv1";
            string key = "v";
            string token = $"{key}={value}";
            string record = $"{token};rua=mailto:abc@cba.com";

            EvaluationResult<Tag> evaluationResult = 
                _versionParser.Parse(new List<Tag>(), record, token, key, value);

            VersionTag versionTag = evaluationResult.Item as VersionTag;

            Assert.That(versionTag, Is.Not.Null);
            Assert.That(versionTag.RawValue, Is.EqualTo(token));
            Assert.That(versionTag.Value, Is.EqualTo(value));
            Assert.That(evaluationResult.Errors, Is.Empty);
        }

        [Test]
        public void RecordDoesNotStartWithVersionGeneratesInvalidVersionError()
        {
            string value = "TLSRPTv1";
            string key = "p";
            string token = $"{key}={value}";
            string record = $"{token};rua=mailto:abc@cba.com";

            EvaluationResult<Tag> evaluationResult =
                _versionParser.Parse(new List<Tag>(), record, token, key, value);

            VersionTag versionTag = evaluationResult.Item as VersionTag;

            Assert.That(versionTag, Is.Not.Null);
            Assert.That(versionTag.RawValue, Is.EqualTo(token));
            Assert.That(versionTag.Value, Is.EqualTo(value));
            Assert.That(evaluationResult.Errors.Count, Is.EqualTo(1));
            Assert.That(evaluationResult.Errors.First(), Is.TypeOf<InvalidVersionTagError>());
        }

        [Test]
        public void InvalidVersionErrorIsOnlyGeneratedOnce()
        {
            string value = "TLSRPTv1";
            string key = "p";
            string token = $"{key}={value}";
            string record = $"{token};rua=mailto:abc@cba.com";

            EvaluationResult<Tag> evaluationResult =
                _versionParser.Parse(new List<Tag>{new VersionTag("", "")}, record, token, key, value);

            VersionTag versionTag = evaluationResult.Item as VersionTag;

            Assert.That(versionTag, Is.Not.Null);
            Assert.That(versionTag.RawValue, Is.EqualTo(token));
            Assert.That(versionTag.Value, Is.EqualTo(value));
            Assert.That(evaluationResult.Errors, Is.Empty);
        }
    }
}
