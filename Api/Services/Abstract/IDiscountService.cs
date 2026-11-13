using EducationCenter.DTO;

namespace EducationCenter.Services.Abstract
{
	public interface IDiscountService
	{
		decimal CalculateTotal(CalculateDiscountRequest request);
	}
}
