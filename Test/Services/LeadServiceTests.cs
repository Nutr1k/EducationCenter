using EducationCenter.DTO;
using EducationCenter.Enums;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenter.Services.Implementation;
using EducationCenterApi.BackgroundServices;
using EducationCenterApi.Data.Abstract;
using EducationCenterApi.Data.Implementation;
using EducationCenterApi.Enums;
using EducationCenterTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
			emailSettings,
			new InMemoryLeadRepository());
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
	public async Task CreateAsync_FallbackFails_SavesLeadAsPending()
	{
		var emailService = new FakeEmailService
		{
			ShouldFail = true,
			ShouldFallbackFail = true
		};

		var repository = new InMemoryLeadRepository();

		var settings = Options.Create(new EmailSettings
		{
			CorporateEmail = "specialist.corp@yandex.ru",
			IndividualEmail = "specialist.ufl@yandex.ru"
		});

		var leadService = new LeadService(emailService, settings, repository);

		var request = new CreateLeadRequest
		{
			Name = "Иван Иванов",
			Phone = "+79991234567",
			Email = "ivan@mail.com",
			ClientType = ClientType.LegalEntity
		};

		await leadService.CreateAsync(request, CancellationToken.None);

		var pendingLeads = await repository.GetPendingAsync(CancellationToken.None);

		Assert.Single(pendingLeads);
		Assert.Equal("Иван Иванов", pendingLeads.First().Name);
		Assert.Equal(LeadStatus.Pending, pendingLeads.First().Status);
	}

	[Fact]
	public async Task CreateAsync_WhenPrimaryEmailFails_ShouldSendFallback()
	{
		var emailService = new FakeEmailService
		{
			ShouldFail = true,
			ShouldFallbackFail = false
		};

		var repository = new InMemoryLeadRepository();

		var emailSettings = Options.Create(new EmailSettings
		{
			CorporateEmail = "specialist.corp@yandex.ru",
			IndividualEmail = "specialist.ufl@yandex.ru"
		});

		var service = new LeadService(
			emailService,
			emailSettings,
			repository);

		var request = new CreateLeadRequest
		{
			Name = "Иван",
			Phone = "+79990000000",
			Email = "ivan@example.com",
			ClientType = ClientType.Individual
		};

		await service.CreateAsync(request, CancellationToken.None);

		Assert.Equal("specialist.ufl@yandex.ru", emailService.FallbackEmail);

		var pendingLeads = await repository.GetPendingAsync(CancellationToken.None);

		Assert.Empty(pendingLeads);
	}

	[Fact]
	public async Task ProcessPendingLeadsAsync_WhenFallbackSucceeds_MarksLeadAsProcessed()
	{
		_emailServiceMock
			.Setup(x => x.SendFallbackAsync(
				It.IsAny<string>(),
				It.IsAny<Lead>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var repository = new InMemoryLeadRepository();

		await repository.SaveAsync(new Lead
		{
			Name = "Иван Иванов",
			Phone = "+79991234567",
			Email = "ivan@mail.com",
			ClientType = ClientType.LegalEntity,
			Status = LeadStatus.Pending
		}, CancellationToken.None);

		var services = new ServiceCollection();
		services.AddSingleton<ILeadRepository>(repository);
		services.AddSingleton<IEmailService>(_emailServiceMock.Object);
		services.Configure<EmailSettings>(options =>
		{
			options.CorporateEmail = "specialist.corp@yandex.ru";
			options.IndividualEmail = "specialist.ufl@yandex.ru";
		});

		using var provider = services.BuildServiceProvider();

		var worker = new PendingLeadBackgroundService(
			provider.GetRequiredService<IServiceScopeFactory>(),
			NullLogger<PendingLeadBackgroundService>.Instance);

		await worker.ProcessPendingLeadsAsync(CancellationToken.None);

		Assert.Empty(await repository.GetPendingAsync(CancellationToken.None));

		_emailServiceMock.Verify(
			x => x.SendFallbackAsync(
				"specialist.corp@yandex.ru",
				It.IsAny<Lead>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}
}