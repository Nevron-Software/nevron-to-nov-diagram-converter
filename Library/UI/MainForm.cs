using System.Windows.Forms;

namespace Nevron.Nov.Diagram.Converter
{
    internal partial class MainForm : Form
    {
        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            TabControl tabControl = new TabControl();
            tabControl.Font = new System.Drawing.Font(Font.FontFamily, Font.Size * 2);
            tabControl.Dock = DockStyle.Fill;
            Controls.Add(tabControl);

            AddTabPage(tabControl, ENDocType.Drawing);
            AddTabPage(tabControl, ENDocType.Library);
        }

        #endregion

        #region Implementation - UI

        private void AddTabPage(TabControl tabControl, ENDocType docType)
        {
            string docTypeStr = NEnum.GetLocalizedString(docType);

            TabPage tabPage = new TabPage(docTypeStr);
            tabPage.Font = new System.Drawing.Font(Font.FontFamily, Font.Size * 1.2f);
            tabControl.TabPages.Add(tabPage);

            NConversionControl conversionControl = NConversionControl.Create(docType);
            tabPage.Controls.Add(conversionControl);
        }

        #endregion
    }
}