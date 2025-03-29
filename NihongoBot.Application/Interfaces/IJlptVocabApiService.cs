using NihongoBot.Application.Models;

namespace NihongoBot.Application.Interfaces;

public interface IJlptVocabApiService
{
	Task<JLPTWord?> GetRandomWordAsync(int level = 5);
}
