using System;

namespace Nevron.Nov.Diagram.Converter
{
	internal static class NPortsImporter
	{
		#region Public Methods

		public static NPortCollection ToPorts(Nevron.Diagram.NPortCollection nevronPorts)
		{
			if (nevronPorts == null)
				return null;

			Nevron.Dom.NNodeList nevronPortsList = nevronPorts.Children(null);
			NPortCollection novPorts = new NPortCollection();

			for (int i = 0; i < nevronPortsList.Count; i++)
			{
				NPort port = CreatePort((Nevron.Diagram.NPort)nevronPortsList[i]);
				if (port != null)
				{
					novPorts.Add(port);
				}
			}

			return novPorts;
		}

		#endregion

		#region Implementation

		private static NPort CreatePort(Nevron.Diagram.NPort nevronPort)
		{
			NPort novPort = new NPort();
			novPort.Name = nevronPort.Name;

			if (nevronPort is Nevron.Diagram.NBoundsPort ||
				nevronPort is Nevron.Diagram.NRotatedBoundsPort)
			{
				Nevron.Diagram.NContentAlignment alignment;
				if (nevronPort is Nevron.Diagram.NBoundsPort boundsPort)
				{
					alignment = boundsPort.Alignment;

					float directionAngle = 0;
					if (boundsPort.GetDirection(ref directionAngle))
					{
						// Set port direction
						novPort.SetDirection(directionAngle);
					}
				}
				else
				{
					Nevron.Diagram.NRotatedBoundsPort rotatedBoundsPort = (Nevron.Diagram.NRotatedBoundsPort)nevronPort;
					alignment = rotatedBoundsPort.Alignment;

					float directionAngle = 0;
					if (rotatedBoundsPort.GetDirection(ref directionAngle))
					{
						// Set port direction
						novPort.SetDirection(directionAngle);
					}
				}

				// Set port location
				novPort.Relative = true;
				novPort.X = alignment.PercentX / 100 + 0.5;
				novPort.Y = alignment.PercentY / 100 + 0.5;
			}
			else
			{
				NDebug.Assert(false, "Unsupported Nevron port");
				return null;
			}

			// Set port glue mode
			novPort.GlueMode = ToPortGlueMode(nevronPort.Type);

			return novPort;
		}

		#endregion

		#region Conversions

		private static ENPortGlueMode ToPortGlueMode(Nevron.Diagram.PortType portType)
		{
			switch (portType)
			{
				case Nevron.Diagram.PortType.Inward:
					return ENPortGlueMode.Inward;
				case Nevron.Diagram.PortType.Outward:
					return ENPortGlueMode.Outward;
				case Nevron.Diagram.PortType.InwardAndOutward:
					return ENPortGlueMode.InwardAndOutward;
				default:
					NDebug.Assert(false, "New Nevron PortType?");
					return default(ENPortGlueMode);
			}
		}

		#endregion
	}
}