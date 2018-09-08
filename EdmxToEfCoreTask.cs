namespace EdmxToEfCore
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using System.Xml.Serialization;
	using Configuration = ModelToCode.Configuration;

	public class EdmxToEfCoreTask : Task
	{
		[Required] public ITaskItem[] ModelInputs { get; set; }

		[Output] public string[] OutputFiles { get; set; }

		public void Deserialize(ITaskItem input, object output)
		{
			foreach (var prop in output.GetType().GetProperties())
			{
				var str = input.GetMetadata(prop.Name);
				if (string.IsNullOrWhiteSpace(str)) { continue; }
				var type = prop.PropertyType;
				if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					type = type.GetGenericArguments()[0];
				object obj;
				if (type.IsEnum) {
					obj = Enum.Parse(type, str, true);
				} else {
					obj = Convert.ChangeType(str, type);
				}
				prop.SetValue(output, obj);
			}
		}

		public void ExecuteInstance(ITaskItem item, List<string> outputFiles)
		{
			var configuration = new Configuration();
			Deserialize(item, configuration);
			var inputFile = item.GetMetadata("FullPath");
			ModelToCode.ProcessFile(configuration, inputFile,
				m => BuildEngine.LogMessageEvent(new BuildMessageEventArgs(m, "", "EdmxToEfCore", MessageImportance.Normal)),
				m => BuildEngine.LogWarningEvent(new BuildWarningEventArgs("", "", inputFile, 0, 0, 0, 0, m, "", "EdmxToEfCore")),
				outputFiles.Add
			);
		}

		public override bool Execute()
		{
			var outputFiles = new List<string>();
			foreach (var item in ModelInputs.OrEmpty())
			{
				ExecuteInstance(item, outputFiles);
			}
			OutputFiles = outputFiles.ToArray();
			return true;
		}
	}
}