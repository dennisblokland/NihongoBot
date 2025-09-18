using NihongoBot.Domain;

namespace NihongoBot.Application.Tests.UserDomain;

public class UserCharacterSelectionTest
{
	[Fact]
	public void User_ShouldHaveAllCharactersEnabledByDefault()
	{
		// Arrange & Act
		User user = new User(123456789L, "testuser");

		// Assert
		Assert.True(user.KaEnabled);
		Assert.True(user.KiEnabled);
		Assert.True(user.KuEnabled);
		Assert.True(user.KeEnabled);
		Assert.True(user.KoEnabled);
	}

	[Fact]
	public void GetEnabledCharacters_ShouldReturnAllCharactersByDefault()
	{
		// Arrange
		User user = new User(123456789L, "testuser");

		// Act
		List<string> enabledCharacters = user.GetEnabledCharacters();

		// Assert
		Assert.Equal(5, enabledCharacters.Count);
		Assert.Contains("ka", enabledCharacters);
		Assert.Contains("ki", enabledCharacters);
		Assert.Contains("ku", enabledCharacters);
		Assert.Contains("ke", enabledCharacters);
		Assert.Contains("ko", enabledCharacters);
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
		Assert.False(user.KaEnabled);
		Assert.False(user.KiEnabled);
		Assert.True(user.KuEnabled);
		Assert.True(user.KeEnabled);
		Assert.True(user.KoEnabled);
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
		Assert.Equal(3, enabledCharacters.Count);
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
			Assert.False(user.KaEnabled);
		else if (character.ToLower() == "ki")
			Assert.False(user.KiEnabled);
		else if (character.ToLower() == "ku")
			Assert.False(user.KuEnabled);
	}
}