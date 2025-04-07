using System.Text;

using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class LeaderBoardCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly IUserRepository _userRepository;

	public LeaderBoardCommandHandler(ITelegramBotClient botClient, IUserRepository userRepository)
	{
		_botClient = botClient;
		_userRepository = userRepository;
	}

	public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user == null)
		{
			await _botClient.SendMessage(chatId, "You are not registered.", cancellationToken: cancellationToken);
			return;
		}

		List<Domain.User> topUsers = (await _userRepository.GetTop10UsersByHighestStreakAsync(cancellationToken)).ToList();
		StringBuilder leaderboard = new("Leaderboard:\n");
		bool isUserInTop10 = false;

		for (int i = 0; i < topUsers.Count; i++)
		{
			Domain.User topUser = topUsers[i];
			string suffix = topUser.Id == user.Id ? " (You)" : string.Empty;
			leaderboard.AppendLine($"{i + 1}. {topUser.Username} - {topUser.Streak}{suffix}");

			if (topUser.Id == user.Id)
			{
				isUserInTop10 = true;
			}
		}

		if (!isUserInTop10)
		{
			int userRank = await _userRepository.GetUserStreakRankAsync(user.Id, cancellationToken);
			leaderboard.AppendLine($"\nYour position: {userRank}. {user.Username} - {user.Streak}");
		}

		await _botClient.SendMessage(chatId, leaderboard.ToString(), cancellationToken: cancellationToken);
	}
}
