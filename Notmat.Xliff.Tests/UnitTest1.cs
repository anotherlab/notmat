using Notmat;
using Notmat.Converters;
using Notmat.Xliff12;
using Notmat.Xliff20;
using System.IO;
using Xunit;

namespace Notmat.Xliff.Tests;

public class XliffLoaderTests
{
    [Fact]
    public void CanCreateXliff12Document()
    {
        // Arrange
        var doc = new Xliff12.XliffDocument();
        var file = new Xliff12.XliffFile
        {
            Original = "test.resx",
            SourceLanguage = "en-US",
            TargetLanguage = "fr-FR"
        };

        var unit = new Xliff12.TranslationUnit
        {
            Id = "test1",
            Source = "Hello",
            Target = "Bonjour",
            State = TranslationState.Translated
        };

        file.Body.TranslationUnits.Add(unit);
        doc.Files.Add(file);

        // Act & Assert
        Assert.Single(doc.Files);
        Assert.Single(doc.Files[0].Body.TranslationUnits);
        Assert.Equal("Hello", doc.Files[0].Body.TranslationUnits[0].Source);
        Assert.Equal("Bonjour", doc.Files[0].Body.TranslationUnits[0].Target);
        Assert.Equal(TranslationState.Translated, doc.Files[0].Body.TranslationUnits[0].State);
    }

    [Fact]
    public void CanCreateXliff20Document()
    {
        // Arrange
        var doc = new Xliff20.XliffDocument
        {
            SourceLanguage = "en-US",
            TargetLanguage = "es-ES"
        };

        var file = new Xliff20.XliffFile
        {
            Id = "file1",
            Original = "test.resx"
        };

        var unit = new Xliff20.TranslationUnit
        {
            Id = "unit1",
            Name = "greeting"
        };

        var segment = new Xliff20.Segment
        {
            Id = "1",
            Source = "Hello",
            Target = "Hola",
            State = TranslationState.Final
        };

        unit.Segments.Add(segment);
        file.Units.Add(unit);
        doc.Files.Add(file);

        // Act & Assert
        Assert.Single(doc.Files);
        Assert.Single(doc.Files[0].Units);
        Assert.Single(doc.Files[0].Units[0].Segments);
        Assert.Equal("Hello", doc.Files[0].Units[0].Segments[0].Source);
        Assert.Equal("Hola", doc.Files[0].Units[0].Segments[0].Target);
        Assert.Equal(TranslationState.Final, doc.Files[0].Units[0].Segments[0].State);
    }

    [Fact]
    public void CanSaveAndLoadXliff12()
    {
        // Arrange
        var originalDoc = new Xliff12.XliffDocument();
        var file = new Xliff12.XliffFile
        {
            Original = "test.resx",
            SourceLanguage = "en-US",
            TargetLanguage = "fr-FR"
        };

        var unit = new Xliff12.TranslationUnit
        {
            Id = "test1",
            ResourceName = "TestResource",
            Source = "Hello World",
            Target = "Bonjour le monde",
            State = TranslationState.Final
        };

        file.Body.TranslationUnits.Add(unit);
        originalDoc.Files.Add(file);

        var tempFile = Path.GetTempFileName();

        try
        {
            // Act - Save
            XliffSaver.SaveXliff12(originalDoc, tempFile);

            // Assert - File exists
            Assert.True(File.Exists(tempFile));

            // Act - Load
            var loadedDoc = XliffLoader.LoadXliff12(tempFile);

            // Assert - Content matches
            Assert.Single(loadedDoc.Files);
            Assert.Equal("en-US", loadedDoc.Files[0].SourceLanguage);
            Assert.Equal("fr-FR", loadedDoc.Files[0].TargetLanguage);
            Assert.Single(loadedDoc.Files[0].Body.TranslationUnits);
            
            var loadedUnit = loadedDoc.Files[0].Body.TranslationUnits[0];
            Assert.Equal("test1", loadedUnit.Id);
            Assert.Equal("TestResource", loadedUnit.ResourceName);
            Assert.Equal("Hello World", loadedUnit.Source);
            Assert.Equal("Bonjour le monde", loadedUnit.Target);
            Assert.Equal(TranslationState.Final, loadedUnit.State);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void TranslationStateEnumWorksCorrectly()
    {
        // Test various translation states
        var states = new[]
        {
            TranslationState.New,
            TranslationState.NeedsTranslation,
            TranslationState.Translated,
            TranslationState.Final,
            TranslationState.SignedOff
        };

        foreach (var state in states)
        {
            var unit = new Xliff12.TranslationUnit
            {
                Id = "test",
                Source = "Test",
                State = state
            };

            Assert.Equal(state, unit.State);
        }
    }
}