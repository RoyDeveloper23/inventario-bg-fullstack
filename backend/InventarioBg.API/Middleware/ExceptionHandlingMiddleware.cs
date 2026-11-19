using InventarioBg.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InventarioBg.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error procesando la solicitud {TraceId}.", context.TraceIdentifier);
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (status, title, detail) = ObtenerRespuesta(exception);
            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            }, cancellationToken: context.RequestAborted);
        }
    }

    private static (int Status, string Title, string Detail) ObtenerRespuesta(Exception exception)
    {
        if (exception is CredencialesInvalidasException)
        {
            return (StatusCodes.Status401Unauthorized, "No autorizado", "Credenciales inválidas.");
        }

        if (exception is RecursoNoEncontradoException)
        {
            return (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message);
        }

        if (exception is ConflictoAplicacionException)
        {
            return (StatusCodes.Status409Conflict, "Conflicto", exception.Message);
        }

        if (exception is ArgumentException)
        {
            return (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message);
        }

        if (exception is DbUpdateException { InnerException: PostgresException postgresException })
        {
            return postgresException.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation =>
                    (StatusCodes.Status409Conflict, "Conflicto", "El recurso ya existe."),
                PostgresErrorCodes.CheckViolation =>
                    (StatusCodes.Status400BadRequest, "Solicitud inválida", "Uno o más valores no son válidos."),
                PostgresErrorCodes.ForeignKeyViolation =>
                    (StatusCodes.Status409Conflict, "Conflicto", "No se puede completar la operación por sus relaciones."),
                _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
            };
        }

        return (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.");
    }
}
