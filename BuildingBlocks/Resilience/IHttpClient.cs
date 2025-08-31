using System;
using System.Text;
using System.Collections.Generic;
namespace Resilience;

public interface IHttpClient
{
    Task<HttpResponseMessage> PostAsync<T>(string url,
                                            T item,
                                            string authorizationToken = null,
                                            string requestid = null,
                                            string authorizationMethod = "Bearer");

    Task<string> GetStringAsync(string url,
                                            string authorizationToken = null,
                                            string authorizationMethod = "Bearer");
    Task<HttpResponseMessage> PostAsync(string url,
                                            Dictionary<string, string> form,
                                            string authorizationToken = null,
                                            string requestid = null,
                                            string authorizationMethod = "Bearer");

    Task<HttpResponseMessage> PutAsync<T>(string url,
                                                T item,
                                                string authorizationToken = null,
                                                string requestid = null, string authorizationMethod = "Bearer");
}
