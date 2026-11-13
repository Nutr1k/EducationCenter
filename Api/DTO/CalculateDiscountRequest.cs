using EducationCenter.Models;

namespace EducationCenter.DTO;

public class CalculateDiscountRequest
{
	public List<CourseDto> Courses { get; set; } = new();

	public bool IsGraduate { get; set; }
}