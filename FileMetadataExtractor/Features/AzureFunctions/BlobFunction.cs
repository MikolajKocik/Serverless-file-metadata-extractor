using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using BlobTriggerAttribute = Microsoft.Azure.Functions.Worker.BlobTriggerAttribute;

namespace FileMetadataExtractor.Features.AzureFunctions;


[StorageAccount("metadataacccount")]
public static class BlobFunction 
{
    [Function("ExtractMetaData")]
    [BlobOutput("test-samples-output/{name}-output.txt")]
    public static async Task<string> RunMetadataAsync(
        [BlobTrigger("test-samples-trigger/{name}")] BlobClient myBlob,
        string name,
        FunctionContext context,
        CancellationToken ct)
    {
        ILogger logger = context.GetLogger("BlobFunction");

        Response<BlobProperties> properties = await myBlob.GetPropertiesAsync(cancellationToken: ct);

        logger.LogInformation($"Name: {name}");
        logger.LogInformation($"ContentType: {properties.Value.ContentType}");

        foreach (var meta in properties.Value.Metadata)
        {
            logger.LogInformation($"Metadata: {meta.Key} = {meta.Value}");
        }

        return $"File: {name}\nContentType: {properties.Value.ContentType}\n";
    }

    [Function("CopyFile")]
    public static async Task RunAsync(
        [BlobTrigger("test-samples-copy/{name}")] Stream myBlobInput,
        [Blob("test-samples-output/{name}-output.txt")] Stream myBlobOutput,
        string name,
        FunctionContext context,
        CancellationToken ct)
    {
        ILogger logger = context.GetLogger("BlobFunction");

        await myBlobInput.CopyToAsync(myBlobOutput, 89120, ct);
    }
}
