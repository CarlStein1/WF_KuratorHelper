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
        public const string connectionstring = "server=localhost;port=3306;database=kuratorhelper;user=root;"; // Строка подключения к базе данных MySQL
        Dictionary<Guna2TileButton, Guna2CustomGradientPanel> buttonsAndMainPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>();
        Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel> mainPanelsAndPanels = new Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel>();
        Dictionary<Guna2TileButton, List<CustomGuna2DataGridView>> buttonsAndDGV = new Dictionary<Guna2TileButton, List<CustomGuna2DataGridView>>();
        Dictionary<int, DataGridViewRow> addingRowsDict = new Dictionary<int, DataGridViewRow>(); // Словарь для записи временных строк, которые после можно будет внести в таблицу
        string lastcellvalue; // Для записи первого значения редактируемой ячейки в DGV
        string primarykeyvalue; // Для записи первого значения редактируемой ячейки в DGV
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
            buttonsAndDGV = new Dictionary<Guna2TileButton, List<CustomGuna2DataGridView>>
            {
                { guna2TileButtonСтуденты, new List<CustomGuna2DataGridView> { guna2DataGridViewСтуденты } },
                { guna2TileButtonАкадемы, new List<CustomGuna2DataGridView> { guna2DataGridViewАкадемы } },
                { guna2TileButtonРасписание, new List<CustomGuna2DataGridView> { guna2DataGridViewРасписание1, guna2DataGridViewРасписание2 } },
                { guna2TileButtonАдреса, new List<CustomGuna2DataGridView> { guna2DataGridViewАдреса } },
                { guna2TileButtonДокументы, new List<CustomGuna2DataGridView> { guna2DataGridViewДокументы } }
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            currentbutton = guna2TileButtonСтуденты;

            List<string[]> temp = VoidsMain.SelectRequestAsList($"SELECT group_name FROM groups WHERE tutor_id = {kuratordata[0]}", connectionstring);
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
        private void UpdateDGVFromDB(int numbers = -1)
        {
            foreach (List<CustomGuna2DataGridView> list in buttonsAndDGV.Values)
            {
                if(numbers == -1)
                {
                    foreach (CustomGuna2DataGridView dgv in buttonsAndDGV[currentbutton])
                    {
                        dgv.QuerySelect = string.Format(dgv.QuerySelect, guna2ComboBox1.SelectedItem?.ToString() ?? "");
                        dgv.QuerySelectCommand.ExecuteQuery(connectionstring);

                        foreach (DataGridViewColumn col in dgv.Columns)
                            if (col.ValueType == typeof(DateTime))
                            {
                                col.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                                col.ReadOnly = true;
                            }
                    }
                }
                else
                {
                    buttonsAndDGV[currentbutton][numbers].QuerySelect = string.Format(buttonsAndDGV[currentbutton][numbers].QuerySelect, guna2ComboBox1.SelectedItem?.ToString() ?? "");
                    buttonsAndDGV[currentbutton][numbers].QuerySelectCommand.ExecuteQuery(connectionstring);

                    foreach (DataGridViewColumn col in buttonsAndDGV[currentbutton][numbers].Columns)
                        if (col.ValueType == typeof(DateTime))
                        {
                            col.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            col.ReadOnly = true;
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

            guna2TileButton10.Checked = guna2TileButton11.Checked = false;
            gtb.Checked = true;
            guna2DataGridViewРасписание2.QuerySelect = string.Format(gtb.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? "");
            UpdateDGVFromDB(1);
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
                            ctrl1.Text = ctrl1.AccessibleDescription.ToString() + " " + VoidsMain.SelectRequestAsList(string.Format(ctrl1.Tag.ToString(), (sender as CustomGuna2DataGridView).SelectedRows[0].Cells[1].Value), connectionstring)[0][0];
                        }
                    }
                }
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            CustomGuna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            Guna2TextBox txb = sender as Guna2TextBox;

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

            if (dgv.DataSource is DataTable table)
            {
                string filterExpression = string.Empty;

                // Фильтрация по текстовому полю
                if (!string.IsNullOrEmpty(txb.Text))
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        filterExpression += $"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{txb.Text}%' OR ";
                    }
                    filterExpression = filterExpression.TrimEnd(" OR ".ToCharArray());
                }

                //// Фильтрация по ComboBox
                //if (combobox != null && combobox.SelectedItem.ToString() != "Все")
                //{
                //    string comboboxTagTemp = VoidsMain.columnheadertexts[combobox.Tag.ToString()];
                //    string comboFilter = $"[{comboboxTagTemp}] = '{combobox.SelectedItem}'";
                //    filterExpression = string.IsNullOrEmpty(filterExpression) ? comboFilter : $"{comboFilter} AND ({filterExpression})";
                //}

                // Применение фильтрации
                (table.DefaultView).RowFilter = filterExpression;
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridViewСтуденты_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Проверка индексации, чтобы не было ошибок
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                CustomGuna2DataGridView dgv = sender as CustomGuna2DataGridView;

                // Проверка на изменение данных во временных строчках, которые не нужно применять, пока администратор не нажал на "Применить изменения"
                if (addingRowsDict.Values.Contains(dgv.CurrentRow) == true)
                {
                    return;
                }

                string primarykey = dgv.Columns[0].HeaderCell.Value.ToString();
                string changingcell = dgv.Columns[e.ColumnIndex].HeaderCell.Value.ToString();

                if (VoidsMain.columnheadertexts.Values.Contains(primarykey))
                    primarykey = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == primarykey).Key;
                if (VoidsMain.columnheadertexts.Values.Contains(changingcell))
                    changingcell = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == changingcell).Key;

                string querrytext = string.Empty;
                switch (dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) // Проверка на соответствие типов данных, приведение их к читабельному варианту для БД
                {
                    case true:
                        querrytext = "1";
                        break;
                    case false:
                        querrytext = "0";
                        break;
                    default:
                        querrytext = "\'" + dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() + "\'";
                        break;
                }

                //Применение изменений в БД
                try
                {
                    dgv.QueryUpdate = ($@"UPDATE {dgv.Tag} SET {changingcell} = {querrytext} WHERE {primarykey} = '{dgv.Rows[e.RowIndex].Cells[0].Value}'");
                    dgv.QueryUpdateCommand.ExecuteQuery();
                    UpdateDGVFromDB();
                }
                catch
                {
                    VoidsMain.MessageBoxCustomShow("Ошибка запроса", "Невозможно выполнить запрос на изменение. Возможно введенное сообщение не соответствует формату!");
                    dgv.CurrentCell.Value = lastcellvalue; // Возвращение изначального значения клетки
                    return;
                }
            }
        }

        private void guna2DataGridViewСтуденты_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 0)
                primarykeyvalue = (sender as Guna2DataGridView).CurrentCell.Value.ToString();
            lastcellvalue = (sender as Guna2DataGridView).CurrentCell.Value.ToString();
        }

        private void guna2DataGridViewСтуденты_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            lastcellvalue = null;
        }

        private void guna2DataGridViewСтуденты_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0) // Проверяем, что это правая кнопка и строка
            {
                if ((sender as Guna2DataGridView).SelectedRows.Count < 3) (sender as Guna2DataGridView).ClearSelection();
                (sender as Guna2DataGridView).Rows[e.RowIndex].Selected = true; // Выделяем строку
                guna2ContextMenuStrip1.Show(Cursor.Position); // Показываем контекстное меню в месте курсора
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {

        }
    }
}
