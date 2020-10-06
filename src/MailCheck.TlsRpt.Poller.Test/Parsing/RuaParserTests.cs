using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Parsing
{
    [TestFixture]
    public class RuaParserTests
    {
        private RuaParser _ruaParser;
        private IMailToUriValidator _mailToUriValidator;

        [SetUp]
        public void SetUp()
        {
            _mailToUriValidator = A.Fake<IMailToUriValidator>();
            IUriParser uriParser = new MailToRuaParser(_mailToUriValidator);

            _ruaParser = new RuaParser(new [] { uriParser });
        }

        [Test]
        public void ValidUriIsCorrectlyParsed()
        {
            List<Tag> tags = new List<Tag>();
            string tagKey = "rua";
            string tagValue = "mailto:abc@cba.com";
            string token = $"{tagKey}={tagValue}";
            string record = $"v=TLSRPTv1;{token}";

            A.CallTo(() => _mailToUriValidator.IsValidUri(A<string>._)).Returns(true);

            EvaluationResult<Tag> evaluationResult = _ruaParser.Parse(tags, record, token, tagKey, tagValue);

            RuaTag ruaTag = evaluationResult.Item as RuaTag;

            Assert.That(ruaTag, Is.Not.Null);
            Assert.That(ruaTag.RawValue, Is.EqualTo(token));
            Assert.That(ruaTag.Uris.Count, Is.EqualTo(1));
            Assert.That(ruaTag.Uris.First(), Is.TypeOf<MailToUri>());
            Assert.That(ruaTag.Uris.First().Value, Is.EqualTo(tagValue));
            Assert.That(evaluationResult.Errors, Is.Empty);
        }

        [TestCase("mailto:abc@cba.com, mailto:def@cba.com")]
        [TestCase("mailto:abc@cba.com,mailto:def@cba.com")]
        public void ValidMultipleUriIsCorrectlyParsed(string tagValue)
        {
            List<Tag> tags = new List<Tag>();
            string tagKey = "rua";
            string token = $"{tagKey}={tagValue}";
            string record = $"v=TLSRPTv1;{token}";

            A.CallTo(() => _mailToUriValidator.IsValidUri(A<string>._)).Returns(true);

            EvaluationResult<Tag> evaluationResult = _ruaParser.Parse(tags, record, token, tagKey, tagValue);

            RuaTag ruaTag = evaluationResult.Item as RuaTag;

            Assert.That(ruaTag, Is.Not.Null);
            Assert.That(ruaTag.RawValue, Is.EqualTo(token));
            Assert.That(ruaTag.Uris.Count, Is.EqualTo(2));
            Assert.That(ruaTag.Uris[0], Is.TypeOf<MailToUri>());
            Assert.That(ruaTag.Uris[0].Value, Is.EqualTo("mailto:abc@cba.com"));
            Assert.That(ruaTag.Uris[1], Is.TypeOf<MailToUri>());
            Assert.That(ruaTag.Uris[1].Value, Is.EqualTo("mailto:def@cba.com"));
            Assert.That(evaluationResult.Errors, Is.Empty);
        }

        [Test]
        public void MalformedUriGeneratesMalformedUriAndMalformedUriError()
        {
            List<Tag> tags = new List<Tag>();
            string tagKey = "rua";
            string tagValue = "mailto:abc@cba.com:test";
            string token = $"{tagKey}={tagValue}";
            string record = $"v=TLSRPTv1;{token}";

            EvaluationResult<Tag> evaluationResult = _ruaParser.Parse(tags, record, token, tagKey, tagValue);

            RuaTag ruaTag = evaluationResult.Item as RuaTag;

            Assert.That(ruaTag, Is.Not.Null);
            Assert.That(ruaTag.RawValue, Is.EqualTo(token));
            Assert.That(ruaTag.Uris.Count, Is.EqualTo(1));
            Assert.That(ruaTag.Uris.First(), Is.TypeOf<MalformedUri>());
            Assert.That(ruaTag.Uris.First().Value, Is.EqualTo(tagValue));
            Assert.That(evaluationResult.Errors.Count, Is.EqualTo(1));
            Assert.That(evaluationResult.Errors.First(), Is.TypeOf<MalformedUriError>());
            Assert.That(evaluationResult.Errors[0].Markdown, Is.Not.Null);
        }



        [Test]
        public void UnknownUriGeneratesUnknownUriAndUnknownUriError()
        {
            List<Tag> tags = new List<Tag>();
            string tagKey = "rua";
            string tagValue = "abc:abc@cba.com";
            string token = $"{tagKey}={tagValue}";
            string record = $"v=TLSRPTv1;{token}";

            EvaluationResult<Tag> evaluationResult = _ruaParser.Parse(tags, record, token, tagKey, tagValue);

            RuaTag ruaTag = evaluationResult.Item as RuaTag;

            Assert.That(ruaTag, Is.Not.Null);
            Assert.That(ruaTag.RawValue, Is.EqualTo(token));
            Assert.That(ruaTag.Uris.Count, Is.EqualTo(1));
            Assert.That(ruaTag.Uris.First(), Is.TypeOf<UnknownUri>());
            Assert.That(ruaTag.Uris.First().Value, Is.EqualTo(tagValue));
            Assert.That(evaluationResult.Errors.Count, Is.EqualTo(1));
            Assert.That(evaluationResult.Errors.First(), Is.TypeOf<UnknownUriError>());
        }
    }
}
