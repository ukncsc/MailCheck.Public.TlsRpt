using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifications;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using AdvisoryMessage = MailCheck.Common.Contracts.Advisories.AdvisoryMessage;

namespace MailCheck.TlsRpt.Reports.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class AdvisoryChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private ITlsRptReportsEntityConfig _config;
        private ILogger<AdvisoryChangedNotifier> _log;
        private IEqualityComparer<AdvisoryMessage> _comparer;
        private AdvisoryChangedNotifier _advisoryChangedNotifier;

        private static Guid testId1 = new Guid("123a26ee-e01c-4673-99cb-badbe8545ba6");
        private static Guid testId2 = new Guid("122ebd52-3d93-46e9-95de-659bf89ca2b8");
        private static Guid testId3 = new Guid("a7541767-ed48-4862-bab7-dd9344caa9aa");
        private static Guid testId4 = new Guid("b0e6747f-a6bd-42f8-b06d-e16fe24e76b4");
        private static Guid testId5 = new Guid("8b52ca95-ba29-4f64-bd0b-873d877ad3b5");
        private static Guid testId6 = new Guid("33c14e10-6223-46c0-abe0-82ef34a815be");

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _config = A.Fake<ITlsRptReportsEntityConfig>();
            _log = A.Fake<ILogger<AdvisoryChangedNotifier>>();
            _comparer = new MessageEqualityComparer();

            _advisoryChangedNotifier = new AdvisoryChangedNotifier(
                _messageDispatcher,
                _config,
                _comparer,
                _log);
        }

        [TestCaseSource(nameof(ExerciseEqualityComparersTestPermutations))]
        public Task ExerciseEqualityComparers(AdvisoryChangedNotifierTestCase testCase)
        {
            _advisoryChangedNotifier.Handle("test.gov.uk", testCase.CurrentMessages, testCase.NewMessages);

            if (testCase.ExpectedAdded > 0)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedAdded), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedAdded), A<string>._))
                 .MustNotHaveHappened();
            }

            if (testCase.ExpectedSustained > 0)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedSustained), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedSustained), A<string>._))
                 .MustNotHaveHappened();
            }

            if (testCase.ExpectedRemoved > 0)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedRemoved), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedRemoved), A<string>._))
                 .MustNotHaveHappened();
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<AdvisoryChangedNotifierTestCase> ExerciseEqualityComparersTestPermutations()
        {
            ReportsAdvisoryMessage error1 = new ReportsAdvisoryMessage(testId1, "name", MessageType.error, "test", "test", "test");
            ReportsAdvisoryMessage error2 = new ReportsAdvisoryMessage(testId2, "name", MessageType.error, "test", "test", "test");
            ReportsAdvisoryMessage error3 = new ReportsAdvisoryMessage(testId3, "name", MessageType.warning, "test", "test", "test");
            ReportsAdvisoryMessage error4 = new ReportsAdvisoryMessage(testId4, "name", MessageType.warning, "test", "test", "test");
            ReportsAdvisoryMessage error5 = new ReportsAdvisoryMessage(testId5, "name", MessageType.info, "test", "test", "test");
            ReportsAdvisoryMessage error6 = new ReportsAdvisoryMessage(testId6, "name", MessageType.info, "test", "test", "test");

            AdvisoryChangedNotifierTestCase test1 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage>(),
                NewMessages = new List<ReportsAdvisoryMessage> { error1 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "0 -> 1 advisory should produce 1 advisory added"
            };

            AdvisoryChangedNotifierTestCase test2 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage> { error1 },
                NewMessages = new List<ReportsAdvisoryMessage>(),
                ExpectedAdded = 0,
                ExpectedRemoved = 1,
                ExpectedSustained = 0,
                Description = "1 -> 0 advisory should produce 1 advisory removed"
            };

            AdvisoryChangedNotifierTestCase test3 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage> { error1 },
                NewMessages = new List<ReportsAdvisoryMessage> { error1 },
                ExpectedAdded = 0,
                ExpectedRemoved = 0,
                ExpectedSustained = 1,
                Description = "1 -> 1 advisory should produce 1 advisory sustained"
            };

            AdvisoryChangedNotifierTestCase test4 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage> { error1 },
                NewMessages = new List<ReportsAdvisoryMessage> { error1, error2 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 1,
                Description = "1 -> 2 advisory should produce 1 advisory added, 1 advisory sustained"
            };

            AdvisoryChangedNotifierTestCase test5 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage> { error1, error2, error3 },
                NewMessages = new List<ReportsAdvisoryMessage> { error4, error5, error6 },
                ExpectedAdded = 3,
                ExpectedRemoved = 3,
                ExpectedSustained = 0,
                Description = "3 different -> 3 different advisory should produce 3 advisory added, 3 advisory removed"
            };

            AdvisoryChangedNotifierTestCase test6 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<ReportsAdvisoryMessage> { error1, error2, error3, error4, error5 },
                NewMessages = new List<ReportsAdvisoryMessage> { error4, error5, error6 },
                ExpectedAdded = 1,
                ExpectedRemoved = 3,
                ExpectedSustained = 2,
                Description = "should produce 1 advisory added, 3 advisories removed, 2 advisories sustained"
            };

            AdvisoryChangedNotifierTestCase test7 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = null,
                NewMessages = null,
                ExpectedAdded = 0,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "when both current and new messages are null, should produce 0 advisories"
            };

            AdvisoryChangedNotifierTestCase test8 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = null,
                NewMessages = new List<ReportsAdvisoryMessage> { error1 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "when current messages is null and there is one new message, should produce 1 added advisory"
            };

            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
            yield return test6;
            yield return test7;
            yield return test8;
        }
    }

    public class AdvisoryChangedNotifierTestCase
    {
        public List<ReportsAdvisoryMessage> CurrentMessages { get; set; }
        public List<ReportsAdvisoryMessage> NewMessages { get; set; }
        public int ExpectedAdded { get; set; }
        public int ExpectedRemoved { get; set; }
        public int ExpectedSustained { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}