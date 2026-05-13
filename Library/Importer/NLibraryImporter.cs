using Nevron.Nov.Graphics;

namespace Nevron.Nov.Diagram.Converter
{
    internal class NLibraryImporter : NDiagramImporter
    {
        #region Public Methods - Import

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

		#region Styles

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

        private void InitializeDefaultNevronStyles()
        {
            Nevron.Diagram.NDrawingDocument nevronDrawingDocument = new Nevron.Diagram.NDrawingDocument();

            m_DefaultNevronFillStyle = nevronDrawingDocument.ComposeFillStyle();
            m_DefaultNevronStrokeStyle = nevronDrawingDocument.ComposeStrokeStyle();
            m_DefaultNevronBeginArrowheadStyle = nevronDrawingDocument.ComposeStartArrowheadStyle();
            m_DefaultNevronEndArrowheadStyle = nevronDrawingDocument.ComposeEndArrowheadStyle();
        }

		#endregion

		#region Page Items

		protected override NPage GetOwnerPage(NShape novShape, Nevron.Diagram.NModel nevronModel)
		{
			Nevron.Diagram.NMaster nevronMaster = GetOwnerMaster(nevronModel);

			NUnit novUnit;
			if (nevronMaster != null &&
				NDiagramConverter.TryConvertUnit(nevronMaster.MeasurementUnit, out novUnit) &&
				novUnit.Dimension == ENUnitDimension.Length)
			{
				// Create a dummy page just for scaling
				NPage novPage = new NPage();
				novPage.DisplayLength = new NLength(1, novUnit);
				return novPage;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Transform

		protected override void ImportTransform1D(NShape novShape, Nevron.Diagram.NModel nevronModel, NPage novPage)
		{
			base.ImportTransform1D(novShape, nevronModel, novPage);

			if (!(nevronModel is Nevron.Diagram.NLineShape))
			{
				NMatrix novParentPageTransform = GetNovParentShapePageTransform(novShape, nevronModel, novPage);
				SetAngle(novShape, novParentPageTransform, nevronModel);
			}
		}
		protected override NMatrix GetNovParentShapePageTransform(NShape novShape, Nevron.Diagram.NModel nevronModel, NPage novPage)
		{
			NMatrix matrix = base.GetNovParentShapePageTransform(novShape, nevronModel, novPage);

			// If the Nevron model is in a composite shape or a group, translate the matrix with the owner shape's location,
			// because the inner shape's pin point expressions will get wrong otherwise.
			Nevron.Diagram.NShape ownerCompositeShapeOrGroup = GetOwnerCompositeShapeOrGroup(nevronModel);
			if (ownerCompositeShapeOrGroup != null)
			{
				NPoint location = NDiagramConverter.ToNPoint(novPage, ownerCompositeShapeOrGroup.Location);
				matrix.Translate(location.X, location.Y);
			}

			return matrix;
		}

		/// <summary>
		/// Gets the owner composite shape (if any) of the given Nevron model.
		/// </summary>
		/// <param name="nevronModel"></param>
		/// <returns></returns>
		private static Nevron.Diagram.NShape GetOwnerCompositeShapeOrGroup(Nevron.Diagram.NModel nevronModel)
		{
			Nevron.Dom.INNode nevronNode = nevronModel;
			while (nevronNode != null)
			{
				nevronNode = nevronNode.ParentNode;
				if (nevronNode is Nevron.Diagram.NCompositeShape nevronCompositeShape)
					return nevronCompositeShape;
				else if (nevronNode is Nevron.Diagram.NGroup nevronGroup)
					return nevronGroup;
			}

			return null;
		}
		/// <summary>
		/// Gets the master that owns the given model.
		/// </summary>
		/// <param name="nevronModel"></param>
		/// <returns></returns>
		private static Nevron.Diagram.NMaster GetOwnerMaster(Nevron.Diagram.NModel nevronModel)
		{
			Nevron.Dom.INNode nevronNode = nevronModel;
			while (nevronNode != null)
			{
				if (nevronNode is Nevron.Diagram.NMaster nevronMaster)
					return nevronMaster;

				nevronNode = nevronNode.ParentNode;
			}

			return null;
		}

		#endregion

		#region Masters

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

        #endregion

        #region Fields

        private GraphicsCore.NFillStyle m_DefaultNevronFillStyle;
        private GraphicsCore.NStrokeStyle m_DefaultNevronStrokeStyle;
        private Nevron.Diagram.NArrowheadStyle m_DefaultNevronBeginArrowheadStyle;
        private Nevron.Diagram.NArrowheadStyle m_DefaultNevronEndArrowheadStyle;

        #endregion
    }
}