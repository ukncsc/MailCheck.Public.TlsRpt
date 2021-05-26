using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Evaluator.Rules;
using NUnit.Framework;
using Uri = MailCheck.TlsRpt.Contracts.SharedDomain.Uri;

namespace MailCheck.TlsRpt.Evaluator.Test.Rules
{
    [TestFixture]
    public class RuaTagsShouldContainTlsRptServiceMailBoxTest
    {
        private RuaTagsShouldContainTlsRptServiceMailBox _rule;
        private const string Id = "ncsc.gov.uk";

        [SetUp]
        public void SetUp()
        {
            _rule = new RuaTagsShouldContainTlsRptServiceMailBox();
        }

        public async Task Test(TlsRptRecord tlsRptRecord, bool isErrorExpected, MessageType? expectedError = null,
            string markDown = null)
        {
            List<Message> messages = await _rule.Evaluate(tlsRptRecord);

            Assert.That(messages.Any(), Is.EqualTo(isErrorExpected));

            Assert.That(messages.FirstOrDefault()?.MessageType, Is.EqualTo(expectedError));

            if (markDown != null)
            {
                Assert.That(messages.FirstOrDefault()?.MarkDown, Is.EqualTo(markDown));
            }
        }

        [Test]
        public async Task NoErrorWhenCorrectMailboxIsUsed()
        {
            RuaTag tag = new RuaTag("rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk",
                new List<Uri> {new MailToUri("mailto:tls-rua@mailcheck.service.ncsc.gov.uk")});


            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk;"
                }, new List<Tag> {tag});


            await Test(testTlsRptRecord, false);
        }


        [Test]
        public async Task WarningWhenSameMailboxMentionedMoreThanOnce()
        {
            RuaTag tag = new RuaTag("rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk; mailto:a@b.com; mailto:a@b.com",
                new List<Uri>
                {
                    new MailToUri("mailto:a@b.com"), new MailToUri("mailto:a@b.com"),
                    new MailToUri("mailto:tls-rua@mailcheck.service.ncsc.gov.uk")
                });


            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk,mailto:a@b.com,mailto:a@b.com;"
                }, new List<Tag> {tag});

            await Test(testTlsRptRecord, true, MessageType.warning);
        }

        [Test]
        public async Task ErrorWhenIncorrectMailCheckMailbox()
        {
            RuaTag tag = new RuaTag("rua=mailto:rua@mailcheck.service.ncsc.gov.uk",
                new List<Uri> {new MailToUri("mailto:rua@mailcheck.service.ncsc.gov.uk")});

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    tag.RawValue
                }, new List<Tag> {tag});
            await Test(testTlsRptRecord, true, MessageType.error);
        }

        [Test]
        public async Task WarningWhenNoUris()
        {
            RuaTag tag = new RuaTag("rua=;", new List<Uri>());

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    tag.RawValue
                }, new List<Tag> {tag});

            await Test(testTlsRptRecord, true, MessageType.info);
        }

        [Test]
        public void NoExceptionWhenNullUri()
        {
            RuaTag tag = new RuaTag("rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk",
                new List<Uri>
                    {new MailToUri("mailto:tls-rua@mailcheck.service.ncsc.gov.uk"), null});

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    tag.RawValue
                }, new List<Tag> {tag});

            Assert.DoesNotThrowAsync(async () => await Test(testTlsRptRecord, false));
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown1()
        {
            string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com";

            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri> {new MailToUri("mailto:tlsrpt@example.com")});

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id, new List<string>(), new List<Tag> {tag});

            string actualMarkdown = (await _rule.Evaluate(testTlsRptRecord)).First().MarkDown;

            string expectedMarkdown =
                $"The TLS-RPT record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate encrypted email deliverability, and you won't see any reporting in Mail Check.{Environment.NewLine}{Environment.NewLine}If you would like Mail Check to receive a copy of your reports, then please change your record to:{Environment.NewLine}{Environment.NewLine}`rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForMultipleIncorrectMailCheckUriHasCorrectlyFormattedMarkdown()
        {
            string record =
                "v=TLSRPTv1;rua=mailto:tlsrpt@example.com, mailto:tlsrpt@example.com, mailto:tlsrpt@example.com";

            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com"), new MailToUri("mailto:tlsrpt@example.com"),
                    new MailToUri("mailto:tlsrpt@example.com")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id, new List<string>(), new List<Tag> {tag});

            string actualMarkdown = (await _rule.Evaluate(testTlsRptRecord)).First().MarkDown;

            string expectedMarkdown =
                $"The TLS-RPT record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate encrypted email deliverability, and you won't see any reporting in Mail Check.{Environment.NewLine}{Environment.NewLine}If you would like Mail Check to receive a copy of your reports, then please change your record to:{Environment.NewLine}{Environment.NewLine}`rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task NoErrorWhenMalformedUriTest()
        {
            string record =
                "v=TLSRPTv1;rua=mailto:tlsrpt@example.com, mailto:tlsrpt@example.com, mailto:tlsrpt@example.com";

            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com"), new MailToUri("mailto:tlsrpt@example.com"),
                    new MalformedUri("mailto:tlsrpt@example.com:test")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id, new List<string>(), new List<Tag> {tag});

            List<Message> result = await _rule.Evaluate(testTlsRptRecord);

            Assert.That(result.Count, Is.EqualTo(0));
        }


        [Test]
        public async Task NoErrorWhenUnknownUriTest()
        {
            string record =
                "v=TLSRPTv1;rua=mailto:tlsrpt@example.com, mailto:tlsrpt@example.com, mailto:tlsrpt@example.com";

            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com"), new MailToUri("mailto:tlsrpt@example.com"),
                    new UnknownUri("abc:abc@cba.com")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id, new List<string>(), new List<Tag> {tag});

            List<Message> result = await _rule.Evaluate(testTlsRptRecord);

            Assert.That(result.Count, Is.EqualTo(0));
        }


        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown2()
        {
            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com"),
                    new MailToUri("mailto:tls-rpt@mailcheck.service.ncsc.gov.uk")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string> {"v=TLSRPTv1;", "rua=mailto:tlsrpt@example.com"}, new List<Tag> {tag});

            string actualMarkdown = (await _rule.Evaluate(testTlsRptRecord)).First().MarkDown;

            string expectedMarkdown =
                $"Your TLS-RPT record contains the wrong email address for Mail Check aggregate report processing.{Environment.NewLine}{Environment.NewLine}Please change your TLS-RPT record to be:{Environment.NewLine}{Environment.NewLine}`v=TLSRPTv1;rua=mailto:tlsrpt@example.com`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved1()
        {
            RuaTag tag = new RuaTag(
                "rua=rua=mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:tls-rpt@mailcheck.service.ncsc.gov.uk;",
                new List<Uri>
                {
                    new MailToUri("mailto:tls-rpt@mailcheck.service.ncsc.gov.uk"),
                    new MailToUri("mailto:tls-rpt@mailcheck.service.ncsc.gov.uk")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:tls-rpt@mailcheck.service.ncsc.gov.uk;"
                }, new List<Tag> {tag});

            string actualMarkdown = (await _rule.Evaluate(testTlsRptRecord)).First().MarkDown;

            string expectedMarkdown =
                $"Your TLS-RPT record contains the wrong email address for Mail Check aggregate report processing.{Environment.NewLine}{Environment.NewLine}Please change your TLS-RPT record to be:{Environment.NewLine}{Environment.NewLine}`v=TLSRPTv1;rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved2()
        {
            RuaTag tag = new RuaTag(
                "rua=rua=mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:0007756a@mxtoolbox.tls-report.com;",
                new List<Uri>
                {
                    new MailToUri("mailto:tls-rpt@mailcheck.service.ncsc.gov.uk"),
                    new MailToUri("mailto:tls-rpt@mailcheck.service.ncsc.gov.uk"),
                    new MailToUri("mailto:0007756a@mxtoolbox.tls-report.com")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:tls-rpt@mailcheck.service.ncsc.gov.uk,mailto:0007756a@mxtoolbox.tls-report.com;"
                }, new List<Tag> {tag});

            string actualMarkdown = (await _rule.Evaluate(testTlsRptRecord)).First().MarkDown;

            string expectedMarkdown =
                $"Your TLS-RPT record contains the wrong email address for Mail Check aggregate report processing.{Environment.NewLine}{Environment.NewLine}Please change your TLS-RPT record to be:{Environment.NewLine}{Environment.NewLine}`v=TLSRPTv1;rua=mailto:tls-rua@mailcheck.service.ncsc.gov.uk,mailto:0007756a@mxtoolbox.tls-report.com;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }


        [Test]
        public async Task NoErrorWhenNewRuaMailboxFoundWithOtherInvalidMailbox()
        {
            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com"),
                    new MailToUri("mailto:tls-rpt@tls-rua.mailcheck.service.ncsc.gov.uk")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tlsrpt@example.com,mailto:tls-rpt@tls-rua.mailcheck.service.ncsc.gov.uk"
                }, new List<Tag> {tag});

            List<Message> result = await _rule.Evaluate(testTlsRptRecord);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ErrorWhenNoNewOrOriginalRuaMailboxFound()
        {
            RuaTag tag = new RuaTag("rua=mailto:tlsrpt@example.com",
                new List<Uri>
                {
                    new MailToUri("mailto:tlsrpt@example.com")
                });

            TlsRptRecord testTlsRptRecord = new TlsRptRecord(Id,
                new List<string>
                {
                    "v=TLSRPTv1;",
                    "rua=mailto:tlsrpt@example.com"
                }, new List<Tag> {tag});

            List<Message> result = await _rule.Evaluate(testTlsRptRecord);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Text,
                Is.EqualTo(
                    "Mail Check can only provide email reporting if the aggregate report URI tag (rua) includes the tls-rua@mailcheck.service.ncsc.gov.uk mailbox. Consider adding `mailto:tls-rua@mailcheck.service.ncsc.gov.uk` to the rua tag values."));
        }
    }
}