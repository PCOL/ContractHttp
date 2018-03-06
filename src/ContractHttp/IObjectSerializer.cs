namespace ContractHttp
{
    using System;

    public interface IObjectSerializer
    {
        string ContentType { get; }

        string SerializeObject(object obj);

        object DeserializeObject(string data, Type type);
    }
}