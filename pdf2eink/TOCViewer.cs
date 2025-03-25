
namespace pdf2eink
{
    public partial class TOCViewer : Form
    {
        public TOCViewer()
        {
            InitializeComponent();
        }

        TOC WorkTOC;
        internal void Init(TOC toc)
        {
            WorkTOC = toc;
            UpdateList();
        }

        ICbViewer Viewer;
        internal void Init(TOC toc, ICbViewer viewer, bool allowEdit = false)
        {
            Init(toc);
            Viewer = viewer;
            contextMenuStrip1.Enabled = allowEdit;
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in WorkTOC.Items)
            {
                listView1.Items.Add(new ListViewItem(new string[] {
                    item.Header,
                    item.Text,
                    $"{item.Page}",
                    item.Ident.ToString() })
                { Tag = item });
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var s = listView1.SelectedItems[0].Tag as TOCItem;
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddStringField("header", "Header", s.Header);
            d.AddStringField("text", "Text", s.Text);
            d.AddNumericField("page", "Page", s.Page, max: 50000, decimalPlaces: 0);
            d.AddNumericField("ident", "Ident", s.Ident, max: 6, decimalPlaces: 0);
            if (!d.ShowDialog())
                return;

            s.Header = d.GetStringField("header");
            s.Text = d.GetStringField("text");
            s.Page = d.GetIntegerNumericField("page");
            s.Ident = d.GetIntegerNumericField("ident");

            UpdateList();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var s = listView1.SelectedItems[0].Tag as TOCItem;
            WorkTOC.Items.Remove(s);
            UpdateList();

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddStringField("header", "Header");
            d.AddStringField("text", "Text");
            d.AddNumericField("page", "Page", max: 50000, decimalPlaces: 0);
            d.AddNumericField("ident", "Ident", max: 6, decimalPlaces: 0);

            if (d.ShowDialog(this) != DialogResult.OK)
                return;

            TOCItem s = new TOCItem();
            s.Header = d.GetStringField("header");
            s.Text = d.GetStringField("text");
            s.Page = d.GetIntegerNumericField("page");
            s.Ident = d.GetIntegerNumericField("ident");

            WorkTOC.Items.Add(s);
            UpdateList();
        }

        private void multToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("factor", "Factor", max: 8, decimalPlaces: 0);

            if (!d.ShowDialog())
                return;

            var factor = d.GetIntegerNumericField("factor");
            foreach (var item in listView1.SelectedItems)
            {
                ((item as ListViewItem).Tag as TOCItem).Page *= factor;
            }

            UpdateList();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                listView1.MultiSelect = true;
                foreach (ListViewItem item in listView1.Items)
                {
                    item.Selected = true;
                }
            }
        }



        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var s = listView1.SelectedItems[0].Tag as TOCItem;
            Viewer.ShowPage(s.Page - 1);
        }

        private void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var s = listView1.SelectedItems[0].Tag as TOCItem;
            var ind1 = WorkTOC.Items.IndexOf(s);
            if (ind1 > 0)
            {
                WorkTOC.Items.Remove(s);
                WorkTOC.Items.Insert(ind1 - 1, s);
            }
            UpdateList();
        }

        private void downToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var s = listView1.SelectedItems[0].Tag as TOCItem;
            var ind1 = WorkTOC.Items.IndexOf(s);
            if (ind1 < WorkTOC.Items.Count - 1)
            {
                WorkTOC.Items.Remove(s);
                WorkTOC.Items.Insert(ind1 + 1, s);
            }
            UpdateList();
        }
               
        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddIntegerNumericField("add", "Add");

            if (!d.ShowDialog())
                return;

            var add = d.GetIntegerNumericField("add");
            foreach (var item in listView1.SelectedItems)
            {
                ((item as ListViewItem).Tag as TOCItem).Page += add;
            }

            UpdateList();
        }
    }
}
