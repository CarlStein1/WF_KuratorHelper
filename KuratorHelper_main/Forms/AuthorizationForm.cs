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
            var a = $"SELECT tutor_id, last_name, first_name, middle_name FROM teachers WHERE login = \"{guna2TextBox1.Text}\" AND password = \"{guna2TextBox2.Text}\"";
            List<string[]> temp = VoidsMain.SelectRequestAsList($"SELECT tutor_id, last_name, first_name, middle_name FROM teachers WHERE login = \"{guna2TextBox1.Text}\" AND password = \"{guna2TextBox2.Text}\"", MainForm.connectionstring);
            if (temp.Count != 0)
            {
                this.Hide();
                MainForm mnfrm = new MainForm();
                mnfrm.kuratordata = temp[0];
                mnfrm.FormClosed += (s, eargs) => this.Close();
                mnfrm.Show();
            }
            else
                VoidsMain.MessageBoxCustomShow("Ошибка", "Преподаватель не найден!");
        }
    }
}
