namespace Matrix;

public static class ApplicationBuilderExtensions
{
    // App Middleware
    public static void UseNullOrEmptyJsonMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<NullOrEmptyJsonMiddleware>();
    }
}
