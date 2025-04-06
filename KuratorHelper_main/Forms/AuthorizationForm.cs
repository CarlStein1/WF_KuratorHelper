using System;
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
    public partial class AuthorizationForm : Form
    {
        public AuthorizationForm()
        {
            InitializeComponent();
        }

        private void guna2TileButtonВыход_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainForm mnfrm = new MainForm();
            mnfrm.FormClosed += (s, eargs) => this.Close();
            mnfrm.Show();
        }
    }
}
