using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLiveStreamsNotifier.Application.Config;
using TwitchLiveStreamsNotifier.Application.Streams.Parameters;
using TwitchLiveStreamsNotifier.API.Helix;
using TwitchLiveStreamsNotifier.API.Parameters.Games;
using TwitchLiveStreamsNotifier.API.Parameters.Initialization;
using TwitchLiveStreamsNotifier.API.Parameters.Streams;
using TwitchLiveStreamsNotifier.API.Parameters.Users;

namespace TwitchLiveStreamsNotifier.Application.Streams
{
    public class TwitchStreamsDataAggregator: ITwitchStreamsDataAggregator
    {
        private const string TwitchUrl = "https://www.twitch.tv/";

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        readonly IOptionsMonitor<TwitchConfig> _config;

        public TwitchStreamsDataAggregator(ILoggerFactory loggerFactory, IOptionsMonitor<TwitchConfig> config)
        {
            _loggerFactory = loggerFactory;
            _config = config;
            _logger = loggerFactory.CreateLogger<TwitchStreamsDataAggregator>();
        }

        private bool CheckConfig(TwitchConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ClientId))
                throw new InvalidOperationException($"TwitchConfig {config.ClientId} is missing");

            if (config.Logins == null)
                throw new InvalidOperationException($"TwitchConfig {config.Logins} is null");

            if (config.Logins.Count == 0)
            {
                _logger.LogWarning("TwitchConfig Logins count is 0");
                return false;
            }

            return true;
        }

        public async Task<List<LiveStreamData>> GetLiveStreamsData()
        {
            var config = _config.CurrentValue;
            if (!CheckConfig(config))
                return null;

            _logger.LogInformation($"TwitchConfig logins count: {config.Logins.Count}");
            if (config.Logins.Count == 0)
                return null;

            var initData = new HelixInitData()
            {
                ClientId = config.ClientId
            };
            var twitchClient = new TwitchHelixClient(initData, _loggerFactory.CreateLogger<TwitchHelixClient>());

            var streamsTask = twitchClient.GetStreamsDataByLogin(config.Logins);
            //TODO Make cache for users and games
            //TODO Request users by id (get ids from streamTaskResult)
            var usersTask = twitchClient.GetUsersDataByLogin(config.Logins);
            Task.WaitAll(streamsTask, usersTask);

            var streamsResult = streamsTask.Result;
            if (streamsResult == null)
            {
                _logger.LogDebug($"{nameof(streamsResult)} is null");
                return null;
            }
            if (streamsResult.Data.Count == 0)
            {
                _logger.LogInformation($"No one streaming right now");
                return null;
            }

            var usersResult = usersTask.Result;
            if (usersResult == null)
            {
                _logger.LogDebug($"{nameof(usersResult)} is null");
                return null;
            }
            if (usersResult.Data.Count == 0)
            {
                _logger.LogWarning($"{nameof(usersResult)} Data count is 0");
                return null;

            }

            var gameIds = streamsResult.Data.Select(x => x.Game_id).ToList();
            
            var gamesResult = await twitchClient.GetGamesDataById(gameIds);
            if (gamesResult == null)
            {
                _logger.LogDebug($"{nameof(gamesResult)} is null");
                return null;
            }

            var liveStreamsData = new List<LiveStreamData>();
            foreach (var streamResult in streamsResult.Data)
            {
                var lsData = MakeStreamData(streamResult, gamesResult, usersResult);
                if (lsData == null) continue;

                liveStreamsData.Add(lsData);
            }

            return liveStreamsData;
        }

        private LiveStreamData MakeStreamData(TwitchStreamData streamData, TwitchGamesResponse gamesResponse, TwitchUsersResponse usersResponse)
        {
            var gameData = gamesResponse.Data.Find(x => streamData.Game_id == x.Id);
            if (gameData == null)
            {
                _logger.LogWarning($"{nameof(gameData)} Is Null; gameId: {streamData.Game_id};UserId: {streamData.User_id}; Streamname {streamData.Title}");
            }
            if (gameData?.Name == null)
                _logger.LogWarning($"Twitch Game with Id {streamData.Game_id} not found");

            var gameName = gameData?.Name ?? gameData?.Id ?? "Unknown game";

            var userData = usersResponse.Data.Find(x => streamData.User_id == x.Id);
            if (userData == null)
            {
                throw new InvalidOperationException($"{nameof(userData)} Is Null; gameId: {streamData.Game_id};UserId: {streamData.User_id} StreamName {streamData.Title}");
            }
            if (userData.Login == null)
            {
                _logger.LogError($"Twitch User with Id {streamData.User_id} not found");
                return null;
            }

            var lsData = new LiveStreamData()
            {
                StreamName = streamData.Title,
                GameName = gameName,
                UserName = userData.Login,
                ChannelUrl = GetChannelUrl(userData.Login),
                StartedAt = streamData.Started_at,
                Type = streamData.Type
            };

            return lsData;
        }

        private string GetChannelUrl(string login)
        {
            return $"{TwitchUrl}{login}";
        }
    }
}
