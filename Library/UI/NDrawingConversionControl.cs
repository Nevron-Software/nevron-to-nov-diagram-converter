using System;
using System.IO;
using System.Windows.Forms;

using Nevron.Nov.Dom;
using Nevron.Nov.UI;

namespace Nevron.Nov.Diagram.Converter
{
    internal class NDrawingConversionControl : NConversionControl
    {
        #region Property Overrides

        protected override ENDocType DocType
        {
            get
            {
                return ENDocType.Drawing;
            }
        }
        protected override string[] FileExtensions
        {
            get
            {
                return DrawingFileExtensions;
            }
        }
        protected override Nevron.Diagram.NDocument NevronDocument
        {
            get
            {
                return NevronDrawingView.Document;
            }
            set
            {
                NevronDrawingView.Document = (Nevron.Diagram.NDrawingDocument)value;
            }
        }
        protected override NDocument NovDocument
        {
            get
            {
                return NovDrawingView.Document;
            }
            set
            {
                NovDrawingView.Document = (NDrawingDocument)value;
            }
        }

        #endregion

        #region Properties

        private Nevron.Diagram.WinForm.NDrawingView NevronDrawingView
        {
            get
            {
                return (Nevron.Diagram.WinForm.NDrawingView)m_NevronView;
            }
        }
        private NDrawingView NovDrawingView
        {
            get
            {
                return (NDrawingView)m_NovView;
            }
        }

		#endregion

		#region Protected Overrides - Toolbar

		protected override NToolBar CreateNovToolbar(string docType)
		{
			NToolBar toolbar = base.CreateNovToolbar(docType);

			toolbar.Items.Add(new NCommandBarSeparator());

			NButton zoomToFitButton = NButton.CreateImageAndText(Nevron.Nov.Presentation.NResources.Image_View_FitZoomMode_png,
				NLoc.Get("Zoom to Fit"));
			zoomToFitButton.Click += OnZoomToFitButtonClick;
			toolbar.Items.Add(zoomToFitButton);


			return toolbar;
		}

		#endregion

		#region Protected Overrides - from NConversionControl

		protected override Control CreateNevronContent(out Nevron.Diagram.WinForm.NView nevronView)
        {
            nevronView = CreateNevronDrawingView();
            return nevronView;
        }
        protected override NWidget CreateNovContent(out INDocumentView novView)
        {
            novView = new NDrawingView();
            return (NDrawingView)novView;
        }

        protected override void ImportDocument()
        {
            // Convert the Nevron Drawing document to a NOV Drawing document
            NDrawingDocument newDrawingDocument;

            try
            {
                newDrawingDocument = NDiagramConverter.ConvertDrawing((Nevron.Diagram.NDrawingDocument)NevronDocument);
            }
            catch (Exception ex)
            {
                NMessageBox.ShowError(NLoc.Get("Converting of the selected Nevron Diagram XML document failed.") +
                    NLoc.Get("Error message") + ": " + ex.Message, NLoc.Get("Conversion Failed"));
                return;
            }

            NovDocument = newDrawingDocument;
        }
        protected override NDocument ConvertStream(Stream nevronStream, out Nevron.Diagram.NDocument nevronDocument)
        {
            Nevron.Diagram.NDrawingDocument nevronDrawingDocument;
            NDrawingDocument novDrawingDocument = NDiagramConverter.ConvertDrawingFromStream(nevronStream, out nevronDrawingDocument);
            nevronDocument = nevronDrawingDocument;

            return novDrawingDocument;
        }
        protected override bool SaveNevronDocumentToFile(string filePath)
        {
            Nevron.Diagram.Extensions.NPersistencyManager persistencyManager = NDiagramConverter.CreatePersistencyManager();
            return persistencyManager.SaveDrawingToFile((Nevron.Diagram.NDrawingDocument)NevronDocument, filePath);
        }

		#endregion

		#region Event Handlers

		private void OnZoomToFitButtonClick(NEventArgs arg)
		{
			Nevron.Diagram.WinForm.NDrawingView nevronDrawingView = (Nevron.Diagram.WinForm.NDrawingView)m_NevronView;
			nevronDrawingView.ViewLayout = Nevron.Diagram.ViewLayout.Fit;

			NDrawingView novDrawingView = (NDrawingView)m_NovView;
			novDrawingView.ActivePage.ZoomMode = ENZoomMode.Fit;
		}

		#endregion

		#region Constants

		private static readonly string[] DrawingFileExtensions = new string[] { "ndx", "xml", "ndb" };

        #endregion
    }
}