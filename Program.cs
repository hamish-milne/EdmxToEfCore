namespace EdmxToEfCore
{
	using System;
	using System.Xml;
	using System.Xml.Serialization;
	using System.IO;

	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length != 2)
			{
				var exeName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
				Console.WriteLine(
$@"EDMX to EF Core code generator
(C) Hamish Milne 2018
Usage:
        {exeName} [input .edmx model] [output .cs file]
"
				);
				return 1;
			}
			var inFile = args[0];
			var outFile = args[1];
			ModelToCode.ProcessFile(inFile, outFile, Console.WriteLine);
			return 0;
		}
	}
}
