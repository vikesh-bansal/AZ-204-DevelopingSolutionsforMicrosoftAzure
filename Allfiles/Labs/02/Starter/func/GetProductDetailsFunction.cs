using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

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
        public  async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, [BlobInput("products/{productId}.json", Connection ="AzureWebProductsStorage")] string? productData)
        {
            try
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                string? productId = queryParams["productId"];
                if (string.IsNullOrEmpty(productId))
                {
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Please provide a productId in the query string.");
                    return badRequestResponse;
                }
                if (productData == null)
                { 
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                   await notFoundResponse.WriteStringAsync($"Product with ID {productId} not found.");
                    return notFoundResponse;
                }

                _logger.LogInformation($"Fetched details for Product ID {productId}");

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
            await    response.WriteStringAsync(productData);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}\n{ex.StackTrace}");

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
               await errorResponse.WriteStringAsync("An error occurred while processing your request.");
                return errorResponse;
            }
        }
    }
}
