namespace EdmxToEfCore
{
	using System;
	using System.IO;

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
		AbstractClass = (1 << 1),
		Static = (1 << 2),
	}

	public enum MetaType
	{
		Class,
		Struct
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
			if ((modifiers & Modifiers.Static) != 0)
			{
				Stream.Write("static ");
			}
			if ((modifiers & Modifiers.Partial) != 0)
			{
				Stream.Write("partial ");
			}
			if ((modifiers & Modifiers.AbstractClass) != 0)
			{
				Stream.Write("abstract ");
			}
		}

		public void WriteVisibility(Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Private: Stream.Write("private "); break;
				case Visibility.Protected: Stream.Write("protected "); break;
				case Visibility.Internal: Stream.Write("internal "); break;
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

		public void Type(MetaType metaType, string name, string[] inherits, Modifiers modifiers = Modifiers.None, Visibility visibility = Visibility.Public)
		{
			WriteIndents();
			WriteVisibility(visibility);
			WriteModifiers(modifiers);
			switch (metaType)
			{
				case MetaType.Class: Stream.Write("class"); break;
				case MetaType.Struct: Stream.Write("struct"); break;
				default: throw new ArgumentOutOfRangeException();
			}
			Stream.Write(' ');
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
			Visibility getVisibility = Visibility.Public,
			Visibility setVisibility = Visibility.Public)
		{
			WriteIndents();
			var propVisibility = (Visibility)Math.Min((int)getVisibility, (int)setVisibility);
			WriteVisibility(propVisibility);
			WriteDefinition(definition);
			Stream.Write(type);
			Stream.Write(' ');
			Stream.Write(name);

			BlockBegin();
			WriteIndents();
			if (getVisibility > propVisibility)
				WriteVisibility(getVisibility);
			Stream.Write("get");
			getBody(this);
			NewLine();
			WriteIndents();
			if (setVisibility > propVisibility)
				WriteVisibility(setVisibility);
			Stream.Write("set");
			setBody(this);
			BlockEnd();
		}

		public void AutoProperty(string name, string type,
			Action<CSharpCodeWriter> initializer,
			Definition definition = Definition.Sealed,
			Visibility getVisibility = Visibility.Public,
			Visibility setVisibility = Visibility.Public)
		{
			WriteIndents();
			var propVisibility = (Visibility)Math.Min((int)getVisibility, (int)setVisibility);
			WriteVisibility(propVisibility);
			WriteDefinition(definition);
			Stream.Write(type);
			Stream.Write(' ');
			Stream.Write(name);

			Stream.Write(" { ");
			if (getVisibility > propVisibility)
				WriteVisibility(getVisibility);
			Stream.Write("get; ");
			if (setVisibility > propVisibility)
				WriteVisibility(setVisibility);
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

		public void Enum(string name, (string name, int? value)[] members,
			EnumUnderlyingType? type = null, Visibility visibility = Visibility.Public)
		{
			WriteIndents();
			WriteVisibility(visibility);
			Stream.Write(type);
			if (type.HasValue)
			{
				Stream.Write(" : ");
				Stream.Write(type);
			}
			BlockBegin();

			foreach (var member in members)
			{
				WriteIndents();
				Stream.Write(member.name);
				if (member.value.HasValue)
				{
					Stream.Write(" = ");
					Stream.Write(member.value);
				}
				Stream.Write(',');
				NewLine();
			}
			BlockEnd();
		}

		public void Method(string name, string returnType, Definition definition, Modifiers modifiers,
			Visibility visibility, params (string type, string name)[] parameters)
		{
			WriteIndents();
			WriteVisibility(visibility);
			WriteDefinition(definition);
			WriteModifiers(modifiers);
			Stream.Write(returnType ?? "void");
			Stream.Write(' ');
			Stream.Write(name);
			Stream.Write(" (");
			bool first = true;
			foreach (var p in parameters)
			{
				if (!first) {
					Stream.Write(", ");
				}
				first = false;
				Stream.Write(p.type);
				Stream.Write(" ");
				Stream.Write(p.name);
			}
			Stream.Write(")");
			if (definition == Definition.Abstract || (modifiers & Modifiers.Partial) != 0)
			{
				Stream.Write(";");
				NewLine();
			} else {
				BlockBegin();
			}
		}

		public void Statement(string content)
		{
			WriteIndents();
			Stream.Write(content);
			Stream.Write(";");
			NewLine();
		}
	}
}