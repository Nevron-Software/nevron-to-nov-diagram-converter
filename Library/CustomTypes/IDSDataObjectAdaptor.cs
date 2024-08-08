using System.Windows.Forms;

namespace Nevron.Nov.Diagram.Converter
{
    public class IDSDataObjectAdaptor : Nevron.Diagram.NDataObjectAdaptor
	{
		public override object Adapt(IDataObject dataObject)
		{
			return null;
		}

		public override bool CanAdapt(IDataObject dataObject)
		{
			return false;
		}
	}
}