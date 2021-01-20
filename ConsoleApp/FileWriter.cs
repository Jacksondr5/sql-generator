using Core;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class FileWriter : IFileWriter
    {
        public Task WriteFile(string location, string content) =>
            File.WriteAllTextAsync(location, content);
    }
}