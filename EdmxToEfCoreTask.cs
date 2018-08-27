namespace EdmxToEfCore
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class EdmxToEfCoreTask : Task
	{
		public ITaskItem[] InputModels { get; set; }
		public bool AutoIncludeFromProject { get; set; }
		public string OutputPathPattern { get; set; }

		public override bool Execute()
		{
			return false;
		}
	}
}