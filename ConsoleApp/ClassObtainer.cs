using Core;
using System;
using System.Reflection;

namespace ConsoleApp
{
    internal class ClassObtainer : IClassObtainer
    {
        public Type GetTypeFromAssembly(
            string assemblyName,
            string typeName
        )
        {
            var assembly = Assembly.LoadFrom(assemblyName);
            return assembly.GetType(typeName);
        }
    }
}