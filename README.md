# EDMX to Entity Framework Core
A utility for generating Entity Framework Core models from an existing EDMX diagram. The generated code is treated as a 'code-first' model by the framework. This allows the use of Visual Studio's excellent visual editor for entity models without needing DDL providers installed in Visual Studio and such, and of course to take advantage of EF Core, which Microsoft has made pretty clear is the successor to EF6.

My motivation for this tool was to use an EDMX model with an SQLite backend, something which is apparently extraordinarily difficult to do in EF6.

# Features

Run the program, give it an EDMX and an output file, and it'll generate the code.

At the moment the tool works well for conventional models. If you're doing something weird it probably won't work. Or hell, it might not work anyway. Raise an issue, or just fix it yourself and file a PR: the code's pretty straightforward.

## TODO

* Make this a 'tool'
* Support derived types not in an EntitySet
* Support many-to-many relationships (without an explicit join entity)
* Complex types, inc. nested
* Enum types
* Add OnDelete (with fluent API)
* Unicode property?
* Warning for fixed length property
* Figure out what to do with StoreGeneratedPattern
* Default value support (property defaults? or fluent API?)
* Support owned types?
* Collection types outside of navigation properties?
* One file per class mode
* Automatic determining of Primary/Dependent one-to-one relationships with no foreign key specified
* Generate an OnConfigureModel stub with forwarding to user-defined method?
