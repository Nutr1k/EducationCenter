using EducationCenter.DTO;
using EducationCenter.Enums;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenter.Services.Implementation;
using EducationCenterTests.Fakes;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EducationCenterTests.Services;

public class LeadServiceTests
{
	private readonly Mock<IEmailService> _emailServiceMock;
	private readonly LeadService _leadService;

	public LeadServiceTests()
	{
		_emailServiceMock = new Mock<IEmailService>();

		var emailSettings = Options.Create(new EmailSettings
		{
			CorporateEmail = "specialist.corp@yandex.ru",
			IndividualEmail = "specialist.ufl@yandex.ru"
		});

		_leadService = new LeadService(
			_emailServiceMock.Object,
			emailSettings);
	}

	[Fact]
	public async Task CreateAsync_EmailFailsForLegalEntity_SendsFallbackEmail()
	{
		var request = new CreateLeadRequest
		{
			Name = "Иван Иванов",
			Phone = "+79991234567",
			Email = "ivan@mail.com",
			ClientType = ClientType.LegalEntity
		};

		_emailServiceMock
			.Setup(x => x.SendAsync(It.IsAny<Lead>(), 
									CancellationToken.None))
			.ThrowsAsync(new InvalidOperationException());

		_emailServiceMock
			.Setup(x => x.SendFallbackAsync(
				It.IsAny<string>(),
				It.IsAny<Lead>(),
				CancellationToken.None))
			.Returns(Task.CompletedTask);
		await _leadService.CreateAsync(request, CancellationToken.None);

		_emailServiceMock.Verify(x => x.SendFallbackAsync(
			"specialist.corp@yandex.ru",
			It.IsAny<Lead>(),
			CancellationToken.None), Times.Once);
	}

	[Fact]
	public async Task CreateAsync_EmailFailsForIndividual_SendsFallbackEmail()
	{
		var request = new CreateLeadRequest
		{
			Name = "Иван Иванов",
			Phone = "+79991234567",
			Email = "ivan@mail.com",
			ClientType = ClientType.Individual
		};

		_emailServiceMock
			.Setup(x => x.SendAsync(It.IsAny<Lead>(), CancellationToken.None))
			.ThrowsAsync(new InvalidOperationException());

		_emailServiceMock
			.Setup(x => x.SendFallbackAsync(
				It.IsAny<string>(),
				It.IsAny<Lead>(),
				CancellationToken.None))
			.Returns(Task.CompletedTask);

		await _leadService.CreateAsync(request, CancellationToken.None);


		_emailServiceMock.Verify(x => x.SendFallbackAsync(
			"specialist.ufl@yandex.ru",
			It.IsAny<Lead>(), 
			CancellationToken.None), Times.Once);
	}
	[Fact]
	public async Task CreateAsync_FallbackFails_ThrowsException()
	{
		var emailService = new FakeEmailService
		{
			ShouldFail = true,
			ShouldFallbackFail = true
		};

		var settings = Options.Create(new EmailSettings
		{
			CorporateEmail = "specialist.corp@yandex.ru",
			IndividualEmail = "specialist.ufl@yandex.ru"
		});

		var leadService = new LeadService(emailService, settings);

		var request = new CreateLeadRequest
		{
			Name = "Иван Иванов",
			Phone = "+79991234567",
			Email = "ivan@mail.com",
			ClientType = ClientType.LegalEntity
		};

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => leadService.CreateAsync(request, CancellationToken.None));
	}
}