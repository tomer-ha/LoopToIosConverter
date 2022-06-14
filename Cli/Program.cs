using CommandLine;
using LoopToIosConverter.Cli;
using LoopToIosConverter.Common;

await Parser.Default.ParseArguments<CliOptions>(args).WithParsedAsync(RunAsync);

static async Task RunAsync(CliOptions cliOptions)
{
    if (cliOptions.Mode == Mode.Infer)
    {
        cliOptions.Mode = InferMode();
    }

    var task = cliOptions.Mode switch
    {
        Mode.LoopToIos => RunLoopToIosAsync(),
        Mode.IosToLoop => RunIosToLoopAsync(),
        Mode.FixIos => RunIosToIosAsync(),
        _ => throw new NotImplementedException()
    };

    await task;

    Mode InferMode()
    {
        const string loopSuffix = "db";
        const string iosSuffix = "csv";

        if (cliOptions.InputFile.EndsWith(iosSuffix))
        {
            if (cliOptions.OutputFile.EndsWith(iosSuffix))
            {
                return Mode.FixIos;
            }
            else if (cliOptions.OutputFile.EndsWith(loopSuffix))
            {
                return Mode.IosToLoop;
            }
        }
        else if (cliOptions.InputFile.EndsWith(loopSuffix) && cliOptions.OutputFile.EndsWith(iosSuffix))
        {
            return Mode.LoopToIos;
        }

        throw new FormatException($"Could not infer conversion direction from file name suffixes. iOS suffix is \"{iosSuffix}\" and Loop suffix is \"{loopSuffix}\"");
    }

    async Task RunLoopToIosAsync()
    {
        using var converter = new LoopToIosConverter.Common.LoopToIosConverter(cliOptions.InputFile);
        await converter.ConvertAsync(cliOptions.OutputFile, cliOptions.PreserveOrder, cliOptions.SkipArchived);
    }

    async Task RunIosToLoopAsync()
    {
        var converter = new IosToLoopConverter(cliOptions.InputFile);
        await converter.ConvertAsync(cliOptions.OutputFile, cliOptions.PreserveOrder);
    }

    async Task RunIosToIosAsync()
    {
        var converter = new IosToIosFixer(cliOptions.InputFile);
        await converter.ConvertAsync(cliOptions.OutputFile);
    }
}