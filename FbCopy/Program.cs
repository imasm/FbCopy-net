using CommandLine;
using CommandLine.Text;
using System;

namespace FbCopy
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<DefineOptions, CopyOptions>(args)
            .MapResult(
                (CopyOptions opts) => ExecuteCopy(opts),
                (DefineOptions opts) => ExecuteDefine(opts),
                errs => 1);            
        }

        private static int ExecuteDefine(DefineOptions opts)
        {
            DisplayHeader();            
            return 0;
        }

        private static int ExecuteCopy(CopyOptions opts)
        {
            DisplayHeader();            
            return 0;
        }

        static void DisplayHeader()
        {
            Console.WriteLine(HeadingInfo.Default.ToString());
            Console.WriteLine(CopyrightInfo.Default.ToString());
        }
    }
}
