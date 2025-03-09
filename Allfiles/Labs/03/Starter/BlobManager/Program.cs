using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class Program
{

    private const string blobServiceEndPoint = "";
    private const string storageAccountName = "";
    private const string storageAccountKey = "";
    public static async Task Main(string[] args)
    {
        StorageSharedKeyCredential storageSharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
        BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndPoint), storageSharedKeyCredential);
        AccountInfo accountInfo = await blobServiceClient.GetAccountInfoAsync();

        await Console.Out.WriteLineAsync($"Connected to the Azure Storage Account");
        await Console.Out.WriteLineAsync($"Account name:\t{storageAccountName}");
        await Console.Out.WriteLineAsync($"Account Kind:\t {accountInfo?.AccountKind}");
        await Console.Out.WriteLineAsync($"Account Sku:\t {accountInfo?.SkuName}");

        //To invoke the EnumerateAsync method
        //passing serviceClient variable as an argument
        await EnumerateContainerAsync(blobServiceClient);

        string existingContainerName = "raster-graphics";
        await EnumerateBlobsAsync(blobServiceClient, existingContainerName);
    
        string newContainerName = "vector-graphics";

        BlobContainerClient blobContainerClient = await GetContainerAsync(blobServiceClient, newContainerName);
    
        string uploadedBlobName = "graph.svg";
        BlobClient blobClient = await GetBlobAsync(blobContainerClient,uploadedBlobName);

        Console.Out.WriteLineAsync($"Blob Uri: \t{blobClient.Uri}");
    }



    private static async Task EnumerateContainerAsync(BlobServiceClient blobServiceClient)
    {
        await foreach (BlobContainerItem blobContainerItem in blobServiceClient.GetBlobContainersAsync())
        {
            // print the name of each container
            await Console.Out.WriteLineAsync($"Container:\t{blobContainerItem.Name}");
        }
    }

    private static async Task EnumerateBlobsAsync(BlobServiceClient blobServiceClient, string containerName)
    {

        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await Console.Out.WriteLineAsync($"Searching:\t {blobContainerClient.Name}");

        await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
        {
            await Console.Out.WriteLineAsync($"Existing Blob: \t{blobItem.Name}");
        }
    }

    private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient blobServiceClient, string containerName)
    {
        BlobContainerClient blobContainerClient=blobServiceClient.GetBlobContainerClient(containerName);
        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        await Console.Out.WriteLineAsync($"New Container: \t{blobContainerClient.Name}");

        return blobContainerClient;
    }

    private static async Task<BlobClient> GetBlobAsync(BlobContainerClient blobContainerClient, string blobName)
    {
        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
        bool exists = await blobClient.ExistsAsync();

        if (!exists)
        {
            await Console.Out.WriteLineAsync($"Blob {blobName} does not exists");

        }
        else
        {
            await Console.Out.WriteLineAsync($"Blob {blobName} exists");
        }

        return blobClient;
    }

}
