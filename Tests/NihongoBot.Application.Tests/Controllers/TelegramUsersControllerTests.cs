using Microsoft.AspNetCore.Mvc;
using Moq;
using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Server.Controllers;
using NihongoBot.Shared.Models;

namespace NihongoBot.Application.Tests.Controllers;

public class TelegramUsersControllerTests
{
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly TelegramUsersController _controller;

	public TelegramUsersControllerTests()
	{
		_controller = new TelegramUsersController(_userRepositoryMock.Object);
	}

	[Fact]
	public async Task Update_WithValidTimezone_ShouldUpdateUser()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var user = new User(12345, "testuser");
		var request = new UpdateTelegramUserRequest { TimeZone = "America/New_York" };

		_userRepositoryMock.Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);
		_userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _controller.Update(userId, request);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var dto = Assert.IsType<TelegramUserDto>(okResult.Value);
		Assert.Equal("America/New_York", dto.TimeZone);
		
		// Verify the user's timezone was updated
		Assert.Equal("America/New_York", user.TimeZone.Id);
		_userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Update_WithInvalidTimezone_ShouldReturnBadRequest()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var user = new User(12345, "testuser");
		var request = new UpdateTelegramUserRequest { TimeZone = "Invalid/Timezone" };

		_userRepositoryMock.Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Act
		var result = await _controller.Update(userId, request);

		// Assert
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal("Invalid timezone identifier", badRequestResult.Value);
		_userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Update_WithNonExistentUser_ShouldReturnNotFound()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var request = new UpdateTelegramUserRequest { TimeZone = "America/New_York" };

		_userRepositoryMock.Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((User?)null);

		// Act
		var result = await _controller.Update(userId, request);

		// Assert
		Assert.IsType<NotFoundResult>(result);
		_userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public void MapToDto_IncludesTimezone()
	{
		// Arrange
		var user = new User(12345, "testuser");
		var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
		user.UpdateTimeZone(easternTimeZone);

		// Act - Using reflection to access the private method for testing
		var mapToDtoMethod = typeof(TelegramUsersController)
			.GetMethod("MapToDto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		var dto = (TelegramUserDto)mapToDtoMethod!.Invoke(null, new object[] { user })!;

		// Assert
		Assert.Equal("America/New_York", dto.TimeZone);
		Assert.Equal(user.TelegramId, dto.TelegramId);
		Assert.Equal(user.Username, dto.Username);
	}
}