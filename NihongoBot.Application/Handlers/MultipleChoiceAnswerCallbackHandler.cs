using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Models;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NihongoBot.Application.Handlers;

public class MultipleChoiceAnswerCallbackHandler : ITelegramCallbackHandler<MultipleChoiceAnswerCallbackData>
{
	private readonly ITelegramBotClient _botClient;
	private readonly IQuestionRepository _questionRepository;
	private readonly IUserRepository _userRepository;
	private readonly IKanaRepository _kanaRepository;
	private readonly HiraganaService _hiraganaService;

	public MultipleChoiceAnswerCallbackHandler(
		ITelegramBotClient botClient,
		IQuestionRepository questionRepository,
		IUserRepository userRepository,
		IKanaRepository kanaRepository,
		HiraganaService hiraganaService)
	{
		_botClient = botClient;
		_questionRepository = questionRepository;
		_userRepository = userRepository;
		_kanaRepository = kanaRepository;
		_hiraganaService = hiraganaService;
	}

	public async Task HandleAsync(long chatId, MultipleChoiceAnswerCallbackData callbackData, CancellationToken cancellationToken)
	{
		Question? question = await _questionRepository.FindByIdAsync(callbackData.QuestionId, cancellationToken);
		if (question == null)
		{
			await _botClient.SendMessage(chatId, "Question not found.", cancellationToken: cancellationToken);
			return;
		}

		if (question.IsAnswered || question.IsExpired)
		{
			await _botClient.SendMessage(chatId, "This question has already been answered or expired.", cancellationToken: cancellationToken);
			return;
		}

		Domain.User? user = await _userRepository.FindByIdAsync(question.UserId, cancellationToken);
		if (user == null)
		{
			await _botClient.SendMessage(chatId, "User not found.", cancellationToken: cancellationToken);
			return;
		}

		bool isCorrect = callbackData.SelectedAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

		if (isCorrect)
		{
			question.IsAnswered = true;
			user.IncreaseStreak();
			await _userRepository.SaveChangesAsync(cancellationToken);

			Kana? kana = await _kanaRepository.GetByCharacterAsync(question.QuestionText, cancellationToken);
			if (kana == null)
			{
				return;
			}

			string message = $"✅ Correct! The Romaji for {question.QuestionText} is {question.CorrectAnswer}.";
			if (kana.Variants != null && kana.Variants.Count > 0)
			{
				message += "\nVariants: \n";
				foreach (KanaVariant variant in kana.Variants)
				{
					message += "   " + variant.Character + " is " + variant.Romaji + "\n";
				}
			}
			message += $"\n\nYour current streak is **{user.Streak}**.";
			await _botClient.SendMessage(chatId,
				message,
				ParseMode.Markdown, cancellationToken: cancellationToken, replyParameters: question.MessageId);

			// Automatically show stroke order animation for correct answers
			await _hiraganaService.SendStrokeOrderAnimation(chatId, question.QuestionText, cancellationToken);
		}
		else
		{
			question.Attempts++;

			if (question.Attempts >= 3)
			{
				question.IsExpired = true;
				user.ResetStreak();
				await _userRepository.SaveChangesAsync(cancellationToken);

				await _botClient.SendMessage(chatId, $"❌ You've reached the maximum number of attempts. The correct answer was **{question.CorrectAnswer}**.", ParseMode.Markdown, cancellationToken: cancellationToken, replyParameters: question.MessageId);
			}
			else
			{
				await _questionRepository.SaveChangesAsync(cancellationToken);
				int attemptsLeft = 3 - question.Attempts;
				await _botClient.SendMessage(chatId, $"❌ Incorrect. You have {attemptsLeft} attempt(s) left.", cancellationToken: cancellationToken, replyParameters: question.MessageId);
			}
		}
	}
}