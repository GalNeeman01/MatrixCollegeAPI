﻿namespace Matrix;

public static class ServiceCollectionExtensions
{
    // Database related services
    public static void AddDbServices(this IServiceCollection services)
    {
        services.AddDbContext<MatrixCollegeContext>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IProgressService, ProgressService>();

        services.AddScoped<IValidationService, ValidationService>();
    }

    // Other services
    public static void AddUtilityServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenService, TokenService>();
        services.AddAutoMapper(typeof(MappingProfile));
    }

    public static void AddDaoServices(this IServiceCollection services)
    {
        services.AddScoped<ICourseDao, CourseDao>();
        services.AddScoped<ILessonDao, LessonDao>();
        services.AddScoped<IUserDao, UserDao>();
        services.AddScoped<IProgressDao, ProgressDao>();
        services.AddScoped<IEnrollmentDao, EnrollmentDao>();
    }

    // Add CORS policies
    public static void AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });
    }
}
