namespace EdmxToEfCore
{
	using System;
	using System.Linq;

	public enum EdmSimpleType
	{
		Binary,
		Boolean,
		Byte,
		DateTime,
		DateTimeOffset,
		Decimal,
		Double,
		Float,
		Guid,
		Int16,
		Int32,
		Int64,
		SByte,
		String,
		Time
	}

	public static class ModelTraversal
	{
		public static EntityType FindTypeByName(this Csdl.Schema schema, string qualifiedName)
		{
			var namespacePrefix = schema.Namespace + '.';
			var localName = qualifiedName.Substring(namespacePrefix.Length);
			return schema.EntityTypes.SingleOrDefault(t => t.Name == localName);
		}

		public static string TypeNameToCLRType(this Csdl.Schema schema, string typeName, out bool valueType)
		{
			object edmSimpleType;
			if (Enum.TryParse(typeof(EdmSimpleType), typeName, out edmSimpleType))
			{
				valueType = true;
				switch ((EdmSimpleType)edmSimpleType)
				{
					case EdmSimpleType.Binary: valueType = false; return "byte[]";
					case EdmSimpleType.Boolean: return "bool";
					case EdmSimpleType.Byte: return "byte";
					case EdmSimpleType.DateTime: return nameof(DateTime);
					case EdmSimpleType.DateTimeOffset: return nameof(DateTimeOffset);
					case EdmSimpleType.Decimal: return "decimal";
					case EdmSimpleType.Double: return "double";
					case EdmSimpleType.Float: return "float";
					case EdmSimpleType.Guid: return nameof(Guid);
					case EdmSimpleType.Int16: return "short";
					case EdmSimpleType.Int32: return "int";
					case EdmSimpleType.Int64: return "long";
					case EdmSimpleType.SByte: return "sbyte";
					case EdmSimpleType.String: valueType = false; return "string";
					case EdmSimpleType.Time: return nameof(DateTime);
					default: throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				valueType = false;
				return schema.FindTypeByName(typeName).Name;
			}
		}

		public static Association GetAssociationByName(this Csdl.Schema schema, string qualifiedName)
		{
			var namespacePrefix = schema.Namespace + '.';
			var localName = qualifiedName.Substring(namespacePrefix.Length);
			return schema.Associations.SingleOrDefault(t => t.Name == localName);
		}

		public static AssociationEnd GetEndForRole(this Association assoc, string roleName)
		{
			return assoc.Ends.First(e => e.Role == roleName);
		}

		public static string GetForeignKey(this Association assoc, string fromRole)
		{
			if (assoc.ReferentialConstraint == null) { return null; }
			return new []{assoc.ReferentialConstraint.Principal, assoc.ReferentialConstraint.Dependent}
				.First(p => p.Role == fromRole)
				.PropertyRefs.SingleOrDefault()?.Name; // TODO: Support multiple foreign keys
		}

		public static string GetInverseProperty(this Association assoc, Csdl.Schema schema, string toRole)
		{
			var otherEnd = assoc.Ends.Single(e => e.Role == toRole);
			var otherType = schema.FindTypeByName(otherEnd.Type);
			var fromRole = assoc.Ends.Single(e => e != otherEnd).Role;
			var otherProperty = otherType.NavigationProperties.SingleOrDefault(p => p.FromRole == toRole && p.ToRole == fromRole);
			return otherProperty == null ? null : (otherType.Name + "." + otherProperty.Name);
		}

		public static bool IsKey(this Property property, EntityType parentType)
		{
			if (parentType.Key?.KeyProperties == null) { return false; }
			return parentType.Key.KeyProperties.Any(p => p.Name == property.Name);
		}

	}
}