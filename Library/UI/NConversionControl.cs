using System;
using System.IO;
using System.Windows.Forms;

using Nevron.Nov.Dom;
using Nevron.Nov.UI;

using Nevron.Nov.Windows.Forms;

namespace Nevron.Nov.Diagram.Converter
{
	internal abstract class NConversionControl : TableLayoutPanel
    {
        #region Constructors

        protected NConversionControl()
        {
            Dock = DockStyle.Fill;
            RowCount = 2;
            ColumnCount = 2;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Add the toolbar at the top
            string docTypeStr = NEnum.GetLocalizedString(DocType);
            NNovWidgetHost<NWidget> novHost = new NNovWidgetHost<NWidget>(CreateNovToolbar(docTypeStr));
            novHost.Dock = DockStyle.Top;
            Controls.Add(novHost, 0, 0);
            SetColumnSpan(novHost, 2);

            // Add the Nevron content
            GroupBox groupBox = new GroupBox();
            groupBox.Text = String.Format(NLoc.Get("Nevron {0}"), docTypeStr);
            groupBox.Dock = DockStyle.Fill;
            Controls.Add(groupBox, 0, 1);

            Control nevronContent = CreateNevronContent(out m_NevronView);
            nevronContent.Dock = DockStyle.Fill;
            groupBox.Controls.Add(nevronContent);

            // Add the NOV content
            groupBox = new GroupBox();
            groupBox.Text = String.Format(NLoc.Get("NOV {0}"), docTypeStr);
            groupBox.Dock = DockStyle.Fill;
            Controls.Add(groupBox, 1, 1);

            novHost = new NNovWidgetHost<NWidget>(CreateNovContent(out m_NovView));
            novHost.Dock = DockStyle.Fill;
            groupBox.Controls.Add(novHost);
        }

        #endregion

        #region Properties - Must Override

        protected abstract ENDocType DocType
        {
            get;
        }
        protected abstract string[] FileExtensions
        {
            get;
        }
        protected abstract NDocument NovDocument
        {
            get;
            set;
        }
        protected abstract Nevron.Diagram.NDocument NevronDocument
        {
            get;
            set;
        }

        #endregion

        #region Protected Must Override

        protected abstract Control CreateNevronContent(out Nevron.Diagram.WinForm.NView nevronView);
        protected abstract NWidget CreateNovContent(out INDocumentView novView);
        /// <summary>
        /// Perform import from left view to the right.
        /// </summary>
        protected abstract void ImportDocument();

        protected abstract NDocument ConvertStream(Stream nevronStream, out Nevron.Diagram.NDocument nevronDocument);
        /// <summary>
        /// Saves the given Nevron document to file.
        /// </summary>
        /// <param name="filePath"></param>
        protected abstract bool SaveNevronDocumentToFile(string filePath);

        #endregion

        #region Protected Overrides - from Control

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // The Dock of the Nevron View should be set after the control is added to a form,
            // otherwise it doesn't work
            m_NevronView.Dock = DockStyle.Fill;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a Nevron Drawing View with commanding (context menus only).
        /// </summary>
        /// <returns></returns>
        protected Nevron.Diagram.WinForm.NDrawingView CreateNevronDrawingView()
        {
            Nevron.Diagram.WinForm.NDrawingView nevronDrawingView = new Nevron.Diagram.WinForm.NDrawingView();
            nevronDrawingView.Document = new Nevron.Diagram.NDrawingDocument();

            // Create a Nevron command bars manager to add support for context menu commands in the Nevron Drawing View
            Nevron.Diagram.WinForm.Commands.NDiagramCommandBarsManager manager = new Nevron.Diagram.WinForm.Commands.NDiagramCommandBarsManager();
            manager.View = nevronDrawingView;
            manager.Toolbars.Clear();

            return nevronDrawingView;
        }

        #endregion

        #region Implementation

        private NToolBar CreateNovToolbar(string docType)
        {
            NToolBar toolbar = new NToolBar();
            toolbar.Pendant.Visibility = ENVisibility.Collapsed;
            toolbar.Gripper.Visibility = ENVisibility.Collapsed;

            NButton openButton = NButton.CreateImageAndText(Nevron.Nov.Presentation.NResources.Image_File_Open_png,
                String.Format(NLoc.Get("Open Nevron {0}"), docType));
            openButton.Click += OnOpenButtonClick;
            toolbar.Items.Add(openButton);

            NButton saveButton = NButton.CreateImageAndText(Nevron.Nov.Diagram.NResources.Image_Library_LibrarySave_png,
                String.Format(NLoc.Get("Save Nevron {0}"), docType));
            saveButton.Click += OnSaveNetButtonClick;
            toolbar.Items.Add(saveButton);

            toolbar.Items.Add(new NCommandBarSeparator());

            NButton importButton = NButton.CreateImageAndText(Nevron.Nov.Presentation.NResources.Image_Insert_VectorImage_png,
                NLoc.Get("Import to NOV"));
            importButton.Click += OnImportButtonClick;
            toolbar.Items.Add(importButton);

            NButton saveNovButton = NButton.CreateImageAndText(Nevron.Nov.Presentation.NResources.Image_File_Save_png,
                String.Format(NLoc.Get("Save NOV {0}"), docType));
            saveNovButton.Click += OnSaveNovButtonClick;
            toolbar.Items.Add(saveNovButton);

            return toolbar;
        }

        #endregion

        #region Event Handlers

        private void OnOpenButtonClick(NEventArgs arg)
        {
            string docType = NEnum.GetLocalizedString(DocType);

            NOpenFileDialog dialog = new NOpenFileDialog();
            dialog.Title = String.Format(NLoc.Get("Open Nevron {0}"), docType);
            dialog.FileTypes = new NFileDialogFileType[] {
                new NFileDialogFileType(String.Format("Nevron Diagram {0}", docType), FileExtensions),
            };

            dialog.Closed += OnOpenDialogClosed;
            dialog.RequestShow();
        }
        private void OnOpenDialogClosed(NOpenFileDialogResult arg)
        {
            if (arg.Result != ENCommonDialogResult.OK)
                return;

            try
            {
                string filePath = arg.Files[0].Path;
                using (Stream stream = File.OpenRead(filePath))
                {
                    Nevron.Diagram.NDocument nevronDocument;
                    NDocument novDocument = ConvertStream(stream, out nevronDocument);
                    NevronDocument = nevronDocument;
                    NovDocument = novDocument;
                }
            }
            catch (Exception ex)
            {
                NMessageBox.ShowError(NLoc.Get("Failed to convert the selected Nevron Drawing document to a NOV Drawing document.") + Environment.NewLine +
                    NLoc.Get("Error message") + ": " + ex.Message, NLoc.Get("Conversion Failed"));
            }
        }
        private void OnSaveNetButtonClick(NEventArgs arg)
        {
            NSaveFileDialog dialog = new NSaveFileDialog();
            dialog.Title = NLoc.Get($"Save Nevron {DocType}");
            dialog.FileTypes = new NFileDialogFileType[] {
                new NFileDialogFileType($"Nevron Diagram XML {DocType}", FileExtensions[0])
            };

            dialog.Closed += OnSaveDialogClosed;
            dialog.RequestShow();
        }
        private void OnSaveDialogClosed(NSaveFileDialogResult arg)
        {
            if (arg.Result != ENCommonDialogResult.OK)
                return;


            // Save the Nevron Diagram to the selected file
            try
            {
                if (!SaveNevronDocumentToFile(arg.File.Path))
                {
                    NMessageBox.ShowError(NLoc.Get("Saving to the selected file failed."),
                        NLoc.Get("Saving Failed"));
                    return;
                }
            }
            catch (Exception ex)
            {
                NMessageBox.ShowError(NLoc.Get("Saving to the selected file failed.") + NLoc.Get("Error message") +
                    ": " + ex.Message, NLoc.Get("Saving Failed"));
                return;
            }
        }

        private void OnSaveNovButtonClick(NEventArgs arg)
        {
            m_NovView.SaveAsAsync();
        }
        private void OnImportButtonClick(NEventArgs arg)
        {
            ImportDocument();
        }

        #endregion

        #region Fields

        protected Nevron.Diagram.WinForm.NView m_NevronView;
        protected INDocumentView m_NovView;

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a conversion control of the given type.
        /// </summary>
        /// <param name="docType"></param>
        /// <returns></returns>
        public static NConversionControl Create(ENDocType docType)
        {
            if (docType == ENDocType.Drawing)
                return new NDrawingConversionControl();
            else
                return new NLibraryConversionControl();
        }

        #endregion
    }
}