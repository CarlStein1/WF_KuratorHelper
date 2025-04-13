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
        internal string[] kuratordata;

        /* В ТАГ ТАБЛИЦ ВПИСАТЬ ЗАПРОС НА ВСТАВКУ ДАННЫХ В ЭТИ ТАБЛИЦЫ
         * В ТАГ КНОПОК ВПИСАТЬ НАЗВАНИЕ ТАБЛИЦЫ В БД
         * ДОБАВЛЕНИЕ И УДАЛЕНИЕ ЧЕРЕЗ КНОПКИ НА ПАНЕЛЕ СВЕРХУ
         * ИЗМЕНЕНИЕ И ДРУГИЕ ФУНКЦИИ(ПО ТИПУ ОТПРАВКА СТУДЕНТА В АКАДЕМ) ЧЕРЕЗ КОНТЕКСТНОЕ МЕНЮ ПРИ НАЖАТИИ ПКМ
         * НЕДЕЛИ РАСПИСАНИИ РЕАЛИЗОВАТЬ ЧЕРЕЗ 2 РАЗНЫЕ ТАБЛИЦЫ: ВЕРХНЯЯ И НИЖНЯЯ НЕДЕЛЯ
         * СДЕЛАТЬ ТАК, ЧТОБЫ АДРЕСА МОЖНО БЫЛО ПРИКРЕПЛЯТЬ К СТУДЕНТАМ ВО ВКЛАДКАХ АДРЕСА, КАК И ДОКУМЕНТЫ*/
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
            guna2DataGridView2.Rows.Add("Английский\nЛузина И.А");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            currentbutton = guna2TileButtonСтуденты;

            List<string[]> temp = VoidsMain.SelectRequestAsList($"SELECT group_name FROM groups WHERE tutor_id = {kuratordata[0]}");
            for (int i = 0; i < temp.Count; i++)
            {
                guna2ComboBox1.Items.Add(temp[i][0]);
            }
            guna2ComboBox1.SelectedIndex = 0;

            UpdateDGVFromDB();
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var a = $"SELECT student_id, last_name, first_name, middle_name, gender, birth_date, status FROM students WHERE group_name = \"{guna2ComboBox1.SelectedItem}\"";
        }

        // Обновление DGV из БД
        private void UpdateDGVFromDB()
        {
            Guna2DataGridView dgv;
            foreach (Control ctrl1 in buttonsAndPanels[currentbutton].Controls)
            {
                if (ctrl1 is Guna2GradientPanel)
                {
                    foreach (Control ctrl2 in ctrl1.Controls)
                    {
                        if (ctrl2 is Guna2GradientPanel)
                        {
                            foreach (Control ctrl3 in ctrl2.Controls)
                            {
                                if (ctrl3 is Guna2DataGridView)
                                {
                                    dgv = ctrl3 as Guna2DataGridView;
                                    if (dgv.Tag != null)
                                        dgv.Columns.Clear();
                                        dgv.DataSource = VoidsMain.SelectRequestAsDataTable(string.Format(dgv.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? ""));
                                }
                            }
                        }
                    }
                }
            }
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

            UpdateDGVFromDB();
        }

        private void guna2GradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            guna2DragControl1.TargetControl = sender as Control;
        }

        private void guna2HScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //guna2DataGridView1.scroll = guna2HScrollBar1;
        }
    }
}
