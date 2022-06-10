namespace LoopToIosConverter.Common;

public class IosToIosTest
{
    private string _csvFilePath;

    public IosToIosTest(string csvFilePath)
    {
        _csvFilePath = csvFilePath;
    }

    public async Task ConvertAsync(string csvOutputFilePath)
    {
        var iosHabitList = new List<IosHabit>();

        foreach (var line in File.ReadLines(_csvFilePath))
        {
            iosHabitList.Add(IosHabitFormatter.FromCsvString(line));
        }

        using var file = new StreamWriter(csvOutputFilePath);
        foreach (var habit in iosHabitList)
        {
            await file.WriteLineAsync(IosHabitFormatter.ToCsvString(habit));
        }
    }
}