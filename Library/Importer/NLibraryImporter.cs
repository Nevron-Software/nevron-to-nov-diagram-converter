namespace Nevron.Nov.Diagram.Converter
{
    internal class NLibraryImporter : NDiagramImporter
    {
        #region Public Methods

        /// <summary>
        /// Creates a NOV library document from the given Nevron library document.
        /// </summary>
        /// <param name="nevronLibraryDocument"></param>
        /// <returns></returns>
        public NLibraryDocument Import(Nevron.Diagram.NLibraryDocument nevronLibraryDocument)
        {
            Initialize();
            InitializeDefaultNevronStyles();

            NLibraryDocument novLibraryDocument = new NLibraryDocument();
            NLibrary novLibrary = novLibraryDocument.Content;

            // Convert each master to a NOV library item
            for (int i = 0; i < nevronLibraryDocument.Nodes.Length; i++)
            {
                if (nevronLibraryDocument.Nodes[i] is Nevron.Diagram.NMaster nevronMaster)
                {
                    ImportMaster(novLibrary, nevronMaster);
                }
            }

            return novLibraryDocument;
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Applies styles to the given NOV geometry. Overriden to apply default Nevron Drawing Document styles if styles are not specified
        /// for the given Nevron model.
        /// </summary>
        /// <param name="novGeometry"></param>
        /// <param name="nevronModel"></param>
        protected override void ApplyStyles(NGeometry novGeometry, Nevron.Diagram.NModel nevronModel)
        {
            GraphicsCore.NFillStyle nevronFillStyle = nevronModel.ComposeFillStyle() ?? m_DefaultNevronFillStyle;
            novGeometry.Fill = NFillStyleImporter.ToFill(nevronFillStyle);

            GraphicsCore.NStrokeStyle nevronStrokeStyle = nevronModel.ComposeStrokeStyle() ?? m_DefaultNevronStrokeStyle;
            novGeometry.Stroke = NStrokeStyleImporter.ToStroke(nevronStrokeStyle);

            Nevron.Diagram.NArrowheadStyle nevronBeginArrowheadStyle = nevronModel.ComposeStartArrowheadStyle() ?? m_DefaultNevronBeginArrowheadStyle;
            novGeometry.BeginArrowhead = NArrowheadStyleImporter.ToArrowhead(nevronBeginArrowheadStyle);

            Nevron.Diagram.NArrowheadStyle nevronEndArrowheadStyle = nevronModel.ComposeEndArrowheadStyle() ?? m_DefaultNevronEndArrowheadStyle;
            novGeometry.EndArrowhead = NArrowheadStyleImporter.ToArrowhead(nevronEndArrowheadStyle);
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Converts the given Nevron master to a NOV library item.
        /// </summary>
        /// <param name="nevronMaster"></param>
        /// <returns></returns>
        private NLibraryItem ImportMaster(NLibrary novLibrary, Nevron.Diagram.NMaster nevronMaster)
        {
            NLibraryItem libraryItem = new NLibraryItem();
            libraryItem.Name = nevronMaster.Name;
            novLibrary.Items.Add(libraryItem);

            if (nevronMaster.IconImage != null)
            {
                // Set the library item image
                libraryItem.Image = NDiagramConverter.ToNImage(nevronMaster.IconImage);
            }

            // Convert each master node to a NOV diagram item
            for (int i = 0; i < nevronMaster.Nodes.Length; i++)
            {
                Nevron.Diagram.NDiagramElement nevronDiagramElement = nevronMaster.Nodes[i];
                NPageItem novPageItem = CreatePageItem(nevronDiagramElement);

                if (novPageItem != null)
                {
                    libraryItem.Items.Add(novPageItem);
                    PostProcessPageItem(novPageItem, nevronDiagramElement);
                }
            }

            return libraryItem;
        }
        private void InitializeDefaultNevronStyles()
        {
            Nevron.Diagram.NDrawingDocument nevronDrawingDocument = new Nevron.Diagram.NDrawingDocument();

            m_DefaultNevronFillStyle = nevronDrawingDocument.ComposeFillStyle();
            m_DefaultNevronStrokeStyle = nevronDrawingDocument.ComposeStrokeStyle();
            m_DefaultNevronBeginArrowheadStyle = nevronDrawingDocument.ComposeStartArrowheadStyle();
            m_DefaultNevronEndArrowheadStyle = nevronDrawingDocument.ComposeEndArrowheadStyle();
        }

        #endregion

        #region Fields

        private GraphicsCore.NFillStyle m_DefaultNevronFillStyle;
        private GraphicsCore.NStrokeStyle m_DefaultNevronStrokeStyle;
        private Nevron.Diagram.NArrowheadStyle m_DefaultNevronBeginArrowheadStyle;
        private Nevron.Diagram.NArrowheadStyle m_DefaultNevronEndArrowheadStyle;

        #endregion
    }
}