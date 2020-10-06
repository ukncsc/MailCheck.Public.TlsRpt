using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.TlsRpt.Api.Domain;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Api.Dao
{
    public interface ITlsRptApiDao
    {
        Task<List<TlsRptInfoResponse>> GetTlsRptForDomains(List<string> domains);
        Task<TlsRptInfoResponse> GetTlsRptForDomain(string domain);
        Task<string> GetTlsRptHistory(string domain);
    }

    public class TlsRptApiDao : ITlsRptApiDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public TlsRptApiDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<TlsRptInfoResponse>> GetTlsRptForDomains(List<string> domain)
        {
            string query = string.Format(TlsRptApiDaoResources.SelectTlsRptStates,
                string.Join(',', domain.Select((_, i) => $"@domain{i}")));

            MySqlParameter[] parameters = domain
                .Select((_, i) => new MySqlParameter($"domain{i}", _))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString, query, parameters))
            {
                List<TlsRptInfoResponse> states = new List<TlsRptInfoResponse>();

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDbNull("state"))
                    {
                        states.Add(JsonConvert.DeserializeObject<TlsRptInfoResponse>(reader.GetString("state"), _serializerSettings));
                    }
                }

                return states;
            }
        }

        public async Task<TlsRptInfoResponse> GetTlsRptForDomain(string domain)
        {
            List<TlsRptInfoResponse> responses = await GetTlsRptForDomains(new List<string>{domain});
            return responses.FirstOrDefault();
        }

        public async Task<string> GetTlsRptHistory(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            return (string) await MySqlHelper.ExecuteScalarAsync(connectionString,
                TlsRptApiDaoResources.SelectTlsRptHistoryStates, new MySqlParameter("domain", domain));
        }
    }
}