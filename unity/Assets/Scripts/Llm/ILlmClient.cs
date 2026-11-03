
using System.Threading;
using System.Threading.Tasks;

namespace MuhurluKapi.Llm
{
    public interface ILlmClient
    {
        Task<string> GenerateResponseAsync(
            string prompt,
            CancellationToken cancellationToken = default
        );
    }
}
  