using MailCheck.TlsRpt.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Parsing
{
    [TestFixture]
    public class HttpsUriValidatorTests
    {
        private HttpsUriValidator _uriValidator;

        [SetUp]
        public void SetUp()
        {
            _uriValidator = new HttpsUriValidator();
        }

        [TestCase("https://cba.com", true, TestName = "Valid https returns true.")]
        [TestCase("https://com", false, TestName = "Invalid https returns false.")]
        [TestCase("htps://cba.com", false, TestName = "Invalid https prefix returns false.")]
        public void Test(string uri, bool expected)
        {
            bool actual = _uriValidator.IsValidUri(uri);
            Assert.That(actual, Is.EqualTo(expected));
        }
        
    }
}
