using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLiveStreamsNotifier.API.Parameters.Authorization;
using TwitchLiveStreamsNotifier.API.Parameters.Games;
using TwitchLiveStreamsNotifier.API.Parameters.Initialization;
using TwitchLiveStreamsNotifier.API.Parameters.Streams;
using TwitchLiveStreamsNotifier.API.Parameters.Users;

namespace TwitchLiveStreamsNotifier.API.Helix
{
    public class TwitchHelixClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private const string ApiUrl = "https://api.twitch.tv";
        private const string StreamsPath = "helix/streams";
        private const string GamesPath = "helix/games";
        private const string UsersPath = "helix/users";

        private const string TokenValidationUrl = "https://id.twitch.tv/oauth2/";
        private const string ValidatePath = "validate";
        private const string ObtainTokenPath = "token";

        private readonly string _clientId;
        private readonly string _clientSecret;

        public TwitchHelixClient(HelixInitData initData, ILogger<TwitchHelixClient> logger)
        {
            _logger = logger;

            CheckInitData(initData);
            _clientId = initData.ClientId;
            _clientSecret = initData.ClientSecret;

            _httpClient = GetHttpClient();
        }

        private bool CheckInitData(HelixInitData data)
        {
            if (string.IsNullOrWhiteSpace(data.ClientId))
                throw new ArgumentException($"{nameof(data.ClientId)} is missing");

            if (string.IsNullOrWhiteSpace(data.ClientSecret))
                throw new ArgumentException($"{nameof(data.ClientId)} is missing");

            return true;
        }

        #region Users

        public async Task<TwitchUsersResponse> GetUsersDataByLogin(List<string> logins)
        {
            if (logins == null)
                throw new ArgumentNullException($"{nameof(logins)}");
            if (logins.Count == 0)
            {
                _logger.LogInformation($"{nameof(logins)} count is 0");
                return null;
            }

            var parameters = MakeParameters("login", logins);
            return await GetUsersData(parameters);
        }

        public async Task<TwitchUsersResponse> GetUsersDataById(List<string> userIds)
        {
            if (userIds == null)
                throw new ArgumentNullException($"{nameof(userIds)}");
            if (userIds.Count == 0)
            {
                _logger.LogWarning($"{nameof(userIds)} count is 0");
                return null;
            }

            throw new NotImplementedException();
        }

        private async Task<TwitchUsersResponse> GetUsersData(string requestParameters)
        {
            var fullUrl = GetFullUrl(ApiUrl, UsersPath, requestParameters);

            var data = await GetRequestData<TwitchUsersResponse>(fullUrl);
            return data;
        }

        #endregion

        #region Streams

        public async Task<TwitchStreamsResponse> GetStreamsDataByLogin(List<string> logins)
        {
            if (logins == null)
                throw new ArgumentNullException($"{nameof(logins)}");
            if (logins.Count == 0)
            {
                _logger.LogWarning($"{nameof(GetStreamsDataByLogin)}; {nameof(logins)} count is 0");
                return null;
            }

            var parameters = MakeParameters("user_login", logins);
            return await GetStreamsData(parameters);
        }

        private async Task<TwitchStreamsResponse> GetStreamsData(string requestParameters)
        {
            var fullUrl = GetFullUrl(ApiUrl, StreamsPath, requestParameters);

            _logger.LogTrace($"GetStreamsURL: {fullUrl}");

            var data = await GetRequestData<TwitchStreamsResponse>(fullUrl);
            return data;
        }

        #endregion

        #region Games

        public async Task<TwitchGamesResponse> GetGamesDataById(List<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException($"{nameof(ids)}");
            if (ids.Count == 0)
            {
                _logger.LogInformation($"{nameof(ids)} count is 0");
                return null;
            }

            var parameters = MakeParameters("id", ids);
            return await GetGamesData(parameters);
        }

        private async Task<TwitchGamesResponse> GetGamesData(string requestParameters)
        {
            var fullUrl = GetFullUrl(ApiUrl, GamesPath, requestParameters);

            var data = await GetRequestData<TwitchGamesResponse>(fullUrl);
            return data;
        }

        #endregion


        private string MakeParameters(string name, List<string> values)
        {
            return string.Join("&", values.Select(x => $"{name}={x}"));
        }

        private string GetFullUrl(string domain, string path, string parameters)
        {
            return $"{domain}/{path}?{parameters}";
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Client-ID", _clientId);
            return client;
        }

        private async Task<string> MakeApiRequest(string url)
        {
            _logger.LogTrace($"ApiRequest URL: {url}");

            var client = _httpClient;

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            _logger.LogTrace($"Received Content: {content}");

            return content;
        }

        private async Task<T> GetRequestData<T>(string url) where T : class
        {
            T data = null;
            try
            {
                await ValidateAuthorizationToken();

                var content = await MakeApiRequest(url);

                data = JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return data;
        }

        /// <summary>
        /// Validate OAuth token
        /// </summary>
        private async Task ValidateAuthorizationToken()
        {
            var currentToken = _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            if (string.IsNullOrWhiteSpace(currentToken))
            {
                throw new InvalidOperationException("Unable to get OAuth token from http header. You must authorize before making requests");
            }

            var fullUrl = $"{TokenValidationUrl}{ValidatePath}";

            using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", currentToken);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new ValidationException($"OAuth token validation failed for {fullUrl} with StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}.");
            }

            _logger.LogDebug("OAuth token was successfully validated");
        }

        /// <summary>
        /// Get Bearer token and add it to the Authorization http header
        /// </summary>
        /// <returns>Token</returns>
        public async Task Authorize()
        {
            var currentToken = _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            if (!string.IsNullOrWhiteSpace(currentToken))
            {
                return;
            }

            var url = $"{TokenValidationUrl}{ObtainTokenPath}";
            var fullUrl = $"{url}?client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";

            _logger.LogDebug($"Obtaining OAuth token. url: {url}");
            var response = await _httpClient.PostAsync(fullUrl, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<AuthorizationTokenResponse>(content);

            if (data == null || string.IsNullOrWhiteSpace(data.Access_token))
            {
                throw new Exception($"Unable to obtain OAuth token for ClientId {_clientId}. Url: {url}.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.Access_token);
        }
    }
}