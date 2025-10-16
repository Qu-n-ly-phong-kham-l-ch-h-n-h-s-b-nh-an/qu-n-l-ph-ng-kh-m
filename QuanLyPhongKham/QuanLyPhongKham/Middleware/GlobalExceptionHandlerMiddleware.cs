using System.Net;
using System.Text.Json;

namespace QuanLyPhongKhamApi.Middleware
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ApplicationException ex) // Bắt lỗi nghiệp vụ cụ thể
            {
                _logger.LogWarning("Lỗi nghiệp vụ: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409 Conflict
                context.Response.ContentType = "application/json";
                var error = new { message = ex.Message };
                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            }
            catch (Exception ex) // Bắt tất cả các lỗi không mong muốn khác
            {
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn: {Message}", ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    message = "Đã có lỗi xảy ra từ hệ thống, vui lòng thử lại sau.",
                    // Chỉ hiển thị chi tiết lỗi ở môi trường Development để bảo mật
                    detail = context.Request.Host.Host.Contains("localhost") ? ex.StackTrace : null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        }
    }
}