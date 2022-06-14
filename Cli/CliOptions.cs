using CommandLine;

namespace LoopToIosConverter.Cli;

internal class CliOptions
{
    [Option('i', "inputFile", Required = true, HelpText = "Input file to be converted")]
    public string InputFile { get; set; } = null!;

    [Option('o', "outputFile", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; } = null!;

    [Option('m', "mode", Required = false, Default = Mode.Infer, HelpText = "Conversion direction")]
    public Mode Mode { get; set; }

    [Option('p', "preserveOrder", Required = false, Default = false, HelpText = "Add numbers to habits so they can be sorted in iOS")]
    public bool PreserveOrder { get; set; } = false;

    [Option('s', "skipArchived", Required = false, Default = false, HelpText = "Do not convert archived habits from Loop")]
    public bool SkipArchived { get; set; } = false;
}

internal enum Mode
{
    LoopToIos,
    IosToLoop,
    FixIos,
    Infer
}