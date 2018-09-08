namespace EdmxToEfCore
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;

	public static class ModelToCode
	{
		public enum CollectionType
		{
			Array,
			List,
			HashSet,
		}

		public enum FileMode
		{
			SingleFile,
			PerClass
		}

		public class Configuration
		{
			public CollectionType CollectionType { get; set; }
			public bool? LazyLoading { get; set; }
			public FileMode FileMode { get; set; }
			public MetaType ComplexMetaType { get; set; }
		}

		public static T[] OrEmpty<T>(this T[] array) => array ?? Array.Empty<T>();

		public static void ProcessFile(Configuration config, string inFile, string outPattern, Action<string> log)
		{
			Edmx edmx;
			log($"Loading EDMX model at {inFile}...");
			using (var fs = File.OpenRead(inFile))
			{
				edmx = (Edmx)(new XmlSerializer(typeof(Edmx))).Deserialize(fs);
			}

			switch (config.FileMode)
			{
				case FileMode.SingleFile:
					var outFile = string.Format(outPattern, Path.GetFileNameWithoutExtension(inFile));
					log($"Writing C# code to {outFile}...");
					using (var codeWriter = new CSharpCodeWriter(outFile))
					{
						edmx.Runtime.ConceptualModels.Schema.WriteSingleFile(config, codeWriter);
					}
					break;
				case FileMode.PerClass:
					log($"Writing C# code to multiple files in {outPattern}...");
					edmx.Runtime.ConceptualModels.Schema.WriteMultipleFiles(config,
						s => string.Format(outPattern, s));
					break;
			}

			log("Success!");
		}

		public enum NamespaceFor
		{
			None = 0,
			PrimitiveTypes = (1 << 0),
			CollectionTypes = (1 << 1),
			Annotations = (1 << 2),
			DbContext = (1 << 3),

			Enum = None,
			ComplexType = PrimitiveTypes | Annotations,
			EntityType = PrimitiveTypes | CollectionTypes | Annotations,
			EntityContainer = PrimitiveTypes | CollectionTypes | Annotations | DbContext,
		}

		public static void BeginNamespace(this Csdl.Schema schema, CSharpCodeWriter codeWriter, NamespaceFor nsFor)
		{
			codeWriter.Namespace(schema.Namespace);

			if ((nsFor & NamespaceFor.PrimitiveTypes) != 0) {
				codeWriter.Using("System");
			}
			if ((nsFor & NamespaceFor.CollectionTypes) != 0) {
				codeWriter.Using("System.Collections.Generic");
			}
			if ((nsFor & NamespaceFor.Annotations) != 0) {
				codeWriter.Using("System.ComponentModel.DataAnnotations");
				codeWriter.Using("System.ComponentModel.DataAnnotations.Schema");
			}
			if ((nsFor & NamespaceFor.DbContext) != 0) {
				codeWriter.Using("Microsoft.EntityFrameworkCore");
			}
			codeWriter.NewLine();
		}

		public static void WriteEnum(this EnumType type, CSharpCodeWriter codeWriter)
		{
			codeWriter.Enum(type.Name,
				type.Members.Select(m => (m.Name, m.Value)).ToArray(),
				type.UnderlyingType);
		}

		public static void WriteComplexType(this ComplexType type, Csdl.Schema schema, Configuration config, CSharpCodeWriter codeWriter)
		{

			codeWriter.Type(config.ComplexMetaType, type.Name, null, Modifiers.Partial);
			foreach (var prop in type.Properties.OrEmpty())
			{
				prop.WriteOut(null, schema, codeWriter);
				codeWriter.NewLine();
			}
			codeWriter.BlockEnd();
		}

		public static void WriteEntityContainer(this EntityContainer container, Csdl.Schema schema, Configuration config, CSharpCodeWriter codeWriter)
		{
			config.LazyLoading = config.LazyLoading ?? container.LazyLoadingEnabled;
			container.WriteClass(schema, codeWriter);
		}

		private static readonly (
			Func<Csdl.Schema, NamedElement[]> elements,
			NamespaceFor nsFor,
			Action<NamedElement, Csdl.Schema, Configuration, CSharpCodeWriter> writeOut)[] ElementsToWrite =
		{
			(s => s.EnumTypes, NamespaceFor.Enum,
				(e, schema, config, codeWriter) => ((EnumType)e).WriteEnum(codeWriter)),
			(s => s.ComplexTypes, NamespaceFor.ComplexType,
				(e, schema, config, codeWriter) => ((ComplexType)e).WriteComplexType(schema, config, codeWriter)),
			(s => s.EntityContainers, NamespaceFor.EntityContainer,
				(e, schema, config, codeWriter) => ((EntityContainer)e).WriteEntityContainer(schema, config, codeWriter)),
			(s => s.EntityTypes, NamespaceFor.EntityType,
				(e, schema, config, codeWriter) => ((EntityType)e).WriteClass(config, schema, codeWriter)),
		};

		public static void WriteSingleFile(this Csdl.Schema schema, Configuration config, CSharpCodeWriter codeWriter)
		{
			schema.BeginNamespace(codeWriter, NamespaceFor.EntityContainer);
			foreach (var t in ElementsToWrite)
			{
				foreach (var e in t.elements(schema).OrEmpty())
				{
					t.writeOut(e, schema, config, codeWriter);
					codeWriter.NewLine();
				}
			}
			codeWriter.BlockEnd();
		}

		public static void WriteMultipleFiles(this Csdl.Schema schema, Configuration config, Func<string, string> nameToPath)
		{
			foreach (var t in ElementsToWrite)
			{
				foreach (var e in t.elements(schema).OrEmpty())
				{
					using (var codeWriter = new CSharpCodeWriter(nameToPath(e.Name)))
					{
						schema.BeginNamespace(codeWriter, t.nsFor);
						t.writeOut(e, schema, config, codeWriter);
						codeWriter.BlockEnd();
					}
				}
			}
		}

		public static void WriteDocumentation(this HasDocumentation obj, CSharpCodeWriter writer)
		{
			if (obj.Documentation != null)
			{
				writer.WriteDocElement("summary", obj.Documentation.Summary);
				writer.WriteDocElement("remarks", obj.Documentation.LongDescription);
			}
		}

		public static void WriteClass(this EntityContainer container, Csdl.Schema schema, CSharpCodeWriter writer)
		{
			writer.Type(MetaType.Class, container.Name, new []{"DbContext"}, Modifiers.Partial);
			foreach (var set in container.EntitySets.OrEmpty())
			{
				set.WriteDocumentation(writer);
				writer.AutoProperty(set.Name, $"DbSet<{schema.FindTypeByName(set.EntityType).Name}>", null);
				writer.NewLine();
			}
			// TODO: OnModelCreating method
			// TODO: Explicitly include derived types that aren't in an EntitySet
			writer.BlockEnd();
		}

		public static void WriteOut(this Property prop, EntityType parent, Csdl.Schema schema, CSharpCodeWriter writer)
		{
			prop.WriteDocumentation(writer);
			if (parent != null && prop.IsKey(parent))
			{
				writer.Attribute("Key");
			}
			if (prop.ConcurrencyMode == ConcurrencyMode.Fixed)
			{
				writer.Attribute("ConcurrencyCheck");
			}
			if (prop.MaxLength > 0)
			{
				writer.Attribute("MaxLength", prop.MaxLength.ToString());
			}
			var propType = schema.TypeNameToCLRType(prop.Type, out var valueType);
			if (valueType && prop.Nullable) {
				propType += "?";
			} else if (!valueType && !prop.Nullable) {
				writer.Attribute("Required");
			}
			writer.AutoProperty(prop.Name, propType, null, Definition.Sealed,
				prop.GetterAccess, prop.SetterAccess);
		}

		public static void WriteClass(this EntityType type, Configuration config, Csdl.Schema schema, CSharpCodeWriter writer)
		{
			type.WriteDocumentation(writer);
			var classMods = Modifiers.Partial;
			if (type.Abstract)
			{
				classMods |= Modifiers.AbstractClass;
			}
			writer.Type(MetaType.Class, type.Name,
				type.BaseType == null ? null : new []{schema.FindTypeByName(type.BaseType).Name},
				classMods, type.TypeAccess);
			if (type.Properties != null)
			foreach (var prop in type.Properties)
			{
				prop.WriteOut(type, schema, writer);
				writer.NewLine();
			}

			foreach (var navProp in type.NavigationProperties.OrEmpty())
			{
				navProp.WriteDocumentation(writer);
				Association association = schema.GetAssociationByName(navProp.Relationship);
				AssociationEnd toEnd = association.GetEndForRole(navProp.ToRole);
				if (toEnd.Multiplicity != Multiplicity.Many) // Not supported for the Many end to have a foreign key
				{
					string foreignKey = association.GetForeignKey(navProp.FromRole);
					if (foreignKey != null)
					{
						writer.Attribute("ForeignKey", writer.NameOf(foreignKey));
					}
				}
				string inverseOf = association.GetInverseProperty(schema, navProp.ToRole);
				if (inverseOf != null)
				{
					writer.Attribute("InverseProperty", writer.NameOf(schema.Namespace + "." + inverseOf));
				}
				var isVirtual = (config.LazyLoading ?? false) ? Definition.Virtual : Definition.Sealed;
				switch (toEnd.Multiplicity)
				{
					case Multiplicity.One:
						writer.Attribute("Required");
						goto case Multiplicity.ZeroOrOne;
					case Multiplicity.ZeroOrOne:
						writer.AutoProperty(navProp.Name, schema.FindTypeByName(toEnd.Type).Name, null, isVirtual,
							navProp.GetterAccess, navProp.SetterAccess);
						break;
					case Multiplicity.Many:
						var typeName = schema.FindTypeByName(toEnd.Type).Name;
						switch (config.CollectionType)
						{
							case CollectionType.Array:
								typeName = typeName + "[]";
								break;
							case CollectionType.List:
								typeName = $"List<{typeName}>";
								break;
							case CollectionType.HashSet:
								typeName = $"HashSet<{typeName}>";
								break;
							default: throw new ArgumentOutOfRangeException(nameof(CollectionType));
						}
						writer.AutoProperty(navProp.Name, typeName, null, isVirtual,
							navProp.GetterAccess, navProp.SetterAccess);
						break;
				}
				writer.NewLine();
			}
			writer.BlockEnd();
		}
	}
}