# Nevron to NOV Diagram Converter
## Application
The Nevron to NOV Diagram Converter application can be used to convert Nevron Diagram drawings and libraries to NOV Diagram drawings and libraries. You can either download the project from this repository, open it with Visual Studio and compile it or download the application binaries from the following link:
[Download Nevron to NOV Diagram Converter](https://www.nevron.com/_author/res/downloads/NevronToNovDiagramConverter.zip).

After you run the application, open the first tab ("Drawings") and then click the <b>Open Nevron Drawing</b> button to open a Nevron Diagram drawing and convert it to a NOV Drawing. You can then click the <b>Save NOV Drawing</b> button to save the drawing in the new format. To convert libraries, open the second tab ("Libraries").

## Library

If you want to automate the document conversion, you can use the NevronToNovDiagramConverter DLL in you project. To do that, follow the steps below:
  1. Make sure that your application targets .NET Framework 4.7.2.
  2. Add all references from the folder of the Nevron to NOV Diagram Converter application (without the EXE file).
  3. Use the NDiagramConverter class from the NevronToNovDiagramConverter DLL to convert drawings and libraries to the new format:
     - Use the ConvertDrawingFromStream method to convert Nevron Diagram drawings (binary – NDB and XML – NDX) to the new format.
     - Use the ConvertLibraryFromStream method to convert Nevron Diagram libraries (binary – NLB and XML - NLX) to the new format.
  4. Save the generated NOV drawing or library to stream or back to your database. For example:

```NDrawingFormat.NevronBinary.SaveToStream(novDrawingDocument, stream);```

You can do the same for libraries:

```NLibraryFormat.NevronBinary.SaveToStream(novLibraryDocument, stream);```
