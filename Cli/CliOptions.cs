using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

internal enum Mode
{
    LoopToIos,
    IosToLoop,
    FixIos,
    Infer
}