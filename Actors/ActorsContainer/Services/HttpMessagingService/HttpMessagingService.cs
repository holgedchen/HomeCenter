﻿using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Utils.Extensions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public class HttpMessagingService : Service
    {
        [Subscibe]
        protected async Task<string> SendGetRequest(HttpQuery httpMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var httpResponse = await httpClient.GetAsync(httpMessage.Address).ConfigureAwait(false);
                httpResponse.EnsureSuccessStatusCode();
                var responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseBody;
            }
        }

        [Subscibe]
        protected async Task<string> SendPostRequest(HttpCommand httpMessage)
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpMessage.Cookies != null)
            {
                httpClientHandler.CookieContainer = httpMessage.Cookies;
                httpClientHandler.UseCookies = true;
            }

            if (httpMessage.Creditionals != null)
            {
                httpClientHandler.Credentials = httpMessage.Creditionals;
            }

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                foreach (var header in httpMessage.DefaultHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                if (!string.IsNullOrWhiteSpace(httpMessage.AuthorisationHeader.Key))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(httpMessage.AuthorisationHeader.Key, httpMessage.AuthorisationHeader.Value);
                }

                var content = new StringContent(httpMessage.Body);
                if (!string.IsNullOrWhiteSpace(httpMessage.ContentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(httpMessage.ContentType);
                }
                var response = await httpClient.PostAsync(httpMessage.Address, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync(Encoding.UTF8).ConfigureAwait(false);

                return responseBody;
            }
        }
    }
}