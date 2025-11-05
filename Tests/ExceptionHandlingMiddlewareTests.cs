using API;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;


namespace Tests
{

    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenExceptionThrown_Returns500AndLogsError()
        {
            // Arrange
            var logMock = new Mock<ILog>();
            var middleware = new ExceptionHandlingMiddleware(logMock.Object);

            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = (ctx) => throw new InvalidOperationException("Simulated failure");

            // Act
            await middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            responseBody.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(responseBody);
            var bodyText = await reader.ReadToEndAsync();
            Assert.Equal("An unexpected error occurred.", bodyText);

            logMock.Verify(l => l.error(It.Is<string>(msg => msg.Contains("Unhandled exception: Simulated failure"))), Times.Once);
        }
    }
}