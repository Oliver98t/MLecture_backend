namespace Common;

public static class GlobalConsts
{
    public static string? LocalEnvironment = Environment.GetEnvironmentVariable("LocalEnvironment");
    public static string? GroqAPIEnable = Environment.GetEnvironmentVariable("GroqAPIEnable");
    public static string? TranscriptAPIEnable = Environment.GetEnvironmentVariable("TranscriptAPIEnable");
}


public static class QueueConsts
{
    public const string triggerSuccessMessage = "ready";
    public const string triggerFailMessage = "Not ready";
    public const string transcriptionJob = "transcription-job";
    public const string transcriptionResults = "transcribe-results";
}

public static class TableConsts
{
    public const string transcriptionTable = "transcriptions";
    public const string notesTable = "notes";
    public const string userTable = "users";
}

public class StartCreateNotesQueueData
{
    public string User { get; set; }
    public string Url { get; set; }
    public string JobId { get; set; }

    public StartCreateNotesQueueData(string user, string url, string jobId)
    {
        User = user;
        Url = url;
        JobId = jobId;
    }
}

public class TranscribeTriggerQueueData
{
    public string User {get; set;}
    public string Transcript {get; set;}
    public string JobId {get; set;}

    public TranscribeTriggerQueueData(string user, string transcript, string jobId)
    {
        User = user;
        Transcript = transcript;
        JobId = jobId;
    }
}