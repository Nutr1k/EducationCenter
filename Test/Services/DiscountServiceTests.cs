using EducationCenter.DTO;
using EducationCenter.Services.Implementation;
using Xunit;

namespace EducationCenterTests.Services;

public class DiscountServiceTests
{
	private readonly DiscountService _discountService = new();

	[Fact]
	public void CalculateTotal_Graduate_ReturnsPriceWith30PercentDiscount()
	{

		var request = new CalculateDiscountRequest
		{
			IsGraduate = true,
			Courses = new List<CourseDto>
			{
				new CourseDto
				{
					Price = 1000m,
					StartDate = DateTime.Now.AddDays(10)
				}
			}
		};


		var result = _discountService.CalculateTotal(request);
		Assert.Equal(700m, result);
	}

	[Fact]
	public void CalculateTotal_CourseStartsWithinFiveDays_ReturnsPriceWith20PercentDiscount()
	{
		var request = new CalculateDiscountRequest
		{
			IsGraduate = false,
			Courses = new List<CourseDto>
			{
				new CourseDto
				{
					Price = 1000m,
					StartDate = DateTime.Now.AddDays(3)
				}
			}
		};


		var result = _discountService.CalculateTotal(request);

		Assert.Equal(800m, result);
	}

	[Fact]
	public void CalculateTotal_BothDiscountsAvailable_ReturnsPriceWith30PercentDiscount()
	{
		var request = new CalculateDiscountRequest
		{
			IsGraduate = true,

			Courses = new List<CourseDto>
			{
				new CourseDto
				{
					Price = 1000m,
					StartDate = DateTime.Now.AddDays(3)
				}
			}
		};

		var result = _discountService.CalculateTotal(request);
		Assert.Equal(700m, result);
	}
}