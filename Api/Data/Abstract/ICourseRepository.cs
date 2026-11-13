using EducationCenter.Models;

namespace EducationCenter.Data.Abstract;

public interface ICourseRepository
{
	Task<List<Course>> GetAllAsync();
}

