using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.TlsRpt.EntityHistory.Test
{
    [TestFixture]
    public class RuaValidatorTests
    {
        private ITlsRptRuaValidator _ruaValidator = new TlsRptRuaValidator();
       

        [Test]
        public void TlsRptRecordWithOneValidRuaTag()
        {
            string ruaEmail = "test1234567@tls-rua.mailcheck.service.ncsc.gov.uk";
            string tlsRptRecord = $"v=TLSRPTv1;rua=mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(tlsRptRecord);
            Assert.That(result.Valid, Is.True);
            Assert.That(result.Tokens.Count, Is.EqualTo(1));
            Assert.That(result.Tokens[0], Is.EqualTo("test1234567"));
        }


        [Test]
        public void TlsRptRecordWithTwoValidRuaTag()
        {
            string ruaEmail1 = "test1234567@tls-rua.mailcheck.service.ncsc.gov.uk";
            string ruaEmail2 = "test1234566@tls-rua.mailcheck.service.ncsc.gov.uk";
            string tlsRptRecord = $"v=TLSRPTv1;rua=mailto:{ruaEmail1},mailto:{ruaEmail2};";
            RuaResult result = _ruaValidator.Validate(tlsRptRecord);
            Assert.That(result.Valid, Is.True);
            Assert.That(result.Tokens.Count, Is.EqualTo(2));
            Assert.That(result.Tokens[0], Is.EqualTo("test1234567"));
            Assert.That(result.Tokens[1], Is.EqualTo("test1234566"));
        }

        [Test]
        public void TlsRptRecordWithInvalidRuaTag()
        {
            string ruaEmail = "tlsrpt-rua@tlsrpt.service.gov.uk";
            string tlsRptRecord = $"v=TLSRPTv1;rua=mailto:{ruaEmail},mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(tlsRptRecord);
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Tokens.Count, Is.EqualTo(0));
        }

        [Test]
        public void TlsRptRecordWitInvalidRuaTagLessThan11CharactersToken()
        {
            string ruaEmail = "test12345@tls-rua.mailcheck.service.ncsc.gov.uk";
            string tlsRptRecord = $"v=TLSRPTv1;rua=mailto:{ruaEmail};";
            RuaResult result = _ruaValidator.Validate(tlsRptRecord);
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Tokens.Count, Is.EqualTo(0));
        }
    }
}