namespace Common;

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