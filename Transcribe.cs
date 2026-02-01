using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Company.Function;

public class Transcribe
{
    private readonly ILogger<Transcribe> _logger;

    public Transcribe(ILogger<Transcribe> logger)
    {
        _logger = logger;
    }

    [Function("Transcribe")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        var youtube = new YoutubeClient();
        var videoId = "4xy_k7efPqY"; // e.g., "dQw4w9WgXcQ"
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

        await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, "audio.mp3");
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}