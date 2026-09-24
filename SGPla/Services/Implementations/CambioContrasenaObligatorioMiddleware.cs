namespace SGPla.Services.Implementations;

public sealed class CambioContrasenaObligatorioMiddleware
{
    private readonly RequestDelegate _next;

    public CambioContrasenaObligatorioMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var debeCambiar = context.User.Identity?.IsAuthenticated == true &&
                          context.User.FindFirst("DebeCambiarContrasena")?.Value == "true";
        if (!debeCambiar || path.StartsWithSegments("/api") || EsRecursoEstatico(path) ||
            path.StartsWithSegments("/Cuenta/CambiarContrasena") ||
            path.StartsWithSegments("/Login/Logout"))
        {
            await _next(context);
            return;
        }

        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        {
            context.Response.Redirect("/Cuenta/CambiarContrasena");
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
    }

    private static bool EsRecursoEstatico(PathString path) =>
        System.IO.Path.HasExtension(path.Value) || path.StartsWithSegments("/_content") ||
        path.StartsWithSegments("/_framework");
}
