using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("/Images2")]
    [ApiController]
    public class Images2Controller : ControllerBase
    {
        private HttpClient _httpClient;
        private Options _options;
        public Images2Controller(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        private async Task<BlobContainerClient> GetCloudBlobContainer(string containerName)
        {
            BlobServiceClient blobServiceClient= new BlobServiceClient(_options.StorageConnectionString);
            BlobContainerClient blobContainerClient=blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            return blobContainerClient;
        }
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            BlobContainerClient blobContainerClient = await GetCloudBlobContainer(_options.FullImageContainerName);

            BlobClient blobClient;
            BlobSasBuilder blobSasBuilder;

            List<string> results = new List<string>();
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                blobSasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _options.FullImageContainerName,
                    BlobName = blobItem.Name,
                    ExpiresOn = DateTime.UtcNow.AddMinutes(5),
                    Protocol = SasProtocol.Https
                };
                blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

                results.Add(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri);
            }
            Console.Out.WriteLine("Got Images");
            return Ok(results);
        }
        [HttpGet("thumbnail")]
        public async Task<ActionResult<IEnumerable<string>>> GetThumbnails()
        {
            BlobContainerClient blobContainerClient = await GetCloudBlobContainer(_options.FullImageContainerName);

            BlobClient blobClient;
            BlobSasBuilder blobSasBuilder;

            List<string> results = new List<string>();
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                blobSasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _options.FullImageContainerName,
                    BlobName = blobItem.Name,
                    ExpiresOn = DateTime.UtcNow.AddMinutes(5),
                    Protocol = SasProtocol.Https
                };
                blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

                results.Add(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri);
            }
            Console.Out.WriteLine("Got Images");
            return Ok(results);
        }
        public async Task<ActionResult> Post()
        {
            Stream image = Request.Body;
            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);
            string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", string.Empty);
            BlobClient blobClient=containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(image);
            return Created(blobClient.Uri, null);
        }
        [HttpPost("thumbnail")]
        public async Task<ActionResult> PostThumbnail()
        {
            Stream image = Request.Body;
            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.ThumbnailImageContainerName);
            string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", string.Empty);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(image);
            return Created(blobClient.Uri, null);
        }

    }
}
