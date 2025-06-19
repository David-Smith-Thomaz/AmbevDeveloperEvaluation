using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string type = "ServerError";
            string error = "An unexpected error occurred.";
            string detail = exception.Message;
            IEnumerable<ValidationErrorDetail>? validationErrors = null;

            switch (exception)
            {
                case DomainValidationException domainValidationException:
                    statusCode = HttpStatusCode.BadRequest;
                    type = "DomainValidation";
                    error = domainValidationException.Message;
                    validationErrors = domainValidationException.Errors;
                    detail = string.Join("; ", validationErrors.Select(e => $"{e.Error}: {e.Detail}"));
                    break;
                case DomainException domainException:
                    statusCode = HttpStatusCode.BadRequest;
                    type = "DomainError";
                    error = domainException.Message;
                    detail = domainException.Message;
                    break;
                case InvalidOperationException invalidOperationException when invalidOperationException.Message.Contains("not found", StringComparison.OrdinalIgnoreCase):
                    statusCode = HttpStatusCode.NotFound; 
                    type = "ResourceNotFound";
                    error = invalidOperationException.Message;
                    detail = invalidOperationException.Message;
                    break;
                case ArgumentException argumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    type = "InvalidArgument";
                    error = argumentException.Message;
                    detail = argumentException.Message;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var apiResponse = new
            {
                type,
                error,
                detail = validationErrors != null && validationErrors.Any() ? (object)validationErrors : (object)detail
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}