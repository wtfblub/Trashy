using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Trashy.Twitch
{
    public static class TwitchAuth
    {
        public const string ClientId = "sm62ysmchtrnphzj4bit3kp1vhthwr";
        private const string BaseUrl = "https://id.twitch.tv/oauth2";
        private const string RedirectUrl = "http://localhost:29938/";
        private const string Scopes = "channel:read:redemptions";

        // ReSharper disable once InconsistentNaming
        private static readonly string HttpJsResponse =
            $"<head><body>Loading...</body><script>window.location.href = '{RedirectUrl}' + '?' + window.location.hash.substr(1)</script></head>";
        private const string HttpErrorResponse = "<head><body>ERROR - {REPLACE}</body></head>";
        private const string HttpResponse = "<head><body>You can close this page now</body></head>";

        private static readonly HttpClient s_client;
        private static readonly HttpListener s_listener;

        public static bool IsValidating { get; private set; }
        public static bool IsWaitingForLogin { get; private set; }

        static TwitchAuth()
        {
            s_client = new HttpClient();
            s_listener = new HttpListener();
            s_listener.Prefixes.Add(RedirectUrl);
        }

        public static async Task<TwitchToken> Validate()
        {
            try
            {
                IsValidating = true;
                if (string.IsNullOrWhiteSpace(ConfigManager.TwitchToken.Value))
                    return null;

                s_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    ConfigManager.TwitchToken.Value
                );
                using (var response = await s_client.GetAsync($"{BaseUrl}/validate").AnyContext())
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return null;

                        case HttpStatusCode.OK:
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync().AnyContext());
                            return new TwitchToken(
                                ConfigManager.TwitchToken.Value,
                                json["login"].Value<string>(),
                                json["user_id"].Value<long>()
                            );

                        default:
                            return null;
                    }
                }
            }
            finally
            {
                IsValidating = false;
            }
        }

        public static async Task<bool> Login()
        {
            try
            {
                IsWaitingForLogin = true;
                s_listener.Stop();
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    var code = await ReceiveCode(cts.Token).AnyContext();
                    if (string.IsNullOrWhiteSpace(code))
                        return false;

                    ConfigManager.TwitchToken.Value = code;
                    return true;
                }
            }
            finally
            {
                IsWaitingForLogin = false;
            }
        }

        public static async Task Revoke()
        {
            s_client.DefaultRequestHeaders.Authorization = null;
            var content = new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["token"] = ConfigManager.TwitchToken.Value
            };
            var response = await s_client.PostAsync($"{BaseUrl}/revoke", new FormUrlEncodedContent(content)).AnyContext();
            response.Dispose();
            ConfigManager.TwitchToken.Value = null;
        }

        private static async Task<string> ReceiveCode(CancellationToken cancellationToken)
        {
            s_listener.Start();
            Application.OpenURL(GetLoginUrl());

            try
            {
                cancellationToken.Register(() => s_listener.Stop());
                return await WaitForRequest().AnyContext();
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
            finally
            {
                s_listener.Stop();
            }
        }

        private static async Task<string> WaitForRequest()
        {
            var context = await s_listener.GetContextAsync().AnyContext();
            var error = context.Request.QueryString.Get("error");
            var errorDescription = context.Request.QueryString.Get("error_description");
            var code = context.Request.QueryString.Get("access_token");

            if (!string.IsNullOrWhiteSpace(error))
            {
                await WriteResponse(HttpErrorResponse.Replace("{REPLACE}", $"{error}: {errorDescription}")).AnyContext();
                return null;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                await WriteResponse(HttpJsResponse).AnyContext();
                return await WaitForRequest().AnyContext();
            }

            await WriteResponse(HttpResponse).AnyContext();
            return code;

            async Task WriteResponse(string content)
            {
                var response = context.Response;
                response.ContentLength64 = content.Length;
                using (response.OutputStream)
                {
                    var bytes = Encoding.UTF8.GetBytes(content);
                    await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).AnyContext();
                }
            }
        }

        public static string GetLoginUrl()
        {
            return $"{BaseUrl}/authorize" +
                   $"?client_id={ClientId}" +
                   $"&redirect_uri={HttpUtility.UrlEncode(RedirectUrl)}" +
                   "&response_type=token" +
                   $"&scope={HttpUtility.UrlEncode(Scopes)}";
        }

        public static void CancelLogin()
        {
            s_listener.Stop();
        }
    }
}
