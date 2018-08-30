namespace EdmxToEfCore
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;

	public static class ModelToCode
	{
		public static void ProcessFile(string inFile, string outFile, Action<string> log)
		{
			Edmx edmx;
			log($"Loading EDMX model at {inFile}...");
			using (var fs = File.Open(inFile, FileMode.Open))
			{
				edmx = (Edmx)(new XmlSerializer(typeof(Edmx))).Deserialize(fs);
			}

			log($"Writing C# code to {outFile}...");
			using (var codeWriter = new CSharpCodeWriter(outFile))
			{
				edmx.Runtime.ConceptualModels.Schema.WriteSingleFile(codeWriter);
			}

			log("Success!");
		}

		public static void WriteSingleFile(this Csdl.Schema schema, CSharpCodeWriter codeWriter)
		{
			codeWriter.Namespace(schema.Namespace);

			codeWriter.Using("System");
			codeWriter.Using("System.Collections.Generic");
			codeWriter.Using("System.ComponentModel.DataAnnotations");
			codeWriter.Using("System.ComponentModel.DataAnnotations.Schema");
			codeWriter.Using("Microsoft.EntityFrameworkCore");
			codeWriter.NewLine();

			if (schema.EnumTypes != null)
			foreach (var type in schema.EnumTypes)
			{
				codeWriter.Enum(type.Name,
					type.Members.Select(m => (m.Name, m.Value)).ToArray(),
					type.UnderlyingType);
			}

			if (schema.ComplexTypes != null)
			foreach (var type in schema.ComplexTypes)
			{
				// TODO: Config for class or struct
				codeWriter.Type(MetaType.Class, type.Name, null, Modifiers.Partial);
				if (type.Properties != null)
				foreach (var prop in type.Properties)
				{
					prop.WriteOut(null, schema, codeWriter);
					codeWriter.NewLine();
				}
				codeWriter.BlockEnd();
				codeWriter.NewLine();
			}

			bool? lazyLoading = null;
			if (schema.EntityContainers != null)
			foreach (var container in schema.EntityContainers)
			{
				lazyLoading = container.LazyLoadingEnabled;
				container.WriteClass(schema, codeWriter);
				codeWriter.NewLine();
			}

			if (schema.EntityTypes != null)
			foreach (var type in schema.EntityTypes)
			{
				type.WriteClass(lazyLoading ?? false, schema, codeWriter);
				codeWriter.NewLine();
			}
			codeWriter.BlockEnd();
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
			if (container.EntitySets != null)
			foreach (var set in container.EntitySets)
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

		public static void WriteClass(this EntityType type, bool lazyLoad, Csdl.Schema schema, CSharpCodeWriter writer)
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
			if (type.NavigationProperties != null)
			foreach (var navProp in type.NavigationProperties)
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
					writer.Attribute("InverseProperty", writer.NameOf(inverseOf));
				}
				var isVirtual = lazyLoad ? Definition.Virtual : Definition.Sealed;
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
						// TODO: Support HashSet, Array etc. here
						writer.AutoProperty(navProp.Name, $"List<{schema.FindTypeByName(toEnd.Type).Name}>", null, isVirtual,
							navProp.GetterAccess, navProp.SetterAccess);
						break;
				}
				writer.NewLine();
			}
			writer.BlockEnd();
		}
	}
}