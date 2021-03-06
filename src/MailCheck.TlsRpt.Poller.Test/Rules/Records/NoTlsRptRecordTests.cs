﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Rules.Records;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test.Rules.Records
{
    [TestFixture]
    public class NoTlsRptRecordTests
    {
        private NoTlsRptRecord _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new NoTlsRptRecord();
        }

        [Test]
        public async Task TlsRptRecordPresentNoErrors()
        {
            TlsRptRecords tlsRptRecords = CreateRecords(CreateRecord());

            List<Error> errors = await _rule.Evaluate(tlsRptRecords);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public async Task TlsRptRecordNotPresentGeneratesError()
        {
            TlsRptRecords tlsRptRecords = CreateRecords();

            List<Error> errors = await _rule.Evaluate(tlsRptRecords);

            Assert.That(errors.Count, Is.EqualTo(1));
        }

        private TlsRptRecords CreateRecords(params TlsRptRecord[] records)
        {
            return new TlsRptRecords(string.Empty, records.ToList(), 10);
        }

        private TlsRptRecord CreateRecord()
        {
            return new TlsRptRecord(string.Empty, new List<string>(), new List<Tag>());
        }
    }
}
