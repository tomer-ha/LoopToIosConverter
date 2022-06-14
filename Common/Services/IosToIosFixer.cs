namespace LoopToIosConverter.Common;

public sealed class IosToIosFixer
{
    private readonly string _csvFilePath;

    public IosToIosFixer(string csvFilePath)
    {
        _csvFilePath = csvFilePath;
    }

    public async Task ConvertAsync(string csvOutputFilePath)
    {
        using var fileReader = new StreamReader(_csvFilePath);
        using var fileWriter = new StreamWriter(csvOutputFilePath);
        fileWriter.NewLine = "\n";

        string? line;

        while ((line = await fileReader.ReadLineAsync()) != null)
        {
            await fileWriter.WriteLineAsync(line);
        }
    }
}