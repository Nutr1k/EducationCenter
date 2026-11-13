using EducationCenter.DTO;

namespace EducationCenter.Services.Abstract
{
	public interface ILeadService
	{
		Task CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken);
	}
}
