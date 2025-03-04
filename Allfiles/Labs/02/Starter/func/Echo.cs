using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace func
{
    public class Echo
    {
        private readonly ILogger<Echo> _logger;

        public Echo(ILogger<Echo> logger)
        {
            _logger = logger;
        }

        [Function("Echo")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            string requestBody;
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            using (StreamReader reader = new StreamReader(req.Body)) // Use 'using' statement
            {
                requestBody = await reader.ReadToEndAsync();
                
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(requestBody);
            return response;
        }
    }
}
