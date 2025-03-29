using Guna.UI2.WinForms;
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
    public partial class MainForm : Form
    {
        Dictionary<Guna2TileButton, Guna2CustomGradientPanel> buttonsAndPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>();
        Guna2TileButton currentbutton;
        public MainForm()
        {
            InitializeComponent();
            buttonsAndPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>{
                { guna2TileButtonСтуденты, guna2CustomGradientPanelСтуденты},
                { guna2TileButtonАкадемы, guna2CustomGradientPanelАкадемы},
                { guna2TileButtonРасписание, guna2CustomGradientPanelРасписание},
                { guna2TileButtonАдреса, guna2CustomGradientPanelАдреса},
                { guna2TileButtonДокументы, guna2CustomGradientPanelДокументы }
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            currentbutton = guna2TileButtonСтуденты;
        }

        private void guna2TileButton3_Click_1(object sender, EventArgs e)
        {
            Guna2TileButton gtb = sender as Guna2TileButton;

            if (!gtb.Checked)
            {
                currentbutton.Checked = false;
                buttonsAndPanels[currentbutton].Visible = false;
                
                (currentbutton = gtb).Checked = true;
                buttonsAndPanels[gtb].Visible = true;
            }
        }

        private void guna2GradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            guna2DragControl1.TargetControl = sender as Control;
        }
    }
}
