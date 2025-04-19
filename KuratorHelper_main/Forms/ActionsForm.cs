using Guna.UI2.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuratorHelper_main
{
    public partial class ActionsForm : Form
    {
        public string query;
        public string value;
        public object[] addition;
        public ActionsForm()
        {
            InitializeComponent();
        }

        private void ActionsForm_Load(object sender, EventArgs e)
        {
            guna2DataGridView1.DataSource = VoidsMain.SelectRequestAsDataTable(string.Format(query,addition));
            foreach (DataGridViewColumn dgvc in guna2DataGridView1.Columns)
                if (VoidsMain.columnheadertexts.Values.Contains(dgvc.HeaderText))
                    dgvc.HeaderText = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == dgvc.HeaderText).Key;
        }

        private void guna2DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            value = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void guna2ControlBox4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
