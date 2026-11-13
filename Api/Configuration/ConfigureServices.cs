using EducationCenter.Data.Abstract;
using EducationCenter.Data.Implementation;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenter.Services.Implementation;

namespace EducationCenter.Configuration;

public static class ServiceCollectionExtensions
{
	public static void AddServices(this WebApplicationBuilder builder)
	{
		builder.AddCommonServices();
		builder.AddConfiguration();
		builder.AddApplicationServices();
		builder.AddInfrastructureServices();
	}

	public static void AddCommonServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
	}

	public static void AddApplicationServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddScoped<ILeadService, LeadService>();
		builder.Services.AddScoped<IDiscountService, DiscountService>();
	}

	public static void AddInfrastructureServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddScoped<IEmailService, EmailService>();
		builder.Services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
	}
	public static void AddConfiguration(this WebApplicationBuilder builder)
	{
		builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
	}
}

