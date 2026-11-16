using EducationCenter.Models;

namespace EducationCenterApi.Data.Abstract
{
	public interface ILeadRepository
	{
		Task SaveAsync(Lead lead, CancellationToken cancellationToken);

		Task<IReadOnlyCollection<Lead>> GetPendingAsync(CancellationToken cancellationToken);

		Task MarkProcessedAsync(int id, CancellationToken cancellationToken);
	}
}
