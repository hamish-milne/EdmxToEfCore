namespace EdmxToEfCore
{
	using System;
	using System.IO;

	public enum Visibility
	{
		Public,
		Protected,
		Private
	}

	public enum Definition
	{
		Sealed,
		Virtual,
		Override,
		Abstract
	}

	[Flags]
	public enum Modifiers
	{
		None = 0,
		Partial = (1 << 0),
	}

	public class CSharpCodeWriter : IDisposable
	{
		public StreamWriter Stream { get; }

		public int IndentLevel { get; private set; }

		public CSharpCodeWriter(string path)
		{
			Stream = new StreamWriter(path);
		}

		public void Dispose()
		{
			Stream.Dispose();
		}

		public void Indent()
		{
			IndentLevel++;
		}

		public void DeIndent()
		{
			IndentLevel--;
		}

		public void NewLine()
		{
			Stream.WriteLine();
		}

		public void WriteIndents()
		{
			for (int i = 0; i < IndentLevel; i++)
			{
				Stream.Write('\t');
			}
		}

		private void BlockBegin()
		{
			NewLine();
			WriteIndents();
			Stream.Write('{');
			NewLine();
			Indent();
		}

		public void BlockEnd()
		{
			DeIndent();
			WriteIndents();
			Stream.Write('}');
			NewLine();
		}

		public void Namespace(string name)
		{
			WriteIndents();
			Stream.Write("namespace ");
			Stream.Write(name);
			BlockBegin();
		}

		public string NameOf(string element) => $"nameof({element})";

		public void Attribute(string content, params string[] args)
		{
			WriteIndents();
			Stream.Write('[');
			Stream.Write(content);
			var argList = string.Join(", ", args);
			if (!string.IsNullOrEmpty(argList))
			{
				Stream.Write('(');
				Stream.Write(argList);
				Stream.Write(')');
			}
			Stream.Write(']');
			NewLine();
		}

		public void Using(string ns)
		{
			WriteIndents();
			Stream.Write("using ");
			Stream.Write(ns);
			Stream.Write(';');
			NewLine();
		}

		public void WriteModifiers(Modifiers modifiers)
		{
			if ((modifiers & Modifiers.Partial) != 0)
			{
				Stream.Write("partial ");
			}
		}

		public void WriteVisibility(Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Private: Stream.Write("private "); break;
				case Visibility.Protected: Stream.Write("protected "); break;
				case Visibility.Public: Stream.Write("public "); break;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public void WriteDefinition(Definition definition)
		{
			switch (definition)
			{
				case Definition.Sealed: break;
				case Definition.Virtual: Stream.Write("virtual "); break;
				case Definition.Override: Stream.Write("override "); break;
				case Definition.Abstract: Stream.Write("abstract "); break;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public void Class(string name, string[] inherits, Modifiers modifiers = Modifiers.None, Visibility visibility = Visibility.Public)
		{
			WriteIndents();
			WriteVisibility(visibility);
			WriteModifiers(modifiers);
			Stream.Write("class ");
			Stream.Write(name);
			if (inherits != null && inherits.Length > 0)
			{
				Stream.Write(" : ");
				Stream.Write(inherits[0]);
				for (int i = 1; i < inherits.Length; i++)
				{
					Stream.Write(", ");
					Stream.Write(inherits[i]);
				}
			}
			BlockBegin();
		}

		public void Property(string name, string type,
			Action<CSharpCodeWriter> getBody,
			Action<CSharpCodeWriter> setBody,
			Definition definition = Definition.Sealed,
			Visibility visibility = Visibility.Public,
			Visibility? getVisibility = null,
			Visibility? setVisibility = null)
		{
			WriteIndents();
			WriteVisibility(visibility);
			WriteDefinition(definition);
			Stream.Write(type);
			Stream.Write(' ');
			Stream.Write(name);

			BlockBegin();
			WriteIndents();
			if (getVisibility.HasValue)
				WriteVisibility(getVisibility.Value);
			Stream.Write("get");
			getBody(this);
			NewLine();
			WriteIndents();
			if (setVisibility.HasValue)
				WriteVisibility(setVisibility.Value);
			Stream.Write("set");
			setBody(this);
			BlockEnd();
		}

		public void AutoProperty(string name, string type,
			Action<CSharpCodeWriter> initializer,
			Definition definition = Definition.Sealed,
			Visibility visibility = Visibility.Public,
			Visibility? getVisibility = null,
			Visibility? setVisibility = null)
		{
			WriteIndents();
			WriteVisibility(visibility);
			WriteDefinition(definition);
			Stream.Write(type);
			Stream.Write(' ');
			Stream.Write(name);

			Stream.Write(" {");
			if (getVisibility.HasValue)
				WriteVisibility(getVisibility.Value);
			Stream.Write(" get; ");
			if (setVisibility.HasValue)
				WriteVisibility(setVisibility.Value);
			Stream.Write("set; }");
			if (initializer != null)
			{
				Stream.Write(" = ");
				initializer(this);
				Stream.Write(';');
			}
			NewLine();
		}

		public void WriteDocElement(string element, string text)
		{
			if (string.IsNullOrWhiteSpace(text)) { return; }
			WriteIndents();
			Stream.WriteLine($"/// <{element}>");
			WriteIndents();
			Stream.WriteLine($"/// {text}");
			WriteIndents();
			Stream.WriteLine($"/// </{element}>");
		}
	}
}