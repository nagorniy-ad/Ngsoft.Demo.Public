using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Ngsoft.Demo.Public.Api.Resolvers
{
    internal class GetResultDataContractResolver : DefaultContractResolver
    {
        private readonly string _targetPropertyName;

        public GetResultDataContractResolver(string targetPropertyName)
        {
            _targetPropertyName = targetPropertyName ?? throw new ArgumentNullException(nameof(targetPropertyName));
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "RequestedData")
            {
                property.PropertyName = _targetPropertyName;
            }
            return property;
        }
    }
}
