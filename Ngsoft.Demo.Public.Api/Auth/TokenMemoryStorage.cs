namespace Ngsoft.Demo.Public.Api.Auth
{
    public class TokenMemoryStorage : ITokenStorage
    {
        private string _token = null;

        public void Delete()
        {
            _token = null;
        }

        public string Get() => _token;

        public void Save(string token)
        {
            _token = token;
        }
    }
}
