using EducationCenter.Models;
using EducationCenter.Services.Abstract;

namespace EducationCenterTests.Fakes;

public class FakeEmailService : IEmailService
{
	public bool ShouldFail { get; set; }
	public bool ShouldFallbackFail { get; set; }

	public string? FallbackEmail { get; private set; }

	public Task SendAsync(Lead lead, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (ShouldFail)
			throw new InvalidOperationException("Ошибка основной отправки");

		return Task.CompletedTask;
	}

	public Task SendFallbackAsync(string email, Lead lead, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (ShouldFallbackFail)
			throw new InvalidOperationException("Ошибка резервной отправки");

		FallbackEmail = email;

		return Task.CompletedTask;
	}
}