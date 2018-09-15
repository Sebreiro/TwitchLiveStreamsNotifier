Twitch Live Streams Notifier
===
ASP.<span></span>NET Core 2.x console application that gets information about active twitch streams and sends message to the REST endpoint  

### Note ###
Startup project is _TwitchLiveStreamsNotifier.Start_

Message format: `$"{StreamerLogin}; {GameName}; {StartedAt}; {StreamType}; {StreamName}; {ChannelUrl}";`  
`StreamType` is _'live'_ if stream is live and **empty** if it's a premiere (prerecorded) video  

Application was developed to send messages to [TelegramBotMessageSender](https://github.com/Sebreiro/TelegramBotMessageSender)

Config
---
_appsettings.json_  - main config and it's requeired  
_nlog.config_  - logging config and it's required too

### Nlog.config ###
By default it's configured to write Logs to Console with minimal level "Info" and write all logs to the file  
More information about nlog can be found in [NLog wiki](https://github.com/NLog/NLog/wiki)

### appsettings.json ###
All config parameters (except `requestInterval`) can be changed in real time.

File structure:  
~~~
{
  "twitchConfig": {
    "clientId": "xxxxxxxxxxxxxxxx",
    "logins": [
      "SovietWomble",      
      "shroud",
      "zondalol",
      "xbox_alive"
    ]
  },
  "jobConfig": {
    "requestInterval": 5
  },
  "streamCacheConfig": {
    "timeBetweenNewStreams":  9
  },
  "messageSender": {
    "url": "http://<host>:<port>/api/Message/SendMessage"
  }
}
~~~

`twitchConfig.clientId` - twitch clientId.  
More about [Getting a ClientId](https://dev.twitch.tv/docs/v5/#getting-a-client-id)

`twitchConfig.logins` - array of twitch streamers logins. You can get one from streamer page URL. Limit is 100

`jobConfig.requestInterval` - time in **minutes** till the next twitch API request. To change this parameter application has to be restarted.

`streamCacheConfig.timeBetweenNewStreams` - time in **minutes** till the stream cache expiration. When stream goes live, it's get cached. When API request returns stream with the same name and time since last request less then `timeBetweenNewStreams` - it's the same stream, and message won't be send.

`messageSender.url` - url where message will be send

TODO
---
  - Cache twitch Games and Users to reduce number of twitch API requests