using System.Globalization;

namespace LoopToIosConverter.Common;

public static class IosHabitFormatter
{
    private const string _creationTimeFormat = "yyyy-MM-d-H:m:ss.ffff";
    private const string _repetitionDateFormat = "yyyyMMdd";

    public static IosHabit FromCsvString(string csvLine)
    {
        var split = csvLine.Split(',');
        if (split.Length != 12) throw new FormatException();

        var id = int.Parse(split[0]);
        var title = split[1].Trim('"');
        var question = split[3].Trim('"');
        var color = (IosColor)int.Parse(split[4]);
        var createdTime = DateTime.ParseExact(split[5], _creationTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        var frequency = new IosFrequency(split[6].Trim('"'));
        var unparsedNotificationSettings = split[7].Trim('"');
        var notificationSettings = string.IsNullOrEmpty(unparsedNotificationSettings) ? null : new IosNotificationSettings(unparsedNotificationSettings);
        var dateAndValues = ParseDateAndValues(split[10]);

        return (split[2] == "0") ? CreateBasicHabit() : CreateProgressiveHabit();

        IosBasicHabit CreateBasicHabit() =>
            new(id, title, question, color, createdTime, frequency, notificationSettings, dateAndValues.Select(dateAndValue => dateAndValue.Date).ToList());

        IosProgressiveHabit CreateProgressiveHabit() =>
            new(id, title, question, color, createdTime, frequency, notificationSettings, decimal.Parse(split[8]), split[9], dateAndValues);
    }

    public static string ToCsvString(IosHabit habit)
    {
        var baseString = string.Join(",", 
            habit.Id.ToString(),
            Quoted(habit.Title),
            habit is IosBasicHabit ? "0" : "1",
            string.IsNullOrEmpty(habit.Question) ? string.Empty : Quoted(habit.Question),
            habit.Color.ToString("D"),
            habit.CreationTime.ToString(_creationTimeFormat),
            Quoted(habit.Frequency.ToCsvString()),
            Quoted(habit.NotificationSettings?.ToCsvString() ?? string.Empty),
            habit.Target.ToString("F1", CultureInfo.InvariantCulture));

        var suffixString = habit switch
        {
            IosBasicHabit basicHabit => GetBasicSuffix(basicHabit),
            IosProgressiveHabit progressiveHabit => GetProgressiveSuffix(progressiveHabit),
            _ => throw new NotImplementedException()
        };

        return string.Join(",", baseString, suffixString);

        static string Quoted(string? str) => 
            "\"" + str + "\"";

        static string GetBasicSuffix(IosBasicHabit basicHabit) =>
            string.Join(",", string.Empty, basicHabit.CompletedDates.ToIosDateAndValue().ToCsvFormat(), "0");

        static string GetProgressiveSuffix(IosProgressiveHabit progressiveHabit) =>
            string.Join(",", progressiveHabit.Units, progressiveHabit.DateAndValues.ToCsvFormat(), "1");
    }

    private static IReadOnlyCollection<IosDateAndValue> ParseDateAndValues(string csvFormat)
    {
        if (string.IsNullOrEmpty(csvFormat))
        {
            return Array.Empty<IosDateAndValue>();
        }

        return csvFormat.Split('-').
            Select(csvDateAndValue => csvDateAndValue.Split(':')).
            Select(splitCsvDateAndValue => new IosDateAndValue(
                DateOnly.ParseExact(splitCsvDateAndValue[0], _repetitionDateFormat, CultureInfo.InvariantCulture),
                decimal.Parse(splitCsvDateAndValue[1]))).
            ToList();
    }

    private static string ToCsvFormat(this IReadOnlyCollection<IosDateAndValue> dateAndValues) =>
        string.Join("-", dateAndValues.Select(dateAndValue => $"{dateAndValue.Date.ToString(_repetitionDateFormat)}:{dateAndValue.Value}").ToArray());

    private static IReadOnlyCollection<IosDateAndValue> ToIosDateAndValue(this IReadOnlyCollection<DateOnly> completedDates) =>
        completedDates.Select(date => new IosDateAndValue(date, 1)).ToList();
}
