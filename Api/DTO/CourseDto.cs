namespace EducationCenter.DTO
{
	public class CourseDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public DateTime StartDate { get; set; }
	}
}
