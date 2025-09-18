using NihongoBot.Domain;

namespace NihongoBot.Application.Tests.UserDomain;

public class UserCharacterSelectionTest
{
	[Fact]
	public void User_ShouldHaveAllCharactersEnabledByDefault()
	{
		// Arrange & Act
		User user = new User(123456789L, "testuser");

		// Assert - Check that all characters are enabled by default
		Assert.True(user.IsCharacterEnabled("ka"));
		Assert.True(user.IsCharacterEnabled("ki"));
		Assert.True(user.IsCharacterEnabled("ku"));
		Assert.True(user.IsCharacterEnabled("ke"));
		Assert.True(user.IsCharacterEnabled("ko"));
		Assert.True(user.IsCharacterEnabled("a"));
		Assert.True(user.IsCharacterEnabled("i"));
		Assert.True(user.IsCharacterEnabled("u"));
		Assert.True(user.IsCharacterEnabled("e"));
		Assert.True(user.IsCharacterEnabled("o"));
	}

	[Fact]
	public void GetEnabledCharacters_ShouldReturnAllCharactersByDefault()
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act
		List<string> enabledCharacters = user.GetEnabledCharacters();

		// Assert - Should return all 67 hiragana characters by default
		Assert.True(enabledCharacters.Count >= 67); // At least 67 characters
		Assert.Contains("ka", enabledCharacters);
		Assert.Contains("ki", enabledCharacters);
		Assert.Contains("ku", enabledCharacters);
		Assert.Contains("ke", enabledCharacters);
		Assert.Contains("ko", enabledCharacters);
		Assert.Contains("a", enabledCharacters);
		Assert.Contains("i", enabledCharacters);
		Assert.Contains("u", enabledCharacters);
		Assert.Contains("e", enabledCharacters);
		Assert.Contains("o", enabledCharacters);
	}

	[Fact]
	public void UpdateCharacterSelection_ShouldUpdateCharacterState()
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act
		user.UpdateCharacterSelection("ka", false);
		user.UpdateCharacterSelection("ki", false);

		// Assert
		Assert.False(user.IsCharacterEnabled("ka"));
		Assert.False(user.IsCharacterEnabled("ki"));
		Assert.True(user.IsCharacterEnabled("ku"));
		Assert.True(user.IsCharacterEnabled("ke"));
		Assert.True(user.IsCharacterEnabled("ko"));
	}

	[Fact]
	public void GetEnabledCharacters_ShouldReturnOnlyEnabledCharacters()
	{
		// Arrange
		User user = new User(123456789L, "testuser");
		user.UpdateCharacterSelection("ka", false);
		user.UpdateCharacterSelection("ki", false);

		// Act
		List<string> enabledCharacters = user.GetEnabledCharacters();

		// Assert
		Assert.True(enabledCharacters.Count < 67); // Should be less than total
		Assert.DoesNotContain("ka", enabledCharacters);
		Assert.DoesNotContain("ki", enabledCharacters);
		Assert.Contains("ku", enabledCharacters);
		Assert.Contains("ke", enabledCharacters);
		Assert.Contains("ko", enabledCharacters);
	}

	[Fact]
	public void UpdateCharacterSelection_ShouldThrowExceptionForInvalidCharacter()
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act & Assert
		Assert.Throws<ArgumentException>(() => user.UpdateCharacterSelection("invalid", false));
	}

	[Theory]
	[InlineData("KA")]
	[InlineData("Ki")]
	[InlineData("kU")]
	public void UpdateCharacterSelection_ShouldBeCaseInsensitive(string character)
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act
		user.UpdateCharacterSelection(character, false);

		// Assert
		if (character.ToLower() == "ka")
			Assert.False(user.IsCharacterEnabled("ka"));
		else if (character.ToLower() == "ki")
			Assert.False(user.IsCharacterEnabled("ki"));
		else if (character.ToLower() == "ku")
			Assert.False(user.IsCharacterEnabled("ku"));
	}

	[Fact]
	public void UpdateCharacterSelection_ShouldWorkWithAllHiraganaCharacters()
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act & Assert - Test a few characters from different groups
		user.UpdateCharacterSelection("a", false);
		Assert.False(user.IsCharacterEnabled("a"));

		user.UpdateCharacterSelection("sha", false);
		Assert.False(user.IsCharacterEnabled("sha"));

		user.UpdateCharacterSelection("kya", false);
		Assert.False(user.IsCharacterEnabled("kya"));

		user.UpdateCharacterSelection("n", false);
		Assert.False(user.IsCharacterEnabled("n"));

		// Re-enable one
		user.UpdateCharacterSelection("a", true);
		Assert.True(user.IsCharacterEnabled("a"));
	}

	[Fact]
	public void GetCharacterDisplayNames_ShouldReturnAllCharacters()
	{
		// Act
		Dictionary<string, string> displayNames = User.GetCharacterDisplayNames();

		// Assert
		Assert.True(displayNames.Count >= 67);
		Assert.Contains("ka", displayNames.Keys);
		Assert.Contains("kya", displayNames.Keys);
		Assert.Contains("n", displayNames.Keys);
		Assert.Equal("Ka (か)", displayNames["ka"]);
	}
}