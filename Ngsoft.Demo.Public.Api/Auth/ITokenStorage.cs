namespace Ngsoft.Demo.Public.Api.Auth
{
    public interface ITokenStorage
    {
        string Get();
        void Save(string token);
        void Delete();
    }
}
