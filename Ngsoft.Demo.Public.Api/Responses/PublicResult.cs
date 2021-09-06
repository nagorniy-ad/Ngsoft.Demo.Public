namespace Ngsoft.Demo.Public.Api.Responses
{
    internal class PublicResult<T>
    {
        public PublicStatus Status { get; set; }
        public T Content { get; set; }
    }
}
