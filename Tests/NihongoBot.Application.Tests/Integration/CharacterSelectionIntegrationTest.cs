using NihongoBot.Domain;

namespace NihongoBot.Application.Tests.Integration;

public class CharacterSelectionIntegrationTest
{
	[Fact]
	public void CharacterSelection_EndToEndScenario_ShouldWorkCorrectly()
	{
		// This test demonstrates the complete character selection workflow
		
		// Arrange - Simulate a new user
		long telegramId = 123456789L;
		string username = "testuser";
		
		// Act 1: User starts with all characters enabled (default behavior)
		User user = new User(telegramId, username);
		
		// Assert 1: All characters should be enabled by default
		Assert.True(user.KaEnabled);
		Assert.True(user.KiEnabled);
		Assert.True(user.KuEnabled);
		Assert.True(user.KeEnabled);
		Assert.True(user.KoEnabled);
		
		List<string> allCharacters = user.GetEnabledCharacters();
		Assert.Equal(5, allCharacters.Count);
		
		// Act 2: User disables some characters (simulating settings UI interaction)
		user.UpdateCharacterSelection("ka", false);  // Disable Ka
		user.UpdateCharacterSelection("ki", false);  // Disable Ki
		
		// Assert 2: Only selected characters should be disabled
		Assert.False(user.KaEnabled);
		Assert.False(user.KiEnabled);
		Assert.True(user.KuEnabled);
		Assert.True(user.KeEnabled);
		Assert.True(user.KoEnabled);
		
		List<string> enabledCharacters = user.GetEnabledCharacters();
		Assert.Equal(3, enabledCharacters.Count);
		Assert.Contains("ku", enabledCharacters);
		Assert.Contains("ke", enabledCharacters);
		Assert.Contains("ko", enabledCharacters);
		Assert.DoesNotContain("ka", enabledCharacters);
		Assert.DoesNotContain("ki", enabledCharacters);
		
		// Act 3: User re-enables one character
		user.UpdateCharacterSelection("ka", true);   // Re-enable Ka
		
		// Assert 3: Character should be re-enabled
		Assert.True(user.KaEnabled);
		Assert.False(user.KiEnabled);  // Still disabled
		
		List<string> finalCharacters = user.GetEnabledCharacters();
		Assert.Equal(4, finalCharacters.Count);
		Assert.Contains("ka", finalCharacters);
		Assert.Contains("ku", finalCharacters);
		Assert.Contains("ke", finalCharacters);
		Assert.Contains("ko", finalCharacters);
		Assert.DoesNotContain("ki", finalCharacters);
		
		// Demonstrate the impact on practice questions:
		// When HiraganaService.SendHiraganaMessage() is called,
		// it will call:
		// 1. _userRepository.GetByTelegramIdAsync(telegramId) -> returns this user
		// 2. user.GetEnabledCharacters() -> returns ["ka", "ku", "ke", "ko"]
		// 3. _kanaRepository.GetRandomAsync(KanaType.Hiragana, enabledCharacters) -> filters to only enabled characters
		// 4. Only Ka, Ku, Ke, Ko characters will appear in practice questions
		// 5. Ki will be completely excluded from practice until re-enabled
	}
	
	[Fact]
	public void CharacterSelection_AllDisabled_ShouldReturnEmptyList()
	{
		// Test edge case: What happens when user disables all characters?
		
		// Arrange
		User user = new User(123456789L, "testuser");
		
		// Act - Disable all characters
		user.UpdateCharacterSelection("ka", false);
		user.UpdateCharacterSelection("ki", false);
		user.UpdateCharacterSelection("ku", false);
		user.UpdateCharacterSelection("ke", false);
		user.UpdateCharacterSelection("ko", false);
		
		// Assert
		List<string> enabledCharacters = user.GetEnabledCharacters();
		Assert.Empty(enabledCharacters);
		
		// In the real application, this would result in:
		// - HiraganaService would call GetRandomAsync with empty list
		// - Repository would return null (no characters available)
		// - Service would log warning: "No enabled characters for user"
		// - No practice questions would be generated
		// This is the expected behavior to prevent errors
	}
}