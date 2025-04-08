using Product_Service.Servise;
using Product_Service.SQL.Repository;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string authHeader = context.Request.Headers["Authorization"];
        string token = authHeader?.Trim();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Войдите в аккаунт");
            return;
        }

        string email = await JWTservise.ExtractEmailFromTokenAsync(token);

        if (string.IsNullOrEmpty(email))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Не удалось извлечь email из токена");
            return;
        }

        int userId = await EmsilRepos.GetIdlByEmailAsync(email);

        if (userId == 0)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Войдите в аккаунт");
            return;
        }

        bool isValid = await JWTservise.ValidateJwtTokenAsync(token, email);

        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Войдите в аккаунт");
            return;
        }

        await _next(context);
    }
}

public static class JwtAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtAuthenticationMiddleware>();
    }
}
