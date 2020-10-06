using MailCheck.TlsRpt.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Parsing
{
    [TestFixture]
    public class MailToUriValidatorTests
    {
        private MailToUriValidator _uriValidator;

        [SetUp]
        public void SetUp()
        {
            _uriValidator = new MailToUriValidator();
        }

        [TestCase("mailto:abc@cba.com", true, TestName = "Valid mailto returns true.")]
        [TestCase("mailto:abccba.com", false, TestName = "Invalid mailto returns false.")]
        [TestCase("mailo:abc@cba.com", false, TestName = "Invalid mailto prefix returns false.")]
        public void Test(string uri, bool expected)
        {
            bool actual = _uriValidator.IsValidUri(uri);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}