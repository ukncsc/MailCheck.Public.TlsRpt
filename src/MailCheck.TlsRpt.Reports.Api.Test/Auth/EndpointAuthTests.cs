using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.TestSupport;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Auth
{
    [TestFixture(Category = "Auth")]
    public class EndpointAuthTests : BaseIntegrationTest<StartUp, Api.Controllers.ReportsSummaryController>
    {
        private IMailCheckAuthorisationService _mailCheckAuthorisationService;
        private HttpClient _client;

        public EndpointAuthTests()
        {
            Environment.SetEnvironmentVariable("ConnectionString", "value");
            Environment.SetEnvironmentVariable("SnsTopicArn", "value");
            Environment.SetEnvironmentVariable("MicroserviceOutputSnsTopicArn", "value");
            Environment.SetEnvironmentVariable("AuthorisationServiceEndpoint", "value");
            Environment.SetEnvironmentVariable("DevMode", "value");
        }

        [SetUp]
        public void SetUp()
        {
            _mailCheckAuthorisationService = A.Fake<IMailCheckAuthorisationService>();

            _client = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Replace(new ServiceDescriptor(typeof(IMailCheckAuthorisationService), _mailCheckAuthorisationService));
                });
            }).CreateClient();
        }

        [Test]
        public async Task AuthenticationRequiredForAllEndpoints()
        {
            Assert.NotZero(Endpoints.Count, "Endpoints are discovered");

            foreach ((string, string) endpoint in Endpoints)
            {
                HttpMethod httpMethod = new HttpMethod(endpoint.Item1);
                string url = endpoint.Item2.Replace("{domain}", "digital.ncsc.gov.uk");
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);

                HttpResponseMessage response = await _client.SendAsync(request);

                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.AreEqual(0, Fake.GetCalls(_mailCheckAuthorisationService).Count(), "The request should not try to authorise");
            }
        }

        [Test]
        public async Task AuthorisationRequiredForAllEndpoints()
        {
            _client.DefaultRequestHeaders.Add("oidc_claim_email", "value");
            _client.DefaultRequestHeaders.Add("oidc_claim_given_name", "value");
            _client.DefaultRequestHeaders.Add("oidc_claim_family_name", "value");

            Assert.NotZero(Endpoints.Count, "Endpoints are discovered");

            foreach ((string, string) endpoint in Endpoints)
            {
                HttpMethod httpMethod = new HttpMethod(endpoint.Item1);
                string url = endpoint.Item2.Replace("{domain}", "digital.ncsc.gov.uk");
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);

                HttpResponseMessage response = await _client.SendAsync(request);

                Assert.AreEqual(1, Fake.GetCalls(_mailCheckAuthorisationService).Count(), "The request should authorise once");
                Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "The request should be forbidden");
                Fake.ClearRecordedCalls(_mailCheckAuthorisationService);
            }
        }
    }
}
