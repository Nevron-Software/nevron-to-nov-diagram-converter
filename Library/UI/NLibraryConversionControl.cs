using System;
using System.IO;
using System.Windows.Forms;

using Nevron.Diagram.WinForm;
using Nevron.Nov.Dom;
using Nevron.Nov.UI;

namespace Nevron.Nov.Diagram.Converter
{
    internal class NLibraryConversionControl : NConversionControl
    {
        #region Property Overrides

        protected override ENDocType DocType
        {
            get
            {
                return ENDocType.Library;
            }
        }
        protected override string[] FileExtensions
        {
            get
            {
                return LibraryFileExtensions;
            }
        }
        protected override Dom.NDocument NovDocument
        {
            get
            {
                return NovLibraryView.Document;
            }
            set
            {
                NovLibraryView.Document = (NLibraryDocument)value;
            }
        }
        protected override Nevron.Diagram.NDocument NevronDocument
        {
            get
            {
                return NevronLibraryView.Document;
            }
            set
            {
                NevronLibraryView.Document = (Nevron.Diagram.NLibraryDocument)value;
            }
        }

        #endregion

        #region Properties

        private Nevron.Diagram.WinForm.NLibraryView NevronLibraryView
        {
            get
            {
                return (Nevron.Diagram.WinForm.NLibraryView)m_NevronView;
            }
        }
        private NLibraryView NovLibraryView
        {
            get
            {
                return (NLibraryView)m_NovView;
            }
        }

        #endregion

        #region Protected Overrides - from NConversion Control

        protected override Control CreateNevronContent(out NView nevronView)
        {
            TableLayoutPanel table = new TableLayoutPanel();
            table.RowCount = 2;
            table.ColumnCount = 1;

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 80));

            // Create a Nevron library view
            nevronView = new Nevron.Diagram.WinForm.NLibraryView();
            nevronView.Dock = DockStyle.Fill;
            table.Controls.Add(nevronView, 0, 0);

            // Create a Nevron drawing view for testing
            GroupBox groupBox = new GroupBox();
            groupBox.Text = "Testing Nevron Drawing View";
            groupBox.Dock = DockStyle.Fill;
            table.Controls.Add(groupBox, 0, 1);

            Nevron.Diagram.WinForm.NDrawingView nevronDrawingView = CreateNevronDrawingView();
            nevronDrawingView.Dock = DockStyle.Fill;
            groupBox.Controls.Add(nevronDrawingView);

            return table;
        }
        protected override NWidget CreateNovContent(out INDocumentView novView)
        {
            novView = new NLibraryView();

            // Create a NOV drawing view for testing of the library items
            NDrawingView testingDrawingView = new NDrawingView();

            // Create a group box for the NOV drawing view
            NLabel groupBoxHeader = new NLabel(NLoc.Get("Testing NOV Drawing View"));
            NStylePropertyEx.SetRelativeFontSize(groupBoxHeader, ENRelativeFontSize.Large);
            NGroupBox groupBox = new NGroupBox(groupBoxHeader, testingDrawingView);

            // Place the library view and the drawing view in a pair box
            NPairBox pairBox = new NPairBox(novView, groupBox, ENPairBoxRelation.Box1AboveBox2);
            return pairBox;
        }

        protected override void ImportDocument()
        {
            // Convert the Nevron Library Document to a NOV Library Document
            NLibraryDocument newLibraryDocument;

            try
            {
                newLibraryDocument = NDiagramConverter.ConvertLibrary((Nevron.Diagram.NLibraryDocument)NevronDocument);
            }
            catch (Exception ex)
            {
                NMessageBox.ShowError(NLoc.Get("Converting of the selected Nevron Diagram XML document failed.") +
                    NLoc.Get("Error message") + ": " + ex.Message, NLoc.Get("Conversion Failed"));
                return;
            }

            NovDocument = newLibraryDocument;
        }
        protected override NDocument ConvertStream(Stream nevronStream, out Nevron.Diagram.NDocument nevronDocument)
        {
            Nevron.Diagram.NLibraryDocument nevronLibraryDocument;
            NLibraryDocument novLibraryDocument = NDiagramConverter.ConvertLibraryFromStream(nevronStream, out nevronLibraryDocument);
            nevronDocument = nevronLibraryDocument;

            return novLibraryDocument;
        }
        protected override bool SaveNevronDocumentToFile(string filePath)
        {
            Nevron.Diagram.Extensions.NPersistencyManager persistencyManager = NDiagramConverter.CreatePersistencyManager();
            return persistencyManager.SaveLibraryToFile((Nevron.Diagram.NLibraryDocument)NevronDocument, filePath);
        }

        #endregion

        #region Constants

        private static readonly string[] LibraryFileExtensions = new string[] { "nlx", "nlb" };

        #endregion
    }
}