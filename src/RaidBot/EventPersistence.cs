using Azure.Identity;
using Azure.Storage.Blobs;
using System.Text.Json;

namespace RaidBot;

internal class AzurePersistence : IEventPersistence
{
    private readonly BlobContainerClient _client;

    public AzurePersistence()
    {
        _client = new BlobServiceClient(new Uri("https://valhallalootliststorage.blob.core.windows.net/"), new DefaultAzureCredential())
            .GetBlobContainerClient("raidbot");
    }

    public async Task<T?> LoadAsync<T>(ulong id) where T : class
    {
        var blob = _client.GetBlobClient($"{typeof(T).Name}-{id}.json");
        if (await blob.ExistsAsync())
        {
            try
            {
                var stream = await blob.DownloadStreamingAsync();
                return await JsonSerializer.DeserializeAsync<T>(stream.Value.Content);
            }
            catch
            {
            }
        }
        return null;
    }

    public async Task SaveAsync(ulong id, object content)
    {
        await using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, content);
        ms.Seek(0, SeekOrigin.Begin);
        await _client.UploadBlobAsync($"{content.GetType().Name}-{id}.json", ms);
    }
}

internal class LocalPersistence : IEventPersistence
{
    private readonly DirectoryInfo _directory;

    public LocalPersistence(string folder)
    {
        _directory = Directory.CreateDirectory(folder);
    }

    public async Task SaveAsync(ulong id, object content)
    {
        await using var fs = File.Create(Path.Combine(_directory.FullName, $"{content.GetType().Name}-{id}.json"));
        await JsonSerializer.SerializeAsync(fs, content);
    }

    public async Task<T?> LoadAsync<T>(ulong id) where T : class
    {
        var path = Path.Combine(_directory.FullName, $"{typeof(T).Name}-{id}.json");
        if (File.Exists(path))
        {
            try
            {
                await using var fs = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<T>(fs);
            }
            catch
            {
            }
        }
        return null;
    }
}
