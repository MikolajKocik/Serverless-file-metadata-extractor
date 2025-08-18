using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using BlobTriggerAttribute = Microsoft.Azure.Functions.Worker.BlobTriggerAttribute;

namespace FileMetadataExtractor.Features.AzureFunctions;

/// <summary>
/// Processes a blob triggered by an Azure Blob Storage event and writes the output to a specified blob.
/// </summary>
/// <remarks>This function is triggered by a blob upload or modification event in the "test-samples-trigger"
/// container. It logs the blob's name, content type, and metadata, and writes a formatted string containing the blob's
/// name and content type to the "test-samples-output" container.</remarks>
[StorageAccount("metadataacccount")]
public static class BlobFunction
{
    [Function(nameof(BlobFunction))]
    [BlobOutput("test-samples-output/{name}-output.txt")]
    public static async Task<string> RunAsync(
        [BlobTrigger("test-samples-trigger/{name}")] BlobClient myBlob,
        string name,
        FunctionContext context)
    {
        var logger = context.GetLogger("BlobFunction");

        Response<BlobProperties> properties = await myBlob.GetPropertiesAsync();

        logger.LogInformation($"Name: {name}");
        logger.LogInformation($"ContentType: {properties.Value.ContentType}");

        foreach (var meta in properties.Value.Metadata)
        {
            logger.LogInformation($"Metadata: {meta.Key} = {meta.Value}");
        }

        return $"File: {name}\nContentType: {properties.Value.ContentType}\n";
    }
}
