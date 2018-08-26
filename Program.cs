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

			Edmx edmx;
			Console.WriteLine($"Loading EDMX model at {inFile}...");
			using (var fs = File.Open(inFile, FileMode.Open))
			{
				edmx = (Edmx)(new XmlSerializer(typeof(Edmx))).Deserialize(fs);
			}

			Console.WriteLine($"Writing C# code to {outFile}...");
			using (var codeWriter = new CSharpCodeWriter(outFile))
			{
				edmx.Runtime.ConceptualModels.Schema.WriteSingleFile(codeWriter);
			}

			Console.WriteLine("Success!");
			return 0;
		}
	}
}
