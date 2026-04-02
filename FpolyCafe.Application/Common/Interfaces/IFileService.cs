using System.IO;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string subFolder);
}
