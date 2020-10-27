using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;

namespace SlideExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new CommandLineBuilder();

            SetupCommand(builder.Command);

            builder.UseVersionOption();

            builder.UseHelp();
            builder.UseSuggestDirective();
            builder.RegisterWithDotnetSuggest();
            builder.UseParseErrorReporting();
            builder.UseExceptionHandler();

            Parser parser = builder.Build();
            parser.Invoke(args);
        }

        static void SetupCommand(Command command)
        {
            command.Description = "Export whole slide image to other formats.";

            command.AddOption(Output());
            command.AddOption(Format());
            command.AddOption(TileSize());
            command.AddOption(Overlap());

            command.AddArgument(new Argument<FileInfo>()
            {
                Name = "source",
                Description = "The whole slide image file.",
                Arity = ArgumentArity.ExactlyOne
            }.ExistingOnly());

            command.Handler = CommandHandler.Create<FileInfo, FileInfo, string, int, int>(Run);

            static Option Output() =>
                new Option(new[] { "--output", "--out", "-o" }, "Output location.")
                {
                    Name = "output",
                    Argument = new Argument<FileInfo>() { Arity = ArgumentArity.ExactlyOne }
                };

            static Option Format() =>
                new Option("--format", "Format. (DeepZoom)")
                {
                    Name = "format",
                    Argument = new Argument<string>() { Arity = ArgumentArity.ExactlyOne }
                };

            static Option TileSize() =>
                new Option("--tile-size", "Tile size.")
                {
                    Name = "tileSize",
                    Argument = new Argument<int>(_ => 256) { Arity = ArgumentArity.ZeroOrOne }
                };

            static Option Overlap() =>
                new Option("--overlap", "Overlap.")
                {
                    Name = "overlap",
                    Argument = new Argument<int>(_ => 0) { Arity = ArgumentArity.ZeroOrOne }
                };
        }

        private static void Run(FileInfo source, FileInfo output, string format, int tileSize, int overlap)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (output is null)
            {
                Console.WriteLine("Error: Output location is not specified.");
                return;
            }
            if (string.IsNullOrEmpty(format))
            {
                Console.WriteLine("Error: Output format is not specified.");
                return;
            }

            if (!"DeepZoom".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Error: Only DeepZoom format is supported.");
                return;
            }

            DeepZoomExporter.Run(source, output, tileSize, overlap);
        }
    }
}
