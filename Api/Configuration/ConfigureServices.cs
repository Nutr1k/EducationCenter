using EducationCenter.Data.Abstract;
using EducationCenter.Data.Implementation;
using EducationCenter.Models;
using EducationCenter.Services.Abstract;
using EducationCenter.Services.Implementation;
using EducationCenterApi.BackgroundServices;
using EducationCenterApi.Data.Abstract;
using EducationCenterApi.Data.Implementation;

namespace EducationCenter.Configuration;

public static class ServiceCollectionExtensions
{
	public static void AddServices(this WebApplicationBuilder builder)
	{
		builder.AddCommonServices();
		builder.AddConfiguration();
		builder.AddApplicationServices();
		builder.AddInfrastructureServices();
		builder.AddBackgroundServices();
	}

	public static void AddCommonServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
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
		builder.Services.AddSingleton<ILeadRepository, InMemoryLeadRepository>();
	}
	public static void AddConfiguration(this WebApplicationBuilder builder)
	{
		builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
	}

	public static void AddBackgroundServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddHostedService<PendingLeadBackgroundService>();
	}
}

