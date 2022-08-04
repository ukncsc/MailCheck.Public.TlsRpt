using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using NUnit.Framework;
using LocalNotifier = MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers.FindingsChangedNotifier;
using MailCheck.Common.Contracts.Findings;
using System.Linq;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class FindingsChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IFindingsChangedNotifier _findingsChangedNotifier;
        private ITlsRptReportsEntityConfig _tlsRptReportsEntityConfig;
        private ILogger<LocalNotifier> _logger;
        private LocalNotifier _notifier;

        private const string Id = "test.gov.uk";
        private MessageEqualityComparer _equalityComparer;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _findingsChangedNotifier = A.Fake<IFindingsChangedNotifier>();
            _tlsRptReportsEntityConfig = A.Fake<ITlsRptReportsEntityConfig>();
            _logger = A.Fake<ILogger<LocalNotifier>>();
            _notifier = new LocalNotifier(_messageDispatcher, _findingsChangedNotifier, _tlsRptReportsEntityConfig, _logger);
            _equalityComparer = new MessageEqualityComparer();
        }

        [TestCaseSource(nameof(ExerciseFindingsChangedNotifierTestPermutations))]
        public void ExerciseFindingsChangedNotifier(FindingsChangedNotifierTestCase testCase)
        {
            A.CallTo(() => _tlsRptReportsEntityConfig.WebUrl).Returns("testurl.com");

            _notifier.Handle(Id, testCase.CurrentAdvisories, testCase.IncomingAdvisories);

            A.CallTo(() => _findingsChangedNotifier.Process(
                "test.gov.uk",
                "TLS-RPT",
                A<List<Finding>>.That.Matches(x => x.SequenceEqual(testCase.ExpectedCurrentFindings, _equalityComparer)),
                A<List<Finding>>.That.Matches(x => x.SequenceEqual(testCase.ExpectedIncomingFindings, _equalityComparer))
            )).MustHaveHappenedOnceExactly();
        }

        private static IEnumerable<FindingsChangedNotifierTestCase> ExerciseFindingsChangedNotifierTestPermutations()
        {
            Finding findingEvalError1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.tlsrpt.testName1",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/tls-rpt",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingEvalError2 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.tlsrpt.testName2",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/tls-rpt",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingEvalWarn1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.tlsrpt.testName3",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/tls-rpt",
                Severity = "Advisory",
                Title = "EvaluationWarning"
            };

            ReportsAdvisoryMessage evalError1 = new ReportsAdvisoryMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName1", MessageType.error, "EvaluationError", string.Empty, string.Empty);
            ReportsAdvisoryMessage evalError2 = new ReportsAdvisoryMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName2", MessageType.error, "EvaluationError", string.Empty, string.Empty);
            ReportsAdvisoryMessage evalWarn1 = new ReportsAdvisoryMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName3", MessageType.warning, "EvaluationWarning", string.Empty, string.Empty);


            FindingsChangedNotifierTestCase test1 = new FindingsChangedNotifierTestCase
            {
                CurrentAdvisories = new List<ReportsAdvisoryMessage> { evalError1, evalError2, evalWarn1 },
                IncomingAdvisories = new List<ReportsAdvisoryMessage>(),
                ExpectedCurrentFindings = new List<Finding> { findingEvalError1, findingEvalError2, findingEvalWarn1 },
                ExpectedIncomingFindings = new List<Finding>(),
                Description = "3 removed advisories should produce 3 current findings and no incoming findings"
            };

            FindingsChangedNotifierTestCase test2 = new FindingsChangedNotifierTestCase
            {
                CurrentAdvisories = new List<ReportsAdvisoryMessage> { evalError1, evalError2, evalWarn1 },
                IncomingAdvisories = new List<ReportsAdvisoryMessage>{ evalError1, evalError2, evalWarn1 },
                ExpectedCurrentFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingEvalWarn1 },
                ExpectedIncomingFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingEvalWarn1 },
                Description = "3 current and 3 incoming advisories should produce 3 current findings and 3 incoming findings"
            };

            FindingsChangedNotifierTestCase test3 = new FindingsChangedNotifierTestCase
            {
                CurrentAdvisories = new List<ReportsAdvisoryMessage>(),
                IncomingAdvisories = new List<ReportsAdvisoryMessage>{ evalError1, evalError2, evalWarn1 },
                ExpectedCurrentFindings = new List<Finding>(),
                ExpectedIncomingFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingEvalWarn1 },
                Description = "3 incoming advisories and no current advisories should produce 3 incoming findings and no current findings"
            };

            FindingsChangedNotifierTestCase test4 = new FindingsChangedNotifierTestCase
            {
                CurrentAdvisories = new List<ReportsAdvisoryMessage> { evalError1, evalError2, evalWarn1 },
                IncomingAdvisories = null,
                ExpectedCurrentFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingEvalWarn1 },
                ExpectedIncomingFindings = new List<Finding>(),
                Description = "incoming advisories being null should produce incoming findings as empty"
            };

            FindingsChangedNotifierTestCase test5 = new FindingsChangedNotifierTestCase
            {
                CurrentAdvisories = null,
                IncomingAdvisories = new List<ReportsAdvisoryMessage>{ evalError1, evalError2, evalWarn1 },
                ExpectedCurrentFindings = new List<Finding>(),
                ExpectedIncomingFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingEvalWarn1 },
                Description = "current advisories being null should produce current findings as empty"
            };


            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
        }

        public class FindingsChangedNotifierTestCase
        {
            public List<ReportsAdvisoryMessage> CurrentAdvisories { get; set; }
            public List<ReportsAdvisoryMessage> IncomingAdvisories { get; set; }
            public List<Finding> ExpectedCurrentFindings { get; set; }
            public List<Finding> ExpectedIncomingFindings { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}
