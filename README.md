# Notmat XLIFF Library
![Logo](/images/logo%201%20128.png)

A comprehensive C# library for working with XLIFF (XML Localization Interchange File Format) files, supporting both XLIFF 1.2 and XLIFF 2.0 specifications.

## Inspiration ##
The idea to create this library was inspired by Microsoft's Multilingual App Toolkit. This project shares no code or other assets with the Multilingual App Toolkit. The Multilingual App Toolkit was [deprecated by Microsoft](https://learn.microsoft.com/en-us/windows/apps/design/globalizing/mat-announcements) and will no longer be supported after October 15, 2025.

Notmat is not a replacement for Multilingual App Toolkit. And that's OK. The initial code base was created with generous amounts of AI suggestions. And that's OK too.

## Features

- ✅ **Multi-Version Support**: Works with XLIFF 1.2 and XLIFF 2.0 formats
- ✅ **File I/O Operations**: Load and save XLIFF files with automatic version detection
- ✅ **Translation State Management**: Full support for translation states and approval workflows
- ✅ **RESX Integration**: Convert between XLIFF and .NET .resx resource files
- ✅ **Async Support**: Asynchronous file operations for better performance
- ✅ **Type Safety**: Strongly-typed models with XML serialization attributes
- ✅ **Comprehensive API**: Easy-to-use classes for common localization workflows

## Planned Features

- 🟩 Import/export to Javascript dictionary files
- 🟩 Visual XLIFF editor for Windows & MacOs
- 🟩 Command line tools
  - 🟩 Scanning code for etxt strings
  - 🟩 Generate XLIFF from REX
  - 🟩 Add new language to XLIFF
  - 🟩 Generate REX from XLIFF
- 🟩 Package availability via Nuget

## Installation (at some point)

Add the Notmat.Xliff NuGet package to your project:

```bash
dotnet add package Notmat.Xliff
```

## Quick Start

### Loading XLIFF Files

```csharp
using Notmat;

// Automatically detect version and load XLIFF file
var (version, document) = XliffLoader.LoadXliff("translations.xliff");

if (version == XliffVersion.Version12)
{
    var xliff12 = (Xliff12.XliffDocument)document;
    // Work with XLIFF 1.2 document
}
else if (version == XliffVersion.Version20)
{
    var xliff20 = (Xliff20.XliffDocument)document;
    // Work with XLIFF 2.0 document
}

// Load specific versions directly
var xliff12Doc = XliffLoader.LoadXliff12("translations.xliff");
var xliff20Doc = XliffLoader.LoadXliff20("translations.xliff");
```

### Creating XLIFF Files

```csharp
using Notmat.Xliff12;

// Create a new XLIFF 1.2 document
var xliffDoc = new XliffDocument();
var file = new XliffFile
{
    Original = "Resources.resx",
    SourceLanguage = "en-US",
    TargetLanguage = "fr-FR",
    DataType = "resx"
};

// Add translation units
var unit = new TranslationUnit
{
    Id = "welcome_message",
    ResourceName = "WelcomeMessage",
    Source = "Welcome to our application!",
    Target = "Bienvenue dans notre application!",
    State = TranslationState.Translated
};

file.Body.TranslationUnits.Add(unit);
xliffDoc.Files.Add(file);

// Save the document
XliffSaver.SaveXliff12(xliffDoc, "output.xliff");
```

### Converting from RESX Files

```csharp
using Notmat.Converters;

// Convert .resx to XLIFF 1.2
var xliff12 = ResxToXliffConverter.ConvertToXliff12(
    "Resources.resx", 
    "en-US", 
    "fr-FR"
);
XliffSaver.SaveXliff12(xliff12, "Resources.fr-FR.xliff");

// Convert .resx to XLIFF 2.0
var xliff20 = ResxToXliffConverter.ConvertToXliff20(
    "Resources.resx", 
    "en-US", 
    "es-ES"
);
XliffSaver.SaveXliff20(xliff20, "Resources.es-ES.xliff");
```

### Converting to RESX Files

```csharp
using Notmat.Converters;

// Convert XLIFF to .resx (using target text as values)
XliffToResxConverter.ConvertFile(
    "translations.xliff", 
    "Resources.fr-FR.resx", 
    useTargetAsValue: true,
    includeUntranslated: false
);

// Convert XLIFF document to .resx
var (version, document) = XliffLoader.LoadXliff("translations.xliff");
XliffToResxConverter.ConvertFromXliff(
    document, 
    "output.resx", 
    useTargetAsValue: true
);
```

## Translation States

The library supports all standard XLIFF translation states:

| State | Description |
|-------|-------------|
| `New` | Default state - no translation provided |
| `NeedsTranslation` | Translation is in progress |
| `NeedsReviewTranslation` | Translation completed but needs review |
| `Translated` | Translation reviewed and approved |
| `Final` | Translation is final and approved |
| `NeedsAdaptation` | Translation needs adaptation |
| `NeedsL10n` | Translation needs L10n review |
| `NeedsReviewAdaptation` | Translation needs review for adaptation |
| `NeedsReviewL10n` | Translation needs review for L10n |
| `SignedOff` | Translation is signed off |

## Working with Translation States

```csharp
// Set translation state
unit.State = TranslationState.Translated;

// Check if translation is complete
if (unit.State == TranslationState.Final || unit.State == TranslationState.SignedOff)
{
    // Translation is complete
}

// Find units that need translation
var pendingUnits = file.Body.TranslationUnits
    .Where(u => u.State == TranslationState.New || u.State == TranslationState.NeedsTranslation)
    .ToList();
```

## Adding a New Language to an Existing XLIFF File

### Method 1: Programmatic Approach

```csharp
using Notmat;

// Load the source XLIFF file
var (version, sourceDocument) = XliffLoader.LoadXliff("source.en-US.xliff");

if (version == XliffVersion.Version12 && sourceDocument is Xliff12.XliffDocument xliff12)
{
    // Create a new document for the target language
    var targetDoc = new Xliff12.XliffDocument();
    
    foreach (var sourceFile in xliff12.Files)
    {
        var targetFile = new Xliff12.XliffFile
        {
            Original = sourceFile.Original,
            SourceLanguage = sourceFile.SourceLanguage,
            TargetLanguage = "de-DE", // New target language
            DataType = sourceFile.DataType,
            Header = sourceFile.Header
        };
        
        // Copy translation units and reset states
        foreach (var sourceUnit in sourceFile.Body.TranslationUnits)
        {
            var targetUnit = new Xliff12.TranslationUnit
            {
                Id = sourceUnit.Id,
                ResourceName = sourceUnit.ResourceName,
                Source = sourceUnit.Source,
                Target = null, // Clear target for new language
                State = TranslationState.New // Reset state
            };
            
            targetFile.Body.TranslationUnits.Add(targetUnit);
        }
        
        targetDoc.Files.Add(targetFile);
    }
    
    // Save the new language file
    XliffSaver.SaveXliff12(targetDoc, "target.de-DE.xliff");
}
```

### Method 2: Template-Based Approach

```csharp
using Notmat;

public static class XliffLanguageHelper
{
    /// <summary>
    /// Creates a new XLIFF file for a target language based on an existing source file
    /// </summary>
    /// <param name="sourceFilePath">Path to the source XLIFF file</param>
    /// <param name="targetLanguage">Target language code (e.g., "de-DE")</param>
    /// <param name="outputPath">Output path for the new XLIFF file</param>
    /// <param name="preserveTranslations">Whether to preserve existing translations</param>
    public static void CreateLanguageVariant(string sourceFilePath, string targetLanguage, 
        string outputPath, bool preserveTranslations = false)
    {
        var (version, sourceDocument) = XliffLoader.LoadXliff(sourceFilePath);
        
        switch (version)
        {
            case XliffVersion.Version12 when sourceDocument is Xliff12.XliffDocument xliff12:
                CreateLanguageVariant12(xliff12, targetLanguage, outputPath, preserveTranslations);
                break;
                
            case XliffVersion.Version20 when sourceDocument is Xliff20.XliffDocument xliff20:
                CreateLanguageVariant20(xliff20, targetLanguage, outputPath, preserveTranslations);
                break;
                
            default:
                throw new NotSupportedException($"Unsupported XLIFF version: {version}");
        }
    }
    
    private static void CreateLanguageVariant12(Xliff12.XliffDocument source, string targetLanguage, 
        string outputPath, bool preserveTranslations)
    {
        var targetDoc = new Xliff12.XliffDocument();
        
        foreach (var sourceFile in source.Files)
        {
            var targetFile = new Xliff12.XliffFile
            {
                Original = sourceFile.Original,
                SourceLanguage = sourceFile.SourceLanguage,
                TargetLanguage = targetLanguage,
                DataType = sourceFile.DataType,
                Header = CloneHeader(sourceFile.Header)
            };
            
            foreach (var sourceUnit in sourceFile.Body.TranslationUnits)
            {
                var targetUnit = new Xliff12.TranslationUnit
                {
                    Id = sourceUnit.Id,
                    ResourceName = sourceUnit.ResourceName,
                    Source = sourceUnit.Source,
                    Target = preserveTranslations ? sourceUnit.Target : null,
                    State = preserveTranslations && !string.IsNullOrEmpty(sourceUnit.Target) 
                        ? sourceUnit.State 
                        : TranslationState.New
                };
                
                targetFile.Body.TranslationUnits.Add(targetUnit);
            }
            
            targetDoc.Files.Add(targetFile);
        }
        
        XliffSaver.SaveXliff12(targetDoc, outputPath);
    }
    
    private static void CreateLanguageVariant20(Xliff20.XliffDocument source, string targetLanguage, 
        string outputPath, bool preserveTranslations)
    {
        var targetDoc = new Xliff20.XliffDocument
        {
            SourceLanguage = source.SourceLanguage,
            TargetLanguage = targetLanguage
        };
        
        foreach (var sourceFile in source.Files)
        {
            var targetFile = new Xliff20.XliffFile
            {
                Id = sourceFile.Id,
                Original = sourceFile.Original,
                Header = CloneHeader20(sourceFile.Header)
            };
            
            foreach (var sourceUnit in sourceFile.Units)
            {
                var targetUnit = new Xliff20.TranslationUnit
                {
                    Id = sourceUnit.Id,
                    Name = sourceUnit.Name
                };
                
                foreach (var sourceSegment in sourceUnit.Segments)
                {
                    var targetSegment = new Xliff20.Segment
                    {
                        Id = sourceSegment.Id,
                        Source = sourceSegment.Source,
                        Target = preserveTranslations ? sourceSegment.Target : null,
                        State = preserveTranslations && !string.IsNullOrEmpty(sourceSegment.Target)
                            ? sourceSegment.State
                            : TranslationState.New
                    };
                    
                    targetUnit.Segments.Add(targetSegment);
                }
                
                targetFile.Units.Add(targetUnit);
            }
            
            targetDoc.Files.Add(targetFile);
        }
        
        XliffSaver.SaveXliff20(targetDoc, outputPath);
    }
    
    private static Xliff12.XliffHeader? CloneHeader(Xliff12.XliffHeader? source)
    {
        if (source == null) return null;
        
        return new Xliff12.XliffHeader
        {
            Tool = source.Tool != null ? new Xliff12.XliffTool
            {
                ToolId = source.Tool.ToolId,
                ToolName = source.Tool.ToolName,
                ToolVersion = source.Tool.ToolVersion
            } : null,
            Notes = source.Notes.ToList()
        };
    }
    
    private static Xliff20.XliffHeader? CloneHeader20(Xliff20.XliffHeader? source)
    {
        if (source == null) return null;
        
        return new Xliff20.XliffHeader
        {
            Tool = source.Tool != null ? new Xliff20.XliffTool
            {
                Id = source.Tool.Id,
                Name = source.Tool.Name,
                Version = source.Tool.Version
            } : null,
            Notes = source.Notes.ToList()
        };
    }
}

// Usage example:
XliffLanguageHelper.CreateLanguageVariant("source.en-US.xliff", "de-DE", "target.de-DE.xliff");
```

## Step-by-Step Guide: Adding a New Language

1. **Identify the Source File**: Start with your source XLIFF file (e.g., `Resources.en-US.xliff`)

2. **Create the Target File**: Use the `XliffLanguageHelper.CreateLanguageVariant()` method or create manually

3. **Translate Content**: Open the new XLIFF file in your preferred translation tool or edit programmatically:
   ```csharp
   var (version, document) = XliffLoader.LoadXliff("target.de-DE.xliff");
   
   if (document is Xliff12.XliffDocument xliff12)
   {
       foreach (var file in xliff12.Files)
       {
           foreach (var unit in file.Body.TranslationUnits)
           {
               if (unit.Id == "welcome_message")
               {
                   unit.Target = "Willkommen in unserer Anwendung!";
                   unit.State = TranslationState.Translated;
               }
           }
       }
       
       XliffSaver.SaveXliff12(xliff12, "target.de-DE.xliff");
   }
   ```

4. **Update Translation States**: Mark translations as complete:
   ```csharp
   // Mark as translated
   unit.State = TranslationState.Translated;
   
   // Mark as final after review
   unit.State = TranslationState.Final;
   ```

5. **Generate Resources**: Convert back to .resx if needed:
   ```csharp
   XliffToResxConverter.ConvertFile(
       "target.de-DE.xliff", 
       "Resources.de-DE.resx",
       useTargetAsValue: true,
       includeUntranslated: false
   );
   ```

## API Reference

### Core Classes

- **`XliffLoader`**: Load XLIFF files with automatic version detection
- **`XliffSaver`**: Save XLIFF files in various formats
- **`ResxToXliffConverter`**: Convert .resx files to XLIFF format
- **`XliffToResxConverter`**: Convert XLIFF files to .resx format

### Data Models

#### XLIFF 1.2
- **`Xliff12.XliffDocument`**: Root document
- **`Xliff12.XliffFile`**: File container
- **`Xliff12.TranslationUnit`**: Individual translation entry
- **`Xliff12.XliffNote`**: Notes and comments

#### XLIFF 2.0
- **`Xliff20.XliffDocument`**: Root document
- **`Xliff20.XliffFile`**: File container
- **`Xliff20.TranslationUnit`**: Translation unit container
- **`Xliff20.Segment`**: Individual translation segment
- **`Xliff20.XliffNote`**: Notes and comments

### Enums

- **`XliffVersion`**: XLIFF version (1.2 or 2.0)
- **`TranslationState`**: Translation workflow states
- **`ApprovalState`**: Approval status (XLIFF 2.0)

## Building and Testing

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Pack for NuGet
dotnet pack
```

## NuGet package

To create the NuGet package (.nupkg) for Notmat.Xliff using csproj-based packing:

```pwsh
# From the repository root
# Build and pack the library in Release
 dotnet pack Notmat.Xliff/Notmat.Xliff.csproj -c Release -o dist
```

The package will be placed in the `dist/` folder. To publish to NuGet.org:

```pwsh
# Replace with your actual API key
 dotnet nuget push dist/*.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Supported XLIFF Specifications

- [XLIFF 1.2 Specification](http://docs.oasis-open.org/xliff/xliff-core/xliff-core.html)
- [XLIFF 2.0 Specification](http://docs.oasis-open.org/xliff/xliff-core/v2.0/xliff-core-v2.0.html)