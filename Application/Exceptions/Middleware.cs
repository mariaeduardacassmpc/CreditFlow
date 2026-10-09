using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CreditFlow.Api.Exceptions;

public class Middleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
            throw ex;

        var (status, title, detail) = ex switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                "Acesso negado",
                "Você não tem permissão para realizar esta operação."),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso não encontrado",
                "O recurso solicitado não foi encontrado."),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                "Argumento inválido",
                ex.Message),

            _ => (
                HttpStatusCode.InternalServerError,
                "Erro interno do servidor",
                "Ocorreu um erro inesperado.")
        };

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            detail
        };

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}