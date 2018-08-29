namespace EdmxToEfCore
{
	/* This file was compiled from observation of generated models and the documentation found at
	 * https://docs.microsoft.com/en-us/ef/ef6/modeling/designer/advanced/edmx/csdl-spec
	 */

	using System.Xml;
	using System.Xml.Serialization;

	public static class Xmlns
	{
		public const string Edmx = "http://schemas.microsoft.com/ado/2009/11/edmx";
		public const string Ssdl = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
		public const string Edm = "http://schemas.microsoft.com/ado/2009/11/edm";
		public const string Cg = "http://schemas.microsoft.com/ado/2006/04/codegeneration";
		public const string Store = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";
		public const string Annotation = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";
		public const string Cs = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";

		public static XmlSerializerNamespaces MakeNamespaces()
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add("edmx", Xmlns.Edmx);
			ns.Add("cg", Xmlns.Cg);
			ns.Add("store", Xmlns.Store);
			ns.Add("annotation", Xmlns.Annotation);
			return ns;
		}
	}

	[XmlRoot(Namespace = Xmlns.Edmx)]
	public class Edmx
	{
		[XmlAttribute] public string Version { get; set; } = "3.0";
		public Runtime Runtime { get; set; }
	}

	[XmlType(Namespace = Xmlns.Edmx)]
	public class Runtime
	{
		public StorageModels StorageModels { get; set; }
		public ConceptualModels ConceptualModels { get; set; }
		public Mappings Mappings { get; set; }
	}

	[XmlType(Namespace = Xmlns.Edmx)]
	public class StorageModels
	{
		[XmlElement(Namespace = Xmlns.Ssdl)]
		public Ssdl.Schema Schema { get; set; }
	}

	[XmlType(Namespace = Xmlns.Edmx)]
	public class ConceptualModels
	{
		[XmlElement(Namespace = Xmlns.Edm)]
		public Csdl.Schema Schema { get; set; }
	}

	[XmlType(Namespace = Xmlns.Edmx)]
	public class Mappings
	{
		[XmlElement(Namespace = Xmlns.Cs)]
		public Msl.Mapping[] Mapping { get; set; }
	}

	public class HasAnnotationAttributes
	{
		[XmlAnyAttribute] public XmlAttribute[] AnnotationAttributes { get; set; }
	}

	public class Documentation : HasAnnotations
	{
		public string Summary { get; set; }

		public string LongDescription { get; set; }
	}

	public class HasAnnotations : HasAnnotationAttributes
	{
		[XmlAnyElement] public XmlElement[] Annotations { get; set; }
	}

	public class HasDocumentation : HasAnnotations
	{
		public Documentation Documentation { get; set; }
	}

	public class NamedElement : HasDocumentation
	{
		[XmlAttribute] public string Name { get; set; }
	}

	public class Association : NamedElement
	{
		[XmlElement("End")]
		public AssociationEnd[] Ends { get; set; }

		public ReferentialConstraint ReferentialConstraint { get; set; }
	}

	public class AssociationSet : NamedElement
	{
		[XmlElement("End")]
		public AssociationSetEnd[] Ends { get; set; }

		[XmlAttribute] public string Association { get; set; }
	}

	public enum Multiplicity
	{
		[XmlEnum("1")]
		One,
		[XmlEnum("0..1")]
		ZeroOrOne,
		[XmlEnum("*")]
		Many
	}

	
	
	public enum OnDeleteAction
	{
		None,
		Restricted,
		Cascade
	}

	public class OnDelete : HasDocumentation
	{
		[XmlAttribute] public OnDeleteAction Action { get; set; }
	}

	public class AssociationEnd : HasDocumentation
	{
		[XmlAttribute] public string Type { get; set; }

		[XmlAttribute] public string Role { get; set; }

		[XmlAttribute] public Multiplicity Multiplicity { get; set; }

		public OnDelete OnDelete { get; set; }
	}

	public class AssociationSetEnd : HasDocumentation
	{
		[XmlAttribute] public string EntitySet { get; set; }

		[XmlAttribute] public string Role { get; set; }
	}

	
	public class PropertyRef : NamedElement
	{
	}

	public class ConstraintElement : HasAnnotations
	{
		[XmlAttribute] public string Role { get; set; }

		[XmlElement("PropertyRef")]
		public PropertyRef[] PropertyRefs { get; set; }
	}

	public class ReferentialConstraint : HasDocumentation
	{
		public ConstraintElement Principal { get; set; }
		public ConstraintElement Dependent { get; set; }
	}

	public class Key : HasAnnotations
	{
		[XmlElement("PropertyRef")]
		public PropertyRef[] KeyProperties { get; set; }
	}


	public enum StoreGeneratedPattern
	{
		None,
		Identity,
		Computed
	}

	public enum ConcurrencyMode
	{
		None,
		Fixed
	}

	public abstract class TypeBase : HasDocumentation
	{
		[XmlAttribute] public string Type { get; set; }
		[XmlAttribute] public bool Nullable { get; set; } = true;
		[XmlAttribute] public string DefaultValue { get; set; }
		[XmlAttribute] public int MaxLength { get; set; }
		[XmlAttribute] public int FixedLength { get; set; }
		[XmlAttribute] public int Precision { get; set; }
		[XmlAttribute] public int Scale { get; set; }
		[XmlAttribute] public bool Unicode { get; set; }
		[XmlAttribute] public string Collation { get; set; }
		[XmlAttribute] public string SRID { get; set; }
	}

	public enum Visibility
	{
		Public,
		Internal,
		Protected,
		Private
	}

	public class Property : TypeBase
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute(Namespace = Xmlns.Annotation)]
		public StoreGeneratedPattern StoreGeneratedPattern { get; set; }

		[XmlAttribute]
		public ConcurrencyMode ConcurrencyMode { get; set; }

		[XmlAttribute(Namespace = Xmlns.Cg)]
		public Visibility GetterAccess { get; set; }

		[XmlAttribute(Namespace = Xmlns.Cg)]
		public Visibility SetterAccess { get; set; }
	}

	public class TypeRef : TypeBase
	{

	}

	public class NavigationProperty : NamedElement
	{
		[XmlAttribute] public string Relationship { get; set; }
		[XmlAttribute] public string ToRole { get; set; }
		[XmlAttribute] public string FromRole { get; set; }

		[XmlAttribute(Namespace = Xmlns.Cg)]
		public Visibility GetterAccess { get; set; }

		[XmlAttribute(Namespace = Xmlns.Cg)]
		public Visibility SetterAccess { get; set; }
	}

	public class EntityType : NamedElement
	{
		[XmlAttribute] public string BaseType { get; set; }
		[XmlAttribute] public bool Abstract { get; set; }
		[XmlAttribute] public bool OpenType { get; set; }

		public Key Key { get; set; }
		[XmlElement("Property")] public Property[] Properties { get; set; }
		[XmlElement("NavigationProperty")] public NavigationProperty[] NavigationProperties { get; set; }

		[XmlAttribute(Namespace = Xmlns.Cg)]
		public Visibility TypeAccess { get; set; }
	}

	public class Member : HasAnnotationAttributes
	{
		[XmlAttribute] public string Name { get; set; }
		[XmlIgnore] public int? Value { get; set; }
		[XmlAttribute("Value")] public int RawValue
		{
			get => Value ?? 0;
			set => Value = value;
		}
		public bool ValueSpecified => Value.HasValue;
	}

	public enum EnumUnderlyingType
	{
		Byte,
		SByte,
		Int16,
		Int32,
		Int64
	}

	public class EnumType : NamedElement
	{
		[XmlAttribute] public bool IsFlags { get; set; }
		[XmlAttribute] public EnumUnderlyingType UnderlyingType { get; set; } = EnumUnderlyingType.Int32;
		[XmlElement("Member")] public Member[] Members { get; set; }
	}

	public enum ParameterMode
	{
		In,
		Out,
		InOut
	}

	public class Parameter : HasAnnotations
	{
		[XmlAttribute] public string Name { get; set; }
		[XmlAttribute] public string Type { get; set; }
		[XmlAttribute] public ParameterMode Mode { get; set; }
		[XmlAttribute] public int MaxLength { get; set; }
		[XmlAttribute] public int Precision { get; set; }
		[XmlAttribute] public int Scale { get; set; }
		[XmlAttribute] public string SRID { get; set; }
	}

	public class RowType : HasAnnotations
	{
		[XmlElement("Property")]
		public Property[] Properties { get; set; }
	}

	public class ReferenceType : HasDocumentation
	{
		public string Type { get; set; }
	}

	public class CollectionType : HasAnnotations
	{
		public RowType RowType { get; set; }
		public ReferenceType ReferenceType { get; set; }
		[XmlElement("CollectionType")]
		public CollectionType CollectionTypeElement { get; set; }
		public TypeRef TypeRef { get; set; }
	}

	public class ReturnType : HasAnnotations
	{
		public CollectionType CollectionType { get; set; }
		public ReferenceType ReferenceType { get; set; }
		public RowType RowType { get; set; }
	}

	public class ComplexType : NamedElement
	{
		[XmlAttribute] public string BaseType { get; set; }
		[XmlAttribute] public bool Abstract { get; set; }

		[XmlElement("Property")] public Property[] Properties { get; set; }
	}

	
	public class EntityContainer : NamedElement
	{
		[XmlElement("EntitySet")] public EntitySet[] EntitySets { get; set; }
		[XmlElement("AssociationSet")] public AssociationSet[] AssociationSets { get; set; }
		[XmlElement("FunctionImport")] public FunctionImport[] FunctionImports { get; set; }

		[XmlAttribute(Namespace = Xmlns.Annotation)]
		public bool LazyLoadingEnabled { get; set; }
	}

	public class EntitySet : NamedElement
	{
		[XmlAttribute] public string EntityType { get; set; }
		[XmlAttribute] public string Schema { get; set; }
		[XmlAttribute] public string Table { get; set; }

		public string DefiningQuery { get; set; }
	}

	public class ImportReturnType : HasAnnotationAttributes
	{
		[XmlAttribute] public string Type { get; set; }
		[XmlAttribute] public string EntitySet { get; set; }
	}

	public class FunctionImport : NamedElement
	{
		[XmlAttribute] public string ReturnType { get; set; }
		[XmlAttribute] public string EntitySet { get; set; }
		[XmlAttribute] public bool IsComposable { get; set; }

		public ImportReturnType ReturnTypeElement { get; set; }

	}

	public class Using : HasDocumentation
	{
		[XmlAttribute] public string Namespace { get; set; }
		[XmlAttribute] public string Alias { get; set; }
	}

	namespace Csdl
	{
		public class Function : NamedElement
		{
			[XmlAttribute] public string ReturnType { get; set; }

			[XmlElement("Parameter")] public Parameter[] Parameters { get; set; }
			public string DefiningExpression { get; set; }
			[XmlElement("ReturnType")] public ReturnType ReturnTypeElement { get; set; }
		}

		public class Schema : HasAnnotations
		{
			[XmlAttribute] public string Namespace { get; set; }
			[XmlAttribute] public string Alias { get; set; }

			[XmlElement("Using")] public Using[] Usings { get; set; }
			[XmlElement("EntityContainer")] public EntityContainer[] EntityContainers { get; set; }
			[XmlElement("EntityType")] public EntityType[] EntityTypes { get; set; }
			[XmlElement("EnumType")] public EnumType[] EnumTypes { get; set; }
			[XmlElement("Association")] public Association[] Associations { get; set; }
			[XmlElement("ComplexType")] public ComplexType[] ComplexTypes { get; set; }
			[XmlElement("Function")] public Function[] Functions { get; set; }
		}
	}


	namespace Ssdl
	{
		public class Function : NamedElement
		{
			[XmlAttribute] public string ReturnType { get; set; }
			[XmlAttribute] public bool Aggregate { get; set; }
			[XmlAttribute] public bool BuiltIn { get; set; }
			[XmlAttribute] public string StoreFunctionName { get; set; }
			[XmlAttribute] public bool NiladicFunction { get; set; }
			[XmlAttribute] public bool IsComposable { get; set; }
			[XmlAttribute] public string ParameterTypeSemantics { get; set; } = "AllowImplicitConversion";
			[XmlAttribute] public string Schema { get; set; }

			[XmlElement("Parameter")]
			public Parameter[] Parameters { get; set; }
			public string CommandText { get; set; }
			[XmlElement("ReturnType")]
			public ReturnType ReturnTypeElement { get; set; }

		}

		public class Schema
		{
			[XmlAttribute] public string Namespace { get; set; }

			[XmlAttribute] public string Alias { get; set; }

			[XmlAttribute] public string Provider { get; set; }

			[XmlAttribute] public string ProviderManifestToken { get; set; }

			[XmlElement("EntityContainer")]
			public EntityContainer[] EntityContainers { get; set; }

			[XmlElement("Association")]
			public Association[] Associations { get; set; }

			[XmlElement("EntityType")]
			public EntityType[] EntityTypes { get; set; }

			[XmlElement("Function")]
			public Function[] Functions { get; set; }
		}
	}

	namespace Msl
	{
		public class Alias
		{
			[XmlAttribute] public string Key { get; set; }
			[XmlAttribute] public string Value { get; set; }
		}

		public class AssociationEnd
		{
			[XmlAttribute] public string AssociationSet { get; set; }
			[XmlAttribute] public string From { get; set; }
			[XmlAttribute] public string To { get; set; }

			public ScalarProperty ScalarProperty { get; set; }
		}

		public class AssociationSetMapping
		{
			[XmlAttribute] public string Name { get; set; }
			[XmlAttribute] public string TypeName { get; set; }
			[XmlAttribute] public string StoreEntitySet { get; set; }

			public QueryView QueryView { get; set; }
			[XmlElement("EndProperty")] public EndProperty[] EndProperties { get; set; }
			[XmlElement("Condition")] public Condition[] Conditions { get; set; }
			public ModificationFunctionMapping ModificationFunctionMapping { get; set; }
		}

		public class ComplexProperty
		{
			[XmlAttribute] public string Name { get; set; }
			[XmlAttribute] public string TypeName { get; set; }

			[XmlElement("ScalarProperty")] public ScalarProperty[] ScalarProperties { get; set; }
		}

		public class ComplexTypeMapping
		{
			[XmlAttribute] public string TypeName { get; set; }

			[XmlElement("ScalarProperty")] public ScalarProperty[] ScalarProperties { get; set; }
		}

		public class Condition
		{
			[XmlAttribute] public string ColumnName { get; set; }
			[XmlAttribute] public bool IsNull { get; set; }
			[XmlAttribute] public string Value { get; set; }
			[XmlAttribute] public string Name { get; set; }
		}

		public class DeleteFunction
		{
			[XmlAttribute] public string FunctionName { get; set; }
			[XmlAttribute] public string RowsAffectedParameter { get; set; }

			[XmlElement] public AssociationEnd[] AssociationEnd { get; set; }
			[XmlElement] public ComplexProperty[] ComplexProperty { get; set; }
			[XmlElement] public ScalarProperty[] ScalarProperty { get; set; }
		}

		public class EndProperty
		{
			[XmlAttribute] public string Name { get; set; }

			[XmlElement] public ScalarProperty[] ScalarProperty { get; set; }
		}

		public class EntityContainerMapping
		{
			[XmlAttribute] public string StorageModelContainer { get; set; }
			[XmlAttribute] public string CdmEntityContainer { get; set; }
			[XmlAttribute] public bool GenerateUpdateViews { get; set; }

			[XmlElement] public EntitySetMapping[] EntitySetMapping { get; set; }
			[XmlElement] public AssociationSetMapping[] AssociationSetMapping { get; set; }
			[XmlElement] public FunctionImportMapping[] FunctionImportMapping { get; set; }
		}

		public class EntitySetMapping
		{
			[XmlAttribute] public string Name { get; set; }
			[XmlAttribute] public string TypeName { get; set; }
			[XmlAttribute] public string StoreEntitySet { get; set; }
			[XmlAttribute] public bool MakeColumnsDistinct { get; set; }

			[XmlElement] public EntityTypeMapping[] EntityTypeMapping { get; set; }
			public QueryView QueryView { get; set; }
			[XmlElement] public MappingFragment[] MappingFragment { get; set; }
		}

		public class EntityTypeMapping
		{
			[XmlAttribute] public string TypeName { get; set; }

			[XmlElement] public MappingFragment[] MappingFragment { get; set; }
			public ModificationFunctionMapping ModificationFunctionMapping { get; set; }
			public ScalarProperty ScalarProperty { get; set; }
			public Condition Condition { get; set; }
		}

		public class FunctionImportMapping
		{
			[XmlAttribute] public string FunctionImportName { get; set; }
			[XmlAttribute] public string FunctionName { get; set; }

			[XmlElement] public ResultMapping[] ResultMapping { get; set; }
		}

		public class InsertFunction
		{
			[XmlAttribute] public string FunctionName { get; set; }
			[XmlAttribute] public string RowsAffectedParameter { get; set; }

			[XmlElement] public AssociationEnd[] AssociationEnd { get; set; }
			[XmlElement] public ComplexProperty[] ComplexProperty { get; set; }
			public ResultBinding ResultBinding { get; set; }
			[XmlElement] public ScalarProperty[] ScalarProperty { get; set; }
			public EndProperty EndProperty { get; set; }
		}

		public class Mapping
		{
			[XmlAttribute] public string Space { get; set; } = "C-S";

			[XmlElement] public Alias[] Alias { get; set; }
			public EntityContainerMapping EntityContainerMapping { get; set; }
		}

		public class MappingFragment
		{
			[XmlAttribute] public string StoreEntitySet { get; set; }
			[XmlAttribute] public bool MakeColumnsDistinct { get; set; }

			[XmlElement] public ComplexType[] ComplexType { get; set; }
			[XmlElement] public ScalarProperty[] ScalarProperty { get; set; }
			[XmlElement] public Condition[] Condition { get; set; }
		}

		public class ModificationFunctionMapping
		{
			public DeleteFunction DeleteFunction { get; set; }
			public InsertFunction InsertFunction { get; set; }
			public UpdateFunction UpdateFunction { get; set; }
		}

		public class QueryView
		{
			[XmlAttribute] public string TypeName { get; set; }

			[XmlText] public string Query { get; set; }
		}

		public class ResultBinding
		{
			[XmlAttribute] public string Name { get; set; }
			[XmlAttribute] public string ColumnName { get; set; }
		}

		public class ResultMapping
		{
			[XmlElement] public EntityTypeMapping[] EntityTypeMapping { get; set; }
			public ComplexTypeMapping ComplexTypeMapping { get; set; }
		}

		public class ScalarProperty
		{
			[XmlAttribute] public string Name { get; set; }
			[XmlAttribute] public string ColumnName { get; set; }
		}

		public class UpdateFunction
		{
			[XmlAttribute] public string FunctionName { get; set; }
			[XmlAttribute] public string RowsAffectedParameter { get; set; }

			[XmlElement] public AssociationEnd[] AssociationEnd { get; set; }
			[XmlElement] public ComplexProperty[] ComplexProperty { get; set; }
			[XmlElement] public ScalarProperty[] ScalarProperty { get; set; }
			public ResultBinding ResultBinding { get; set; }
		}
	}	
}