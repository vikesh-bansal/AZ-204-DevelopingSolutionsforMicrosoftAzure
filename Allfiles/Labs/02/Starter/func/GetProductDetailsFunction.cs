using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace func
{
    public class GetProductDetailsFunction
    {
        private readonly ILogger<GetProductDetailsFunction> _logger;

        public GetProductDetailsFunction(ILogger<GetProductDetailsFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetProductDetailsFunction")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, [BlobInput("products/{productId}.json", Connection ="AzureWebProductsStorage")] string? productData)
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string? productId = queryParams["productId"];
            if (string.IsNullOrEmpty(productId))
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                badRequestResponse.WriteString("Please provide a productId in the query string.");
                return badRequestResponse;
            }
            if(productData == null)
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                notFoundResponse.WriteString($"Product with ID {productId} not found.");
                return notFoundResponse;
            }

            _logger.LogInformation($"Fetched details for Product ID {productId}");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(productData);
            return response;
        }
    }
}
