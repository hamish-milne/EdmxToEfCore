# EDMX to Entity Framework Core

[![NuGet](https://www.nuget.org/packages/EdmxToEfCore/)](https://img.shields.io/nuget/v/EdmxToEfCore.svg)
[![Build status](https://ci.appveyor.com/api/projects/status/d8h898lhghnedwmd?svg=true)](https://ci.appveyor.com/project/hamish-milne/edmxtoefcore)

A utility for generating Entity Framework Core models from an existing EDMX diagram. The generated code is treated as a 'code-first' model by the framework. This allows the use of Visual Studio's excellent visual editor for entity models without needing DDL providers installed in Visual Studio and such, and of course to take advantage of EF Core, which Microsoft has made pretty clear is the successor to EF6.

My motivation for this tool was to use an EDMX model with an SQLite backend, something which is apparently extraordinarily difficult to do in EF6.

To my knowledge, nearly every feature of the CSDL portion (the bit that defines the model) in EDMX is supported, so the tool should work well for most models. I'd like to make this as robust as possible, so if you find your model doesn't compile or isn't as expected, please do file an issue!

## Future work

See the TODO for a full roadmap, but in short:

* Support for many-many relationships without a join entity
* Use of the Mappings section for database interoperability
* Better logging
* Other bits and pieces

## Usage

The package is formed as an MSBuild task that gets invoked before compilation. Items to process are added in the same way as source files, but the item name being *EdmxToEfCore*. Normal rules apply regarding semicolon-separated lists and path globbing. For example, the following snippet in your .csproj file will process every .edmx file in the project:

```XML
<!-- NB: This is included in the package by default -->
<ItemGroup>
    <EdmxToEfCore Include="*.edmx" />
</ItemGroup>
```

There are several properties you can add to control the generation process:

```XML
<ItemGroup>
    <EdmxToEfCore Include="MyModel.edmx">
        <!-- These are the default values for each property -->
        <OutputPathPattern>{0}.autogenerated.cs</OutputPathPattern>
        <CollectionType>List</CollectionType>
        <ComplexMetaType>class</ComplexMetaType>
        <FileMode>SingleFile</FileMode>
        <LazyLoading />
    </EdmxToEfCore>
</ItemGroup>
```

* *OutputPathPattern*: Where to put the generated code files. If present, `{0}` is replaced with the base file name in single file mode, or the class name in multi-file mode.
* *CollectionType*: The type to use for collections in navigation properties. Acceptable values are *Array*, *List* and *HashSet*.
* *ComplexMetaType*: The meta-type to use for Complex Types (not entity types, which are always classes). Acceptable values are *Class* and *Struct*.
* *FileMode*: Accepted values are *SingleFile* and *PerClass*.
* *LazyLoading*: Allows overriding the property in EDMX to force lazy-loading on or off. Accepted values are *true* and *false*.
