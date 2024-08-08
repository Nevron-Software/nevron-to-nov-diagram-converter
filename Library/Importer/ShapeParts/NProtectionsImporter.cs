namespace Nevron.Nov.Diagram.Converter
{
	internal static class NProtectionsImporter
	{
		public static void ImportProtections(NShape shape, Nevron.Diagram.NAbilities protection)
		{
			shape.AllowChangeAspectRatio = !protection.ChangeAspectRatio;
			shape.AllowChangeBeginPoint = !protection.ChangeStartPoint;
			shape.AllowChangeEndPoint = !protection.ChangeEndPoint;
			shape.AllowFormat = !protection.ChangeStyle;
			shape.AllowContextMenuEdit = !protection.ContextMenuEdit;
			shape.AllowDelete = !protection.Delete;
			shape.AllowInplaceEdit = !protection.InplaceEdit;
			shape.AllowMoveX = !protection.MoveX;
			shape.AllowMoveY = !protection.MoveY;
			shape.AllowPrint = !protection.Print;
			shape.AllowResizeX = !protection.ResizeX;
			shape.AllowResizeY = !protection.ResizeY;
			shape.AllowRotate = !protection.Rotate;
			shape.AllowSelect = !protection.Select;
			shape.AllowGeometryEdit = !protection.TrackersEdit;

			if (shape is NGroup)
			{
				((NGroup)shape).AllowUngroup = !protection.Ungroup;
			}
		}
	}
}