using EducationCenter.Models;

namespace EducationCenter.Services.Abstract
{
	public interface IEmailService
	{
		Task SendAsync(Lead lead, CancellationToken cancellationToken);

		Task SendFallbackAsync(string email, Lead lead, CancellationToken cancellationToken);
	}
}
