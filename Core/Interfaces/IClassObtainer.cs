using System;

namespace Core
{
    public interface IClassObtainer
    {
        Type? GetTypeFromAssembly(string assemblyName, string typeName);
    }
}