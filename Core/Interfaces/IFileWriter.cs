using System.Threading.Tasks;

namespace Core
{
    public interface IFileWriter
    {
        Task WriteFile(string location, string content);
    }
}