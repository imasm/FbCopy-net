using CommandLine;

namespace FbCopy
{
    public abstract class CommonOptions
    {
        [Option('s', "Source", Required = true,
        HelpText = "Source database")]
        public string Source { get; set; }

        [Option('d', "Destination", Required = true,
        HelpText = "Destination database")]
        public string Destination { get; set; }

        [Option('v', "Verbose", 
         HelpText = "Verbose, show all errors with K option (default = off).")]
        public bool Verbose { get; set; }
    }

    [Verb("copy", HelpText = "Reads that definition from stdin and does the copying.")]
    public class CopyOptions : CommonOptions
    {
        [Option('u', "WithUpdates",
         HelpText = "If insert fails, try Update statement.")]
        public bool WithUpdates { get; set; }
    }

    [Verb("define", HelpText = "outputs a definition of fields in format:\n\ttable:common fields:missing fields:extra fields[:where clause|UID]")]
    public class DefineOptions : CommonOptions
    {
    }
}
