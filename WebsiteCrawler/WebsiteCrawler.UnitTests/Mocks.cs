using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCrawler.UnitTests
{
    internal static class Mocks
    {
        internal static Mock<HttpMessageHandler> CreateHttpMessageHandlerMock(
            string responseBody,
            HttpStatusCode responseStatus,
            string responseContentType)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = responseStatus,
                Content = new StringContent(responseBody),
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(responseContentType);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            return handlerMock;
        }
    }
}