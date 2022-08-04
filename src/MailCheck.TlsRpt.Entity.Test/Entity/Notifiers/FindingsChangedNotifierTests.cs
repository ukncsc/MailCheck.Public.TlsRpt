using System;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Entity;
using NUnit.Framework;
using LocalNotifier = MailCheck.TlsRpt.Entity.Entity.Notifiers.FindingsChangedNotifier;
using ErrorMessage = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using MailCheck.TlsRpt.Contracts;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.Common.Contracts.Findings;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class FindingsChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IFindingsChangedNotifier _findingsChangedNotifier;
        private ITlsRptEntityConfig _tlsRptEntityConfig;
        private ILogger<LocalNotifier> _logger;

        private LocalNotifier _notifier;
        private MessageEqualityComparer _equalityComparer;

        private const string Id = "test.gov.uk";

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _findingsChangedNotifier = A.Fake<IFindingsChangedNotifier>();
            _tlsRptEntityConfig = A.Fake<ITlsRptEntityConfig>();
            _logger = A.Fake<ILogger<LocalNotifier>>();
            _notifier = new LocalNotifier(_messageDispatcher, _findingsChangedNotifier, _tlsRptEntityConfig, _logger);
            _equalityComparer = new MessageEqualityComparer();
        }

        [TestCaseSource(nameof(ExerciseFindingsChangedNotifierTestPermutations))]
        public void ExerciseFindingsChangedNotifier(FindingsChangedNotifierTestCase testCase)
        {
            A.CallTo(() => _tlsRptEntityConfig.WebUrl).Returns("testurl.com");

            TlsRptRecords stateTlsRptRecords = CreateTlsRptRecords();
            stateTlsRptRecords.Records[0].Tags[0].Explanation = "Explanation";

            TlsRptEntityState state = new TlsRptEntityState(Id, 2, TlsRptState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                TlsRptRecords = stateTlsRptRecords,
                Messages = testCase.StateMessages
            };

            TlsRptRecords resultTlsRptRecords = CreateTlsRptRecords();
            resultTlsRptRecords.Records[0].Tags[0].Explanation = "Explanation";

            TlsRptRecordsEvaluated tlsRptRecordsEvaluated = new TlsRptRecordsEvaluated(Id, resultTlsRptRecords, testCase.ResultMessages, DateTime.MinValue);

            _notifier.Handle(state, tlsRptRecordsEvaluated);

            A.CallTo(() => _findingsChangedNotifier.Process(
                "test.gov.uk",
                "TLS-RPT",
                A<List<Finding>>.That.Matches(x => x.SequenceEqual(testCase.ExpectedCurrentFindings, _equalityComparer)),
                A<List<Finding>>.That.Matches(x => x.SequenceEqual(testCase.ExpectedIncomingFindings, _equalityComparer))
            )).MustHaveHappenedOnceExactly();
        }

        private static IEnumerable<FindingsChangedNotifierTestCase> ExerciseFindingsChangedNotifierTestPermutations()
        {
            ErrorMessage evalError1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName1", MessageSources.TlsRptEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage evalError2 = new ErrorMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName2", MessageSources.TlsRptEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage pollerWarn1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.tlsrpt.testName3", MessageSources.TlsRptPoller, MessageType.warning, "PollerError", string.Empty);

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

            Finding findingPollerWarn1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.tlsrpt.testName3",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/tls-rpt",
                Severity = "Advisory",
                Title = "PollerError"
            };

            FindingsChangedNotifierTestCase test1 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ResultMessages = new List<ErrorMessage>(),
                ExpectedCurrentFindings = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedIncomingFindings = new List<Finding>(),
                Description = "3 current advisories should produce 3 current findings"
            };

            FindingsChangedNotifierTestCase test2 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ResultMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ExpectedCurrentFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedIncomingFindings = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                Description = "3 current advisories and 3 incoming advisories should produce 3 current findings and 3 incoming findings"
            };

            FindingsChangedNotifierTestCase test3 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage>(),
                ResultMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ExpectedCurrentFindings = new List<Finding>(),
                ExpectedIncomingFindings = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                Description = "3 incoming advisories should produce 3 incoming findings"
            };

            FindingsChangedNotifierTestCase test4 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                ResultMessages = new List<ErrorMessage> { evalError2, pollerWarn1 },
                ExpectedCurrentFindings = new List<Finding> { findingEvalError1},
                ExpectedIncomingFindings = new List<Finding>{ findingEvalError2, findingPollerWarn1 },
                Description = "2 incoming advisories and 1 current should produce 2 incoming and 1 current findings"
            };

            FindingsChangedNotifierTestCase test5 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ResultMessages = null,
                ExpectedCurrentFindings = new List<Finding>{ findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedIncomingFindings = new List<Finding>(),
                Description = "null incoming advisories should produce empty incoming findings"
            };

            FindingsChangedNotifierTestCase test6 = new FindingsChangedNotifierTestCase
            {
                StateMessages = null,
                ResultMessages = new List<ErrorMessage> { evalError1, evalError2, pollerWarn1 },
                ExpectedCurrentFindings = new List<Finding>(),
                ExpectedIncomingFindings = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                Description = "null current advisories should produce empty current findings"
            };

            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
            yield return test6;
        }

        private static TlsRptRecords CreateTlsRptRecords(string domain = "test.gov.uk")
        {
            return new TlsRptRecords(domain, new List<TlsRptRecord>
            {
                new TlsRptRecord(domain, new List<string>(), new List<Tag>{ new VersionTag("v=TLSRPTv1", "TLSRPTv1") })
            }, 100);
        }

        public class FindingsChangedNotifierTestCase
        {
            public List<ErrorMessage> StateMessages { get; set; }
            public List<ErrorMessage> StateRecordsMessages { get; set; }
            public List<ErrorMessage> ResultMessages { get; set; }
            public List<ErrorMessage> ResultRecordsMessages { get; set; }
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
