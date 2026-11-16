using EducationCenter.Enums;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenterApi.Data.Abstract;
using Microsoft.Extensions.Options;

namespace EducationCenterApi.BackgroundServices
{
	public class PendingLeadBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<PendingLeadBackgroundService> _logger;

		private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

		public PendingLeadBackgroundService(IServiceScopeFactory scopeFactory, ILogger<PendingLeadBackgroundService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessPendingLeadsAsync(stoppingToken);
				}
				catch (OperationCanceledException) 
				{
					break;
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "Ошибка при обработке ожидающих заявок.");
				}

				await Task.Delay(_interval, stoppingToken);
			}
		}

		internal async Task ProcessPendingLeadsAsync(CancellationToken cancellationToken)
		{
			using var scope = _scopeFactory.CreateScope();

			var repository = scope.ServiceProvider.GetRequiredService<ILeadRepository>();

			var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

			var emailSettings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

			var pendingLeads = await repository.GetPendingAsync(cancellationToken);

			foreach (var lead in pendingLeads)
			{
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					var email = GetFallbackEmail(lead, emailSettings);

					await emailService.SendFallbackAsync(email, lead, cancellationToken);

					await repository.MarkProcessedAsync(lead.Id, cancellationToken);

					_logger.LogInformation("\u001b[32mЗаявка {LeadId} успешно обработана фоновым воркером.\u001b[0m", lead.Id);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception exception)
				{
					_logger.LogError("Не удалось повторно отправить заявку {LeadId}.", lead.Id);
				}
			}
		}

		private static string GetFallbackEmail(Lead lead, EmailSettings settings)
		{
			return lead.ClientType switch
			{
				ClientType.LegalEntity => settings.CorporateEmail,
				ClientType.Individual => settings.IndividualEmail,
				_ => throw new ArgumentOutOfRangeException(
					nameof(lead.ClientType))
			};
		}
	}
}
