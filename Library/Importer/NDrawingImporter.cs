namespace Nevron.Nov.Diagram.Converter
{
	internal class NDrawingImporter : NDiagramImporter
    {
        #region Public Methods

        /// <summary>
        /// Creates a NOV Diagram drawing document from the given Nevron Diagram drawing document.
        /// </summary>
        /// <param name="drawingDocument"></param>
        /// <returns></returns>
        public NDrawingDocument Import(Nevron.Diagram.NDrawingDocument drawingDocument)
        {
            Initialize();

            NDrawingDocument novDrawingDocument = new NDrawingDocument();

            // Create NOV Diagram page
            NPage page = novDrawingDocument.Content.ActivePage;
            page.Bounds = NDiagramConverter.ToNRectangle(drawingDocument.Bounds);

            if (drawingDocument.BackgroundStyle != null)
            {
                page.BackgroundFill = NFillStyleImporter.ToFill(drawingDocument.BackgroundStyle.FillStyle);
            }

            // Get the Nevron drawing document layers
            Nevron.Dom.NNodeList layers = drawingDocument.Layers.Children(null);

            // Pass 1:
            // Loop through the layers of the Nevron drawing document and add them to the NOV diagram page
            for (int i = 0; i < layers.Count; i++)
            {
                Nevron.Diagram.NLayer nevronLayer = (Nevron.Diagram.NLayer)layers[i];
                ImportLayer(page, nevronLayer);
            }

            // Pass 2:
            // Loop through the layers of the Nevron drawing document and post process the corresponding NOV shapes
            for (int i = 0; i < layers.Count; i++)
            {
                Nevron.Diagram.NLayer nevronLayer = (Nevron.Diagram.NLayer)layers[i];
                Nevron.Dom.NNodeList nevronNodes = nevronLayer.Children(null);

                // Post process the nodes of the Nevron layer
                for (int j = 0; j < nevronNodes.Count; j++)
                {
                    Nevron.Dom.INNode nevronNode = nevronNodes[j];

                    NPageItem novPageItem;
                    if (TryGetNovPageItem(nevronNode, out novPageItem))
                    {
                        PostProcessPageItem(novPageItem, nevronNode);
                    }
                }
            }

            // Pass 3:
            // Connect the shapes
            Nevron.Dom.NNodeList nevronConnectors = drawingDocument.Descendants(Nevron.Diagram.Filters.NFilters.Shape1D, -1);
            for (int i = 0; i < nevronConnectors.Count; i++)
            {
                Nevron.Diagram.NShape nevronConnector = (Nevron.Diagram.NShape)nevronConnectors[i];

                NPageItem novConnector;
                if (TryGetNovPageItem(nevronConnector, out novConnector))
                {
                    // Connect the connector to its targets
                    Connect((NShape)novConnector, nevronConnector);
                }
            }

            return novDrawingDocument;
        }

        #endregion

        #region Implementation - Layers and Page Items

        private void ImportLayer(NPage novPage, Nevron.Diagram.NLayer nevronLayer)
        {
            if (!nevronLayer.Visible)
                return;

            // Get the nodes of the layer
            Nevron.Dom.NNodeList nevronNodes = nevronLayer.Children(null);

            // Loop through the nodes of the Nevron drawing document and convert them to page items
            for (int i = 0; i < nevronNodes.Count; i++)
            {
                Nevron.Dom.INNode nevronNode = nevronNodes[i];
                NPageItem novPageItem = CreatePageItem(nevronNode);

                if (novPageItem != null)
                {
                    if (nevronNode is Nevron.Diagram.NDiagramElement nevronDiagramElement)
                    {
                        // Set the name and the user ID of the NOV shape
                        novPageItem.Name = nevronDiagramElement.Name;
                        novPageItem.UserId = nevronDiagramElement.UniqueId.ToString();

                        if (nevronDiagramElement.Tag != null)
                        {
                            // Set the Tag of the NOV shape
                            novPageItem.Tag = nevronDiagramElement.Tag;
                        }
                    }

                    novPage.Items.Add(novPageItem);
                    AddPageItem(nevronNode, novPageItem);
                }
            }
        }

        #endregion
	}
}