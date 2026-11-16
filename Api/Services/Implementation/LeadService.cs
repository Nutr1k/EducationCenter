using EducationCenter.DTO;
using EducationCenter.Enums;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenterApi.Data.Abstract;
using Microsoft.Extensions.Options;
using System.Threading;


namespace EducationCenter.Services.Implementation;

public class LeadService : ILeadService
{
	private readonly IEmailService	 _emailService;
	private readonly EmailSettings	 _emailSettings;
	private readonly ILeadRepository _leadRepository;


	public LeadService(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILeadRepository leadRepository)
	{
		_emailService = emailService;
		_emailSettings = emailSettings.Value;
		_leadRepository = leadRepository;
	}

	public async Task CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken)
	{
		var lead = new Lead
		{
			Name = request.Name,
			Phone = request.Phone,
			Email = request.Email,
			ClientType = request.ClientType,
			CreatedAt = DateTime.UtcNow
		};

		try
		{
			await _emailService.SendAsync(lead, cancellationToken);
		}
		catch (Exception) when (!cancellationToken.IsCancellationRequested)
		{
			var fallbackEmail = request.ClientType switch
			{
				ClientType.LegalEntity => _emailSettings.CorporateEmail,
				ClientType.Individual => _emailSettings.IndividualEmail,
				_ => throw new ArgumentOutOfRangeException(nameof(request.ClientType))
			};

			try
			{
				await _emailService.SendFallbackAsync(
					fallbackEmail,
					lead,
					cancellationToken);
			}
			catch (Exception) when (!cancellationToken.IsCancellationRequested)
			{
				await _leadRepository.SaveAsync(lead, cancellationToken);
			}
		}
	}
}

