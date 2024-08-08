using System;
using Nevron.GraphicsCore;
using Nevron.Nov.UI;

namespace Nevron.Nov.Diagram.Converter
{
    internal class NInteractivityStyleImporter : NStyleImporter
    {
        #region Public Methods

        public static void Import(NShape novShape, NInteractivityStyle interactivityStyle)
        {
            if (interactivityStyle == null)
                return;

            // Import tooltip
            NTooltip tooltip = ToTooltip(interactivityStyle.Tooltip);
            if (tooltip != null)
            {
                novShape.Tooltip = tooltip;
            }

			// Import cursor
			Nov.UI.NCursor cursor = ToCursor(interactivityStyle.Cursor);
            if (cursor != null)
            {
                novShape.Cursor = cursor;
            }
        }

        #endregion

        #region Implementation

        private static NTooltip ToTooltip(NTooltipAttribute tooltipAttribute)
        {
            if (tooltipAttribute == null || String.IsNullOrEmpty(tooltipAttribute.Text))
                return null;

            return new NTooltip(tooltipAttribute.Text);
        }
        private static Nov.UI.NCursor ToCursor(NCursorAttribute cursorAttribute)
        {
            if (cursorAttribute == null)
                return null;

			ENPredefinedCursor predefinedCursor;
            switch (cursorAttribute.Type)
            {
                case CursorType.Default:
                case CursorType.Alias:
                case CursorType.Cell:
                case CursorType.Copy:
                case CursorType.NoDrop:
                case CursorType.VText:
                case CursorType.Custom:
                    return null;
                case CursorType.AppStarting:
                    predefinedCursor = ENPredefinedCursor.AppStarting;
                    break;
                case CursorType.Arrow:
                    predefinedCursor = ENPredefinedCursor.Arrow;
                    break;
                case CursorType.Cross:
                    predefinedCursor = ENPredefinedCursor.Cross;
                    break;
                case CursorType.Hand:
                    predefinedCursor = ENPredefinedCursor.Hand;
                    break;
                case CursorType.Help:
                    predefinedCursor = ENPredefinedCursor.Help;
                    break;
                case CursorType.HSplit:
                    predefinedCursor = ENPredefinedCursor.HSplit;
                    break;
                case CursorType.IBeam:
                    predefinedCursor = ENPredefinedCursor.IBeam;
                    break;
                case CursorType.No:
                    predefinedCursor = ENPredefinedCursor.No;
                    break;
                case CursorType.NoMove2D:
                    predefinedCursor = ENPredefinedCursor.NoMove2D;
                    break;
                case CursorType.NoMoveHoriz:
                    predefinedCursor = ENPredefinedCursor.NoMoveH;
                    break;
                case CursorType.NoMoveVert:
                    predefinedCursor = ENPredefinedCursor.NoMoveV;
                    break;
                case CursorType.PanEast:
                    predefinedCursor = ENPredefinedCursor.PanEast;
                    break;
                case CursorType.PanNE:
                    predefinedCursor = ENPredefinedCursor.PanNorthEast;
                    break;
                case CursorType.PanNorth:
                    predefinedCursor = ENPredefinedCursor.PanNorth;
                    break;
                case CursorType.PanNW:
                    predefinedCursor = ENPredefinedCursor.PanNorthWest;
                    break;
                case CursorType.PanSE:
                    predefinedCursor = ENPredefinedCursor.PanSouthEast;
                    break;
                case CursorType.PanSouth:
                    predefinedCursor = ENPredefinedCursor.PanSouth;
                    break;
                case CursorType.PanSW:
                    predefinedCursor = ENPredefinedCursor.PanSouthWest;
                    break;
                case CursorType.PanWest:
                    predefinedCursor = ENPredefinedCursor.PanWest;
                    break;
                case CursorType.SizeAll:
                    predefinedCursor = ENPredefinedCursor.SizeAll;
                    break;
                case CursorType.SizeNESW:
                    predefinedCursor = ENPredefinedCursor.SizeNESW;
                    break;
                case CursorType.SizeNS:
                    predefinedCursor = ENPredefinedCursor.SizeNS;
                    break;
                case CursorType.SizeNWSE:
                    predefinedCursor = ENPredefinedCursor.SizeNWSE;
                    break;
                case CursorType.SizeWE:
                    predefinedCursor = ENPredefinedCursor.SizeWE;
                    break;
                case CursorType.UpArrow:
                    predefinedCursor = ENPredefinedCursor.UpArrow;
                    break;
                case CursorType.VSplit:
                    predefinedCursor = ENPredefinedCursor.VSplit;
                    break;
                case CursorType.WaitCursor:
                    predefinedCursor = ENPredefinedCursor.Wait;
                    break;
                default:
                    NDebug.Assert(false, "New CursorType?");
                    return null;
            }

            return new Nov.UI.NCursor(predefinedCursor);
        }

        #endregion
    }
}