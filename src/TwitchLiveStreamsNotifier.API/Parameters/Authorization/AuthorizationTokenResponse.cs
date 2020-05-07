// ReSharper disable InconsistentNaming
namespace TwitchLiveStreamsNotifier.API.Parameters.Authorization
{
    public class AuthorizationTokenResponse
    {
        /// <summary>
        /// OAuth/Bearer toeken
        /// </summary>
        public string Access_token { get; set; }
        
        /// <summary>
        /// In seconds
        /// </summary>
        public string Expires_in { get; set; }
        
        /// <summary>
        /// oauth or bearer
        /// should be bearer for server to server requests
        /// </summary>
        public string Token_type { get; set; }
    }
}