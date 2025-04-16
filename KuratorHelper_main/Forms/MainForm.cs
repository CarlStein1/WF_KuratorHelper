using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
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
        Guna2TileButton currentconfirmbutton;
        DataGridViewCell currentcell;
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
                                col.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
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
                        if (addingRowsDict.Values.Contains((sender as CustomGuna2DataGridView).CurrentRow))
                        {
                            ctrl1.Text = string.Empty;
                            continue;
                        }
                        if ((ctrl1 as Guna2HtmlLabel).Tag != null)
                        {
                            if ((ctrl1 as Guna2HtmlLabel).AccessibleDescription == null)
                            {
                                (ctrl1 as Guna2HtmlLabel).AccessibleDescription = string.Empty;
                            }
                            string tempdata = "";
                            try
                            {
                                tempdata = VoidsMain.SelectRequestAsList(string.Format(ctrl1.Tag.ToString(), (sender as CustomGuna2DataGridView).SelectedRows[0].Cells[1].Value), connectionstring)[0][0];
                            }
                            catch
                            {
                                tempdata = "Не назначено";
                            }
                            ctrl1.Text = ctrl1.AccessibleDescription.ToString() + " " + tempdata;
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
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                CustomGuna2DataGridView dgv = sender as CustomGuna2DataGridView;

                if (addingRowsDict.Values.Contains(dgv.Rows[e.RowIndex]))
                    return;

                string primaryKey = dgv.Columns[0].HeaderCell.Value.ToString();
                string changingColumn = dgv.Columns[e.ColumnIndex].HeaderCell.Value.ToString();

                if (VoidsMain.columnheadertexts.Values.Contains(primaryKey))
                    primaryKey = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == primaryKey).Key;
                if (VoidsMain.columnheadertexts.Values.Contains(changingColumn))
                    changingColumn = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == changingColumn).Key;

                object value = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                string queryValue;

                if (value == null || value == DBNull.Value)
                {
                    queryValue = "NULL";
                }
                else
                {
                    Type columnType = dgv.Columns[e.ColumnIndex].ValueType;

                    if (columnType == typeof(bool))
                    {
                        queryValue = (bool)value ? "1" : "0";
                    }
                    else if (columnType == typeof(DateTime))
                    {
                        queryValue = $"'{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss")}'";
                    }
                    else
                    {
                        queryValue = $"'{MySqlHelper.EscapeString(value.ToString())}'";
                    }
                }

                try
                {
                    dgv.QueryUpdate = $@"UPDATE {dgv.Tag} 
                                 SET {changingColumn} = {queryValue} 
                                 WHERE {primaryKey} = '{MySqlHelper.EscapeString(dgv.Rows[e.RowIndex].Cells[0].Value?.ToString())}'";

                    dgv.QueryUpdateCommand.ExecuteQuery();
                    UpdateDGVFromDB();
                }
                catch
                {
                    VoidsMain.MessageBoxCustomShow("Ошибка запроса", "Невозможно выполнить запрос на изменение. Возможно введенное значение не соответствует формату!");
                    dgv.CurrentCell.Value = lastcellvalue;
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
                if ((sender as Guna2DataGridView).SelectedRows.Count < 2) (sender as Guna2DataGridView).ClearSelection();
                (sender as Guna2DataGridView).Rows[e.RowIndex].Selected = true; // Выделяем строку
                guna2ContextMenuStrip1.Show(Cursor.Position); // Показываем контекстное меню в месте курсора
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {

        }

        private void ConfirmButtonSeek(Guna2TileButton gtb)
        {
            foreach(Guna2TileButton cgtb in gtb.Parent.Controls)
            {
                if (cgtb.Tag.ToString() == "Confirm")
                    currentconfirmbutton = cgtb;
            }
        }

        private void guna2TileButton1_Click(object sender, EventArgs e)
        {
            ConfirmButtonSeek(sender as Guna2TileButton);
            CustomGuna2DataGridView dgv = buttonsAndDGV[currentbutton][0];

            dgv.HidePrimary = false;
            DataTable table = (DataTable)dgv.DataSource;
            DataRow row = table.NewRow();

            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn column = table.Columns[i];
                object value = DBNull.Value;

                if (column.DataType == typeof(string))
                {
                    string baseValue = "Значение";
                    string newValue = baseValue + dgv.Rows.Count;

                    if (column.Unique)
                    {
                        while (table.AsEnumerable().Any(r => r[i].ToString() == newValue))
                            newValue = baseValue + Guid.NewGuid().ToString("З").Substring(0, 6);
                    }

                    if (column.MaxLength > 0 && newValue.Length > column.MaxLength)
                        newValue = newValue.Substring(0, column.MaxLength);

                    value = newValue;
                }
                else if (column.DataType == typeof(Int32) || column.DataType == typeof(Int64))
                {
                    int newValue = 1;
                    if (column.Unique)
                    {
                        var existing = table.AsEnumerable().Select(r => Convert.ToInt32(r[i])).ToHashSet();
                        while (existing.Contains(newValue)) newValue++;
                    }

                    value = newValue;
                }
                else if (column.DataType == typeof(SByte) || column.DataType == typeof(Byte))
                {
                    byte newValue = 1;
                    if (column.Unique)
                    {
                        var existing = table.AsEnumerable().Select(r => Convert.ToByte(r[i])).ToHashSet();
                        while (existing.Contains(newValue)) newValue++;
                    }

                    value = newValue;
                }
                else if (column.DataType == typeof(DateTime))
                {
                    value = DateTime.Now;
                }
                else if (column.DataType == typeof(Boolean))
                {
                    value = false;
                }

                if (!column.ReadOnly)
                    row[i] = value;
            }

            table.Rows.Add(row);

            int rowIndex = dgv.Rows.Count - 1;
            addingRowsDict[rowIndex] = dgv.Rows[rowIndex];

            dgv.HidePrimary = true;
            if (addingRowsDict.Count > 0)
                currentconfirmbutton.Enabled = true;
        }

        private void guna2TileButton2_Click(object sender, EventArgs e)
        {
            ConfirmButtonSeek(sender as Guna2TileButton);
            CustomGuna2DataGridView dgv = buttonsAndDGV[currentbutton][0]; // Получаем текущий DataGridView
            DataTable table = (DataTable)dgv.DataSource; // Приводим источник данных DataGridView к DataTable
            dgv.HidePrimary = false;
            string primarykey = dgv.Columns[0].HeaderText;
            dgv.HidePrimary = true;

            if (VoidsMain.columnheadertexts.Values.Contains(primarykey))
                primarykey = VoidsMain.columnheadertexts.FirstOrDefault(x => x.Value == primarykey).Key;

            // Получаем список индексов выделенных строк (сортируем по убыванию, чтобы индексация не сбивалась)
            List<int> selectedIndexes = dgv.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).OrderByDescending(index => index).ToList();

            foreach (int rowIndex in selectedIndexes)
            {
                if (rowIndex >= 0)
                {
                    if (!addingRowsDict.ContainsKey(rowIndex)) // Если строка уже в БД, нужно удалить ее через запрос
                    {
                        try
                        {
                            dgv.QueryDelete = ($@"DELETE FROM {dgv.Tag} WHERE {primarykey} = '{dgv.Rows[rowIndex].Cells[0].Value}'");
                            dgv.QueryDeleteCommand.ExecuteQuery();
                            UpdateDGVFromDB();
                        }
                        catch
                        {
                            VoidsMain.MessageBoxCustomShow("Ошибка запроса", "Невозможно удалить данные, проверьте их наличие или целостность!");
                            return;
                        }
                    }
                    else
                    {
                        addingRowsDict.Remove(rowIndex);
                    }

                    table.Rows.RemoveAt(rowIndex); // Удаление строки из DataTable
                }
            }

            // Пересоздаем словарь с правильными индексами
            Dictionary<int, DataGridViewRow> updatedDict = new Dictionary<int, DataGridViewRow>();
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (addingRowsDict.Values.Contains(dgv.Rows[i])) // Сопоставляение строки по объектам
                {
                    updatedDict[i] = dgv.Rows[i]; // Присваивание новых индексов
                }
            }
            addingRowsDict = updatedDict; // Обновляем словарь

            currentconfirmbutton.Enabled = addingRowsDict.Count > 0; // Включаем кнопку "Применить изменения", если есть добавленные строки
        }

        private void guna2TileButton3_Click(object sender, EventArgs e)
        {
            CustomGuna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            string tableName = dgv.Tag.ToString();
            string connectionString = MainForm.connectionstring;

            // Получаем имя первичного ключа (первый столбец предполагается как PK)
            string primaryKeyColumn = dgv.Columns[0].Name;

            // Получаем максимальное значение первичного ключа через VoidsMain
            string maxIdQuery = $"SELECT MAX({primaryKeyColumn}) AS MaxId FROM {tableName}";
            DataTable maxIdTable = VoidsMain.SelectRequestAsDataTable(maxIdQuery, connectionString);
            int maxId = 0;

            if (maxIdTable.Rows.Count > 0 && maxIdTable.Rows[0]["MaxId"] != DBNull.Value)
                maxId = Convert.ToInt32(maxIdTable.Rows[0]["MaxId"]);

            foreach (var kvp in addingRowsDict)
            {
                DataGridViewRow dgvr = kvp.Value;

                // Устанавливаем значение первичного ключа как MAX + 1
                dgvr.Cells[0].Value = ++maxId;

                // Экранируем значения и собираем строку для SQL
                string temp = string.Join(", ", dgvr.Cells.Cast<DataGridViewCell>()
                .Where(cell => cell.ColumnIndex > 0)
                .Select(cell =>
                {
                    object value = cell.Value;

                    if (value == null || value == DBNull.Value)
                        return "NULL";

                    if (value is DateTime dt)
                        return $"'{dt:yyyy-MM-dd HH:mm:ss}'"; // Правильный формат для MySQL

                    return $"'{MySqlHelper.EscapeString(value.ToString())}'";
                }));
                try
                {
                    dgv.QueryInsert = string.Format(dgv.QueryInsert, temp, guna2ComboBox1.SelectedItem?.ToString() ?? "");
                    dgv.QueryInsertCommand.ExecuteQuery();
                }
                catch
                {
                    VoidsMain.MessageBoxCustomShow("Ошибка запроса", "Невозможно выполнить запрос. Возможно несоответствие первичных и внешних ключей!");
                    return;
                }
            }

            addingRowsDict.Clear();
            currentconfirmbutton.Enabled = false;
        }

        private void guna2DataGridViewРасписание1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CustomGuna2DataGridView dgv = sender as CustomGuna2DataGridView;

            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].ValueType == typeof(DateTime))
            {
                // Получаем прямоугольник ячейки
                Rectangle cellRect = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                Point screenPoint = dgv.PointToScreen(new Point(cellRect.X, cellRect.Y + cellRect.Height));

                // Находим активную вкладку среди GradientPanel-ов
                Control activeTab = dgv;
                while (activeTab != null && !(activeTab is Guna2CustomGradientPanel && activeTab.Visible))
                {
                    activeTab = activeTab.Parent;
                }

                // Если нашли активную панель — перемещаем туда календарь
                if (activeTab != null)
                {
                    monthCalendar1.Parent = activeTab;
                    monthCalendar1.Location = activeTab.PointToClient(screenPoint);
                    monthCalendar1.BringToFront();
                    monthCalendar1.Show();

                    currentcell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }
            }
        }

        private void guna2DataGridViewСтуденты_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (!monthCalendar1.Visible && !monthCalendar1.Focused)
            {
                monthCalendar1.Hide();
                currentcell = null;
            }
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (currentcell != null)
            {
                currentcell.Value = e.Start; // Просто задаем дату
                monthCalendar1.Hide();
                currentcell = null;
            }
        }
    }
}
