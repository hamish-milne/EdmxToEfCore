namespace EdmxToEfCore
{
	using System.IO;

	public static class ModelToCode
	{
		public static void WriteSingleFile(this Csdl.Schema schema, CSharpCodeWriter codeWriter)
		{
			codeWriter.Namespace(schema.Namespace);

			codeWriter.Using("System");
			codeWriter.Using("System.Collections.Generic");
			codeWriter.Using("System.ComponentModel.DataAnnotations");
			codeWriter.Using("System.ComponentModel.DataAnnotations.Schema");
			codeWriter.Using("Microsoft.EntityFrameworkCore");
			codeWriter.NewLine();

			bool? lazyLoading = null;
			foreach (var container in schema.EntityContainers)
			{
				lazyLoading = container.IsLazyLoadingEnabled();
				container.WriteClass(schema, codeWriter);
				codeWriter.NewLine();
			}
			foreach (var type in schema.EntityTypes)
			{
				type.WriteClass(lazyLoading ?? false, schema, codeWriter);
				codeWriter.NewLine();
			}
			codeWriter.BlockEnd();
		}

		public static void WriteClass(this EntityContainer container, Csdl.Schema schema, CSharpCodeWriter writer)
		{
			writer.Class(container.Name, new []{"DbContext"}, Modifiers.Partial);
			foreach (var set in container.EntitySets)
			{
				writer.AutoProperty(set.Name, $"DbSet<{schema.FindTypeByName(set.EntityType).Name}>", null);
			}
			// TODO: OnModelCreating method
			// TODO: Explicitly include derived types that aren't in an EntitySet
			writer.BlockEnd();
		}

		public static void WriteClass(this EntityType type, bool lazyLoad, Csdl.Schema schema, CSharpCodeWriter writer)
		{
			writer.Class(type.Name,
				type.BaseType == null ? null : new []{schema.FindTypeByName(type.BaseType).Name},
				Modifiers.Partial);
			if (type.Properties != null)
			foreach (var prop in type.Properties)
			{
				if (prop.IsKey(type))
				{
					writer.Attribute("Key");
				}
				var propType = schema.TypeNameToCLRType(prop.Type, out var valueType);
				if (valueType && prop.Nullable) {
					propType += "?";
				} else if (!valueType && !prop.Nullable) {
					writer.Attribute("Required");
				}
				writer.AutoProperty(prop.Name, propType, null);
				writer.NewLine();
			}
			if (type.NavigationProperties != null)
			foreach (var navProp in type.NavigationProperties)
			{
				Association association = schema.GetAssociationByName(navProp.Relationship);
				AssociationEnd toEnd = association.GetEndForRole(navProp.ToRole);
				if (toEnd.Multiplicity != Multiplicity.Many) // Not supported for the Many end to have a foreign key
				{
					string foreignKey = association.GetForeignKey(navProp.FromRole);
					if (foreignKey != null)
					{
						writer.Attribute($"ForeignKey(\"{foreignKey}\")");
					}
				}
				string inverseOf = association.GetInverseProperty(schema, navProp.ToRole);
				if (inverseOf != null)
				{
					writer.Attribute($"InverseProperty(\"{inverseOf}\")");
				}
				var isVirtual = lazyLoad ? Definition.Virtual : Definition.Sealed;
				switch (toEnd.Multiplicity)
				{
					case Multiplicity.One:
						writer.Attribute("Required");
						goto case Multiplicity.ZeroOrOne;
					case Multiplicity.ZeroOrOne:
						writer.AutoProperty(navProp.Name, schema.FindTypeByName(toEnd.Type).Name, null, isVirtual);
						break;
					case Multiplicity.Many:
						// TODO: Support HashSet, Array etc. here
						writer.AutoProperty(navProp.Name, $"List<{schema.FindTypeByName(toEnd.Type).Name}>", null, isVirtual);
						break;
				}
				writer.NewLine();
			}
			writer.BlockEnd();
		}
	}
}