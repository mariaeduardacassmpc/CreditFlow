using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Application.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Falha de validação da requisição.");

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Title = "Requisição inválida",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400"
            };

            await WriteProblemDetails(context, problem);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acesso não autorizado.");

            var problem = new ProblemDetails
            {
                Title = "Não autorizado",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://httpstatuses.com/401"
            };

            await WriteProblemDetails(context, problem);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Operação inválida.");

            var problem = new ProblemDetails
            {
                Title = "Operação inválida",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400"
            };

            await WriteProblemDetails(context, problem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado.");

            var problem = new ProblemDetails    
            {
                Title = "Erro interno",
                Detail = "Ocorreu um erro interno no servidor.",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://httpstatuses.com/500"
            };

            await WriteProblemDetails(context, problem);
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
            throw new InvalidOperationException("A resposta HTTP já foi iniciada.");

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}