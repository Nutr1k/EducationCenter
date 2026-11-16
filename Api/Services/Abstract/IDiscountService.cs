using EducationCenter.DTO;

namespace EducationCenter.Services.Abstract
{
	public interface IDiscountService
	{
		Task<decimal> CalculateTotalAsync(CalculateDiscountRequest request);
	}
}
