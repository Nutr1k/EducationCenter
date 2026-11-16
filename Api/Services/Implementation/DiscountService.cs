using EducationCenter.Data.Abstract;
using EducationCenter.DTO;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using System.Threading;

namespace EducationCenter.Services.Implementation;

public class DiscountService : IDiscountService
{
	private readonly ICourseRepository _courseRepository;
	private readonly TimeProvider _timeProvider;

	public DiscountService(ICourseRepository courseRepository, TimeProvider timeProvider)
	{
		_courseRepository = courseRepository;
		_timeProvider = timeProvider;
	}

	public async Task<decimal> CalculateTotalAsync(CalculateDiscountRequest request)
	{
		decimal total = 0;

		var now = _timeProvider.GetLocalNow().DateTime;

		var courseIds = request.Courses
			.Select(x => x.Id)
			.ToList();

		var courses = await _courseRepository.GetByIdsAsync(courseIds);
		
		if (courses.Count != courseIds.Distinct().Count())
			throw new InvalidOperationException("Некоторые курсы не найдены.");

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

