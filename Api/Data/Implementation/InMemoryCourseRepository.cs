using EducationCenter.Data.Abstract;
using EducationCenter.Models;

namespace EducationCenter.Data.Implementation;

public class InMemoryCourseRepository : ICourseRepository
{
	private readonly List<Course> _courses = new List<Course>
	{
		new Course
		{
			Id = 1,
			Name = "C#",
			Price = 15000m,
			StartDate = DateTime.Now.AddDays(3)
		},
		new Course
		{
			Id = 2,
			Name = "Entity Framework Core",
			Price = 25000m,
			StartDate = DateTime.Now.AddDays(10)
		},
		new Course
		{
			Id = 3,
			Name = "HTML",
			Price = 10000m,
			StartDate = DateTime.Now.AddDays(5)
		},
		new Course
		{
			Id = 4,
			Name = "ASP.NET",
			Price = 20000m,
			StartDate = DateTime.Now.AddDays(2)
		}
	};

	public Task<List<Course>> GetAllAsync()
	{
		return Task.FromResult(_courses);
	}
}
