using EducationCenter.Data.Abstract;
using EducationCenter.DTO;
using EducationCenter.Models;
using EducationCenter.Services.Implementation;
using Moq;
using Xunit;

namespace EducationCenterTests.Services;

public class DiscountServiceTests
{
	private readonly Mock<ICourseRepository> _courseRepositoryMock = new();
	private readonly Mock<TimeProvider> _timeProviderMock = new();

	private readonly DiscountService _discountService;

	private readonly DateTime _now = new(2026, 11, 17, 12, 0, 0);

	public DiscountServiceTests()
	{
		_timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(_now, TimeSpan.Zero));

		_timeProviderMock.Setup(x => x.LocalTimeZone).Returns(TimeZoneInfo.Utc);

		_discountService = new DiscountService(_courseRepositoryMock.Object, _timeProviderMock.Object);
	}

	[Fact]
	//Выпускник = скидка 30%
	public async Task CalculateTotalAsync_Graduate_ReturnsPriceWith30PercentDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(10)
		});

		var request = CreateRequest(isGraduate: true, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(700m, result);
	}

	[Fact]
	//Курс начинается в течение 5 дней — скидка 20%
	public async Task CalculateTotalAsync_CourseStartsWithinFiveDays_ReturnsPriceWith20PercentDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(3)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(800m, result);
	}

	
	[Fact]
	//Одновременно доступны обе скидки = применяется скидка 30%
	public async Task CalculateTotalAsync_BothDiscountsAvailable_ReturnsPriceWith30PercentDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(3)
		});

		var request = CreateRequest(isGraduate: true, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(700m, result);
	}

	[Fact]
	//Нет оснований для скидки = полная стоимость
	public async Task CalculateTotalAsync_NoDiscount_ReturnsFullPrice()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(10)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(1000m, result);
	}

	private void SetupCourses(params Course[] courses)
	{
		_courseRepositoryMock
			.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
			.ReturnsAsync(courses.ToList());
	}

	private static CalculateDiscountRequest CreateRequest(bool isGraduate, int courseId)
	{
		return new CalculateDiscountRequest
		{
			IsGraduate = isGraduate,
			Courses = new List<CourseDto>
			{
				new CourseDto { Id = courseId }
			}
		};
	}

	[Fact]
	//До начала курса ровно 5 дней = скидка не применяется
	public async Task CalculateTotalAsync_ExactlyFiveDays_NoDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(5)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(1000m, result);
	}

	[Fact]
	//До начала курса осталось 4 дня 23 часа 59 минут = скидка 20%
	public async Task CalculateTotalAsync_LessThanFiveDays_GetsDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(5).AddMinutes(-1)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(800m, result);
	}
	[Fact]
	//Курс уже начался - полная стоимость
	public async Task CalculateTotalAsync_CourseStarted_ReturnsFullPrice()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddDays(-1)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(1000m, result);
	}

	[Fact]
	//Несколько курсов с разными скидками
	public async Task CalculateTotalAsync_MultipleCourses_ReturnsTotal()
	{
		SetupCourses(
			new Course
			{
				Id = 1,
				Price = 1000m,
				StartDate = _now.AddDays(3)
			},
			new Course
			{
				Id = 2,
				Price = 2000m,
				StartDate = _now.AddDays(10)
			});

		var request = new CalculateDiscountRequest
		{
			IsGraduate = false,
			Courses = new List<CourseDto>
			{
				new() { Id = 1 },
				new() { Id = 2 }
			}
		};

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(2800m, result);
	}

	[Fact]
	//Пустая корзина
	public async Task CalculateTotalAsync_EmptyBasket_ReturnsZero()
	{
		SetupCourses();

		var request = new CalculateDiscountRequest
		{
			IsGraduate = false,
			Courses = new List<CourseDto>()
		};

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(0m, result);
	}

	[Fact]
	//Несуществующий курс
	public async Task CalculateTotalAsync_CourseNotFound_ThrowsException()
	{
		SetupCourses();

		var request = CreateRequest(isGraduate: false, courseId: 999);

		await Assert.ThrowsAsync<InvalidOperationException>(
			() => _discountService.CalculateTotalAsync(request));
	}

	[Fact]
	//Курс начинается прямо сейчас
	public async Task CalculateTotalAsync_CourseStartsNow_ReturnsFullPrice()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(1000m, result);
	}

	[Fact]
	//Курс начинается через минуту
	public async Task CalculateTotalAsync_CourseStartsInOneMinute_Returns20PercentDiscount()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 1000m,
			StartDate = _now.AddMinutes(1)
		});

		var request = CreateRequest(isGraduate: false, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(800m, result);
	}

	[Fact]
	//Нулевая стоимость курса
	public async Task CalculateTotalAsync_ZeroPrice_ReturnsZero()
	{
		SetupCourses(new Course
		{
			Id = 1,
			Price = 0m,
			StartDate = _now.AddDays(10)
		});

		var request = CreateRequest(isGraduate: true, courseId: 1);

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(0m, result);
	}

	[Fact]
	//Несколько курсов для выпускника
	public async Task CalculateTotalAsync_GraduateWithMultipleCourses_ReturnsCorrectTotal()
	{
		SetupCourses(
			new Course
			{
				Id = 1,
				Price = 1000m,
				StartDate = _now.AddDays(10)
			},
			new Course
			{
				Id = 2,
				Price = 2000m,
				StartDate = _now.AddDays(3)
			});

		var request = new CalculateDiscountRequest
		{
			IsGraduate = true,
			Courses = new List<CourseDto>
			{
				new() { Id = 1 },
				new() { Id = 2 }
			}
		};

		var result = await _discountService.CalculateTotalAsync(request);

		Assert.Equal(2100m, result);
	}
}