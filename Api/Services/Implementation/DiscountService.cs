using EducationCenter.DTO;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;

namespace EducationCenter.Services.Implementation;

public class DiscountService : IDiscountService
{
	public decimal CalculateTotal(CalculateDiscountRequest request)
	{
		decimal total = 0;
		var now = DateTime.Now;
		
		var courses = request.Courses.Select(course => new Course
		{
			Id = course.Id,
			Name = course.Name,
			Price = course.Price,
			StartDate = course.StartDate
		});


		foreach (var course in courses)
		{
			decimal discount = 0;

			if (request.IsGraduate)
			{
				discount = 30;
			}

			var timeUntilStart = course.StartDate - now;

			if (timeUntilStart > TimeSpan.Zero && timeUntilStart < TimeSpan.FromDays(5))
			{
				discount = Math.Max(discount, 20);
			}

			var finalPrice = course.Price * (1 - discount / 100);

			total += finalPrice;
		}

		return total;
	}
}

