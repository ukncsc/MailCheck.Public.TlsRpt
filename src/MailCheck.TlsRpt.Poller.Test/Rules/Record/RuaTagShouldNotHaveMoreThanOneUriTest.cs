using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Rules.Record
{
    [TestFixture]
    public class RuaTagShouldNotHaveMoreThanOneUriTest
    {
        private RuaTagShouldNotHaveMoreThanOneUri _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RuaTagShouldNotHaveMoreThanOneUri();
        }

        [Test]
        public async Task OneUriNoError()
        {
            List<Error> errors = await _rule.Evaluate(Create(new RuaTag(string.Empty, new List<Uri>
            {
                new MailToUri(string.Empty)
            })));

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public async Task TwoUrisError()
        {
            List<Error> errors = await _rule.Evaluate(Create(new RuaTag(string.Empty, new List<Uri>
            {
                new MailToUri(string.Empty),
                new MailToUri(string.Empty)
            })));

            Assert.That(errors.Count, Is.EqualTo(1));
        }

        private TlsRptRecord Create(params Tag[] tags)
        {
            return new TlsRptRecord(string.Empty, new List<string>(), tags.ToList());
        }
    }
}
