using NihongoBot.Domain;

namespace NihongoBot.Domain.Tests;

public class UserTests
{
	[Fact]
	public void UpdateTimeZone_WithValidTimeZone_ShouldUpdateTimeZone()
	{
		// Arrange
		var user = new User(12345, "testuser");
		var newTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
		
		// Act
		user.UpdateTimeZone(newTimeZone);
		
		// Assert
		Assert.Equal(newTimeZone.Id, user.TimeZone.Id);
	}
	
	[Fact]
	public void UpdateTimeZone_WithNullTimeZone_ShouldThrowArgumentNullException()
	{
		// Arrange
		var user = new User(12345, "testuser");
		
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => user.UpdateTimeZone(null!));
	}
	
	[Fact]
	public void User_DefaultTimeZone_ShouldBeUtc()
	{
		// Arrange & Act
		var user = new User(12345, "testuser");
		
		// Assert
		Assert.Equal(TimeZoneInfo.Utc.Id, user.TimeZone.Id);
	}
}