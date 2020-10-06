using System;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Entity.Test
{
    [TestFixture]
    public class LambdaEntryPointTest
    {
        [Test]
        public void CanCreateLambdaEntryPoint()
        {
            Environment.SetEnvironmentVariable("RemainingTimeThresholdSeconds", "10");
            Environment.SetEnvironmentVariable("SqsQueueUrl", "http://");
            Environment.SetEnvironmentVariable("TimeoutSqsSeconds", "10");
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "50");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "50");
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "50");

            LambdaEntryPoint lambdaEntryPoint = new LambdaEntryPoint();

            Assert.That(lambdaEntryPoint, Is.Not.Null);
        }
    }   
}
