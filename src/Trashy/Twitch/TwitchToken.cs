namespace Trashy.Twitch
{
    public class TwitchToken
    {
        public string Token { get; }
        public string Login { get; }
        public long UserId { get; }

        public TwitchToken(string token, string login, long userId)
        {
            Token = token;
            Login = login;
            UserId = userId;
        }
    }
}
