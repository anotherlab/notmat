# SharpFetch

SharpFetch is a cross-platform [FastFetch](https://github.com/fastfetch-cli/fastfetch) clone written in C#. It is designed to demonstrate how to localize a .NET application using resource files. The project is part of the Notmat solution and serves as a sample for implementing localization in your own apps.

It comes with a default English resource file and a Spanish resource file.

## Features
- Cross-platform support (Windows, macOS, Linux)
- Displays system information similar to FastFetch
- Demonstrates localization using .resx resource files

## Localization
SharpFetch uses .resx resource files to provide localized strings. By default, the application uses the system's current UI culture. You can override the language by specifying the desired culture code as a command line argument.

### Command Line Syntax
To run SharpFetch with the Spanish language resource file, use the following command:

```
dotnet run --project Samples/SharpFetch -- -l es
```

```
dotnet run --project Samples/SharpFetch -- --locale es
```

Or, if running the compiled executable:

```
./sharpfetch -l es
```

```
./sharpfetch --local es
```

The `es` argument tells SharpFetch to use the Spanish (`es`) resource file for localization.

## License
This project is licensed under the MIT License.
