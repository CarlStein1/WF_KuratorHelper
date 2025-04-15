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
        Dictionary<Guna2TileButton, Guna2CustomGradientPanel> buttonsAndMainPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>();
        Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel> mainPanelsAndPanels = new Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel>();
        List<Guna2TileButton> schedulebuttons;
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

            buttonsAndMainPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>{
                { guna2TileButtonСтуденты, guna2CustomGradientPanelСтуденты},
                { guna2TileButtonАкадемы, guna2CustomGradientPanelАкадемы},
                { guna2TileButtonРасписание, guna2CustomGradientPanelРасписание},
                { guna2TileButtonАдреса, guna2CustomGradientPanelАдреса},
                { guna2TileButtonДокументы, guna2CustomGradientPanelДокументы }
            };
            mainPanelsAndPanels = new Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel>{
                { guna2CustomGradientPanelСтуденты, guna2GradientPanelСтуденты },
                { guna2CustomGradientPanelАкадемы, guna2GradientPanelАкадемы },
                { guna2CustomGradientPanelАдреса, guna2GradientPanelАдреса },
                { guna2CustomGradientPanelДокументы, guna2GradientPanelДокументы }
            };
            schedulebuttons = new List<Guna2TileButton>
            {
                guna2TileButton10, guna2TileButton11
            };
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
        private void UpdateDGVFromDB(CustomGuna2DataGridView dgv = null, string querry = null)
        {
            foreach (Control ctrl1 in buttonsAndMainPanels[currentbutton].Controls)
            {
                if (ctrl1 is Guna2GradientPanel)
                {
                    foreach (Control ctrl2 in ctrl1.Controls)
                    {
                        if (ctrl2 is Guna2GradientPanel)
                        {
                            foreach (Control ctrl3 in ctrl2.Controls)
                            {
                                if (ctrl3 is CustomGuna2DataGridView)
                                {
                                    if (dgv != null)
                                    {
                                        if (!String.IsNullOrEmpty(dgv.Tag.ToString()))
                                        {
                                            dgv.Columns.Clear();
                                            dgv.DataSource = VoidsMain.SelectRequestAsDataTable(string.Format(dgv.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? ""));
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        CustomGuna2DataGridView currentdgv = ctrl3 as CustomGuna2DataGridView;
                                        if (!String.IsNullOrEmpty(currentdgv.Tag.ToString()))
                                        {
                                            currentdgv.Columns.Clear();
                                            currentdgv.DataSource = VoidsMain.SelectRequestAsDataTable(string.Format(currentdgv.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? ""));
                                            currentdgv.Columns[0].ReadOnly = currentdgv.ReadOnlyForeignKey;
                                        }
                                    }
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
                buttonsAndMainPanels[currentbutton].Visible = false;
                
                (currentbutton = gtb).Checked = true;
                buttonsAndMainPanels[gtb].Visible = true;
            }

            UpdateDGVFromDB();
        }

        private void guna2GradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            guna2DragControl1.TargetControl = sender as Control;
        }

        private void guna2TileButton10_Click_1(object sender, EventArgs e)
        {
            Guna2TileButton gtb = sender as Guna2TileButton;
            
            foreach (Guna2TileButton gunatb in schedulebuttons)
                gunatb.Checked = false;
            gtb.Checked = true;
            guna2DataGridViewРасписание2.Tag = gtb.Tag;
            UpdateDGVFromDB(guna2DataGridViewРасписание2);
        }

        private void guna2DataGridViewСтуденты_SelectionChanged(object sender, EventArgs e)
        {
            if ((sender as CustomGuna2DataGridView).SelectedRows.Count > 0)
            {

                foreach (Control ctrl1 in mainPanelsAndPanels[buttonsAndMainPanels[currentbutton]].Controls)
                {
                    if (ctrl1 is Guna2HtmlLabel)
                    {
                        if ((ctrl1 as Guna2HtmlLabel).Tag != null)
                        {
                            if ((ctrl1 as Guna2HtmlLabel).AccessibleDescription == null)
                            {
                                (ctrl1 as Guna2HtmlLabel).AccessibleDescription = "";
                            }
                            ctrl1.Text = ctrl1.AccessibleDescription.ToString() + " " + VoidsMain.SelectRequestAsList(string.Format(ctrl1.Tag.ToString(), (sender as CustomGuna2DataGridView).SelectedRows[0].Cells[0].Value))[0][0];
                        }
                    }
                }
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            //CustomGuna2DataGridView dgv = currentpanel.Controls.OfType<CustomGuna2DataGridView>().First();

            //Guna2TextBox txb = null;
            //Guna2ComboBox combobox = currentpanel.Controls.OfType<Guna2ComboBox>().FirstOrDefault();

            //// Определяем, откуда пришел вызов
            //if (sender is Guna2TextBox)
            //{
            //    txb = sender as Guna2TextBox;
            //}
            //else if (sender is Guna2ComboBox)
            //{
            //    combobox = sender as Guna2ComboBox;
            //    txb = currentpanel.Controls.OfType<Guna2TextBox>().FirstOrDefault();
            //}

            //if (dgv.DataSource is DataTable table)
            //{
            //    string filterExpression = string.Empty;

            //    // Фильтрация по текстовому полю
            //    if (!string.IsNullOrEmpty(txb.Text))
            //    {
            //        foreach (DataColumn col in table.Columns)
            //        {

            //            filterExpression += $"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{txb.Text}%' OR ";
            //        }
            //        filterExpression = filterExpression.TrimEnd(" OR ".ToCharArray());
            //    }

            //    // Фильтрация по ComboBox
            //    if (combobox != null && combobox.SelectedItem.ToString() != "Все")
            //    {
            //        string comboboxTagTemp = VoidsMain.columnheadertexts[combobox.Tag.ToString()];
            //        string comboFilter = $"[{comboboxTagTemp}] = '{combobox.SelectedItem}'";
            //        filterExpression = string.IsNullOrEmpty(filterExpression) ? comboFilter : $"{comboFilter} AND ({filterExpression})";
            //    }

            //    // Применение фильтрации
            //    (table.DefaultView).RowFilter = filterExpression;
            //}
        }
    }
}
