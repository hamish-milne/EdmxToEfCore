# EDMX to Entity Framework Core

A utility for generating Entity Framework Core models from an existing EDMX diagram. The generated code is treated as a 'code-first' model by the framework. This allows the use of Visual Studio's excellent visual editor for entity models without needing DDL providers installed in Visual Studio and such, and of course to take advantage of EF Core, which Microsoft has made pretty clear is the successor to EF6.

My motivation for this tool was to use an EDMX model with an SQLite backend, something which is apparently extraordinarily difficult to do in EF6.

## Features

Just add the package and it'll automatically convert all the files in the project.

At the moment the tool works well for conventional models. If you're doing something weird it probably won't work. Or hell, it might not work anyway. Raise an issue, or just fix it yourself and file a PR: the code's pretty straightforward.
