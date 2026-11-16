using EducationCenter.Data.Abstract;
using EducationCenter.DTO;
using EducationCenter.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
	private readonly ICourseRepository _courseRepository;
	private readonly IDiscountService _discountService;

	public DiscountsController(ICourseRepository courseRepository, IDiscountService discountService)
	{
		_courseRepository = courseRepository;
		_discountService = discountService;
	}

	[HttpGet("courses")]
	public async Task<IActionResult> GetCourses()
	{
		var courses = await _courseRepository.GetAllAsync();

		return Ok(courses);
	}

	[HttpPost("calculate")]
	public IActionResult Calculate([FromBody] CalculateDiscountRequest request)
	{
		var total = _discountService.CalculateTotalAsync(request);

		return Ok(new
		{
			Total = total
		});
	}
}
