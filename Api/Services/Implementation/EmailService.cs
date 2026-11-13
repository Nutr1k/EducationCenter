using EducationCenter.Models;
using EducationCenter.Services.Abstract;

namespace EducationCenter.Services.Implementation;


public class EmailService : IEmailService
{
	public Task SendAsync(Lead lead, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		throw new InvalidOperationException(
			"Основной почтовый сервис временно недоступен");
	}

	public Task SendFallbackAsync(string email, Lead lead, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		Console.WriteLine($"Заявка отправлена на {email}");

		return Task.CompletedTask;
	}
}

