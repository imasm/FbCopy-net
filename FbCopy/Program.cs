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
            DefineService defineService = new DefineService(opts);
            defineService.Run();

            return 0;
        }

        private static int ExecuteCopy(CopyOptions opts)
        {
            DisplayHeader();
            CopyService copyService = new CopyService(opts);
            copyService.Run(Console.In);
            return 0;
        }

        static void DisplayHeader()
        {
            Console.Error.WriteLine(HeadingInfo.Default.ToString());
            Console.Error.WriteLine(CopyrightInfo.Default.ToString());
        }
               
    }
}
