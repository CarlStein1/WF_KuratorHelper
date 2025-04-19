// Основные подключения библиотек
using Guna.UI2.WinForms; // Библиотека Guna UI 2 для визуальных компонентов
using MySql.Data.MySqlClient; // Работа с MySQL
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data; // Для работы с DataTable и другими структурами данных
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuratorHelper_main
{
    public partial class MainForm : Form
    {
        // Связь между кнопками и панелями (основные вкладки)
        Dictionary<Guna2TileButton, Guna2CustomGradientPanel> buttonsAndMainPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>();
        // Связь между основными панелями и внутренними панелями
        Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel> mainPanelsAndPanels = new Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel>();
        // Связь между кнопками и таблицами (DataGridView)
        Dictionary<Guna2TileButton, List<Guna2DataGridView>> buttonsAndDGV = new Dictionary<Guna2TileButton, List<Guna2DataGridView>>();
        // Временные строки, которые добавляются вручную и могут быть внесены в БД
        Dictionary<int, DataGridViewRow> addingRowsDict = new Dictionary<int, DataGridViewRow>();
        string lastcellvalue; // Хранит старое значение редактируемой ячейки
        string primarykeyvalue; // Хранит значение первичного ключа редактируемой строки
        string currentgroup; // Текущая выбранная группа
        Guna2TileButton currentbutton; // Текущая активная кнопка (вкладка)
        Guna2TileButton currentconfirmbutton; // Кнопка "Подтвердить" для добавления строк
        DataGridViewCell currentcell; // Текущая редактируемая ячейка
        internal string[] kuratordata; // Данные текущего куратора

        /* 
         * В ТАГ ТАБЛИЦ ВПИСАТЬ ЗАПРОС НА ВСТАВКУ ДАННЫХ В ЭТИ ТАБЛИЦЫ
         * В ТАГ КНОПОК ВПИСАТЬ НАЗВАНИЕ ТАБЛИЦЫ В БД
         * ДОБАВЛЕНИЕ И УДАЛЕНИЕ ЧЕРЕЗ КНОПКИ НА ПАНЕЛЕ СВЕРХУ
         * ИЗМЕНЕНИЕ И ДРУГИЕ ФУНКЦИИ (ПО ТИПУ ОТПРАВКА СТУДЕНТА В АКАДЕМ) ЧЕРЕЗ КОНТЕКСТНОЕ МЕНЮ ПРИ НАЖАТИИ ПКМ
         * НЕДЕЛИ РАСПИСАНИЯ РЕАЛИЗОВАТЬ ЧЕРЕЗ 2 РАЗНЫЕ ТАБЛИЦЫ: ВЕРХНЯЯ И НИЖНЯЯ НЕДЕЛЯ
         * СДЕЛАТЬ ТАК, ЧТОБЫ АДРЕСА МОЖНО БЫЛО ПРИКРЕПЛЯТЬ К СТУДЕНТАМ ВО ВКЛАДКАХ АДРЕСА, КАК И ДОКУМЕНТЫ
        */

        // Конструктор формы
        public MainForm()
        {
            InitializeComponent();

            // Заполняем словарь кнопок и панелей
            buttonsAndMainPanels = new Dictionary<Guna2TileButton, Guna2CustomGradientPanel>{
                { guna2TileButtonСтуденты, guna2CustomGradientPanelСтуденты},
                { guna2TileButtonАкадемы, guna2CustomGradientPanelАкадемы},
                { guna2TileButtonРасписание, guna2CustomGradientPanelРасписание},
                { guna2TileButtonАдреса, guna2CustomGradientPanelАдреса},
                { guna2TileButtonДокументы, guna2CustomGradientPanelДокументы }
            };

            // Связываем основную панель с вложенной
            mainPanelsAndPanels = new Dictionary<Guna2CustomGradientPanel, Guna2GradientPanel>{
                { guna2CustomGradientPanelСтуденты, guna2GradientPanelСтуденты },
                { guna2CustomGradientPanelАкадемы, guna2GradientPanelАкадемы },
                { guna2CustomGradientPanelАдреса, guna2GradientPanelАдреса },
                { guna2CustomGradientPanelДокументы, guna2GradientPanelДокументы }
            };

            // Привязка таблиц (DGV) к соответствующим кнопкам
            buttonsAndDGV = new Dictionary<Guna2TileButton, List<Guna2DataGridView>>
            {
                { guna2TileButtonСтуденты, new List<Guna2DataGridView> { Студенты } },
                { guna2TileButtonАкадемы, new List<Guna2DataGridView> { Академы } },
                { guna2TileButtonРасписание, new List<Guna2DataGridView> { Расписание1, Расписание2 } },
                { guna2TileButtonАдреса, new List<Guna2DataGridView> { Адреса } },
                { guna2TileButtonДокументы, new List<Guna2DataGridView> { Документы } }
            };
        }

        // Метод при загрузке формы
        private void MainForm_Load(object sender, EventArgs e)
        {
            currentbutton = guna2TileButtonСтуденты; // По умолчанию открываем вкладку студентов

            // Получаем список групп для текущего куратора
            List<string[]> temp = VoidsMain.SelectRequestAsList($"SELECT group_name FROM groups WHERE id_tutor = {kuratordata[0]}");
            for (int i = 0; i < temp.Count; i++)
            {
                guna2ComboBox1.Items.Add(temp[i][0]);
            }
            guna2ComboBox1.SelectedIndex = 0;
            currentgroup = guna2ComboBox1.SelectedItem.ToString();

            UpdateDGVFromDB(); // Загружаем данные в таблицы
        }

        // Изменение выбранной группы
        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentgroup = guna2ComboBox1.SelectedItem.ToString();
            UpdateDGVFromDB();
        }

        // Обновление данных в DataGridView из БД
        private void UpdateDGVFromDB(params object[] list)
        {
            DataTable dt;
            Guna2DataGridView dgv;

            if (list.Length == 0) list = buttonsAndDGV[currentbutton].ToArray(); // Если ничего не передано — обновляем все DGV текущей вкладки

            for (int i = 0; i < list.Length; i++)
            {
                dgv = list[i] as Guna2DataGridView;
                string query = dgv.Tag.ToString();
                if (dgv.AccessibleDescription != null)
                    if (dgv.AccessibleDescription.ToString() == "groupneed") query = string.Format(query, guna2ComboBox1.SelectedItem?.ToString() ?? "");

                if (query == null)
                    dt = VoidsMain.SelectRequestAsDataTable($"SELECT * FROM {buttonsAndMainPanels[currentbutton]}");
                else
                    dt = VoidsMain.SelectRequestAsDataTable(query);

                VoidsMain.AssignRowsIfColumnCountMatches(dgv, dt); // Подгружаем строки в DGV
            }
        }

        // Смена вкладки
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

            UpdateDGVFromDB(); // Обновляем данные при переключении вкладки
        }

        // Перемещение формы (захват за панель)
        private void guna2GradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            guna2DragControl1.TargetControl = sender as Control;
        }

        // Переключение между неделями расписания
        private void guna2TileButton10_Click_1(object sender, EventArgs e)
        {
            Guna2TileButton gtb = sender as Guna2TileButton;

            guna2TileButton10.Checked = guna2TileButton11.Checked = false;
            gtb.Checked = true;
            Расписание2.Tag = string.Format(gtb.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? "");
            UpdateDGVFromDB(); // Обновляем таблицу расписания
        }

        // Выделение строки в таблице студентов — обновление меток
        private void guna2DataGridViewСтуденты_SelectionChanged(object sender, EventArgs e)
        {
            if ((sender as Guna2DataGridView).SelectedRows.Count > 0)
            {
                foreach (Control ctrl1 in mainPanelsAndPanels[buttonsAndMainPanels[currentbutton]].Controls)
                {
                    if (ctrl1 is Guna2HtmlLabel)
                    {
                        if (addingRowsDict.Values.Contains((sender as Guna2DataGridView).CurrentRow))
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
                                tempdata = VoidsMain.SelectRequestAsList(string.Format(ctrl1.Tag.ToString(), (sender as Guna2DataGridView).SelectedRows[0].Cells[1].Value))[0][0];
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

        // Поиск по текстовому полю
        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            Guna2TextBox txb = sender as Guna2TextBox;

            if (dgv.DataSource is DataTable table)
            {
                string filterExpression = string.Empty;

                // Фильтрация по всем столбцам
                if (!string.IsNullOrEmpty(txb.Text))
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        filterExpression += $"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{txb.Text}%' OR ";
                    }
                    filterExpression = filterExpression.TrimEnd(" OR ".ToCharArray());
                }

                // Применяем фильтр
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
                Guna2DataGridView dgv = sender as Guna2DataGridView;

                if (addingRowsDict.Values.Contains(dgv.Rows[e.RowIndex])) return;
                string changingcolumn = dgv.Columns[e.ColumnIndex].HeaderText;
                var a = $"UPDATE {buttonsAndMainPanels[currentbutton].Tag} SET {VoidsMain.columnheadertexts[dgv.Columns[e.ColumnIndex].HeaderText]} = '{dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value}' WHERE {dgv.Columns[0].HeaderText} = '{dgv.Rows[e.RowIndex].Cells[0].Value}'";
                VoidsMain.InsDelUpdRequest($"UPDATE {buttonsAndMainPanels[currentbutton].Tag} SET {VoidsMain.columnheadertexts[dgv.Columns[e.ColumnIndex].HeaderText]} = '{dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value}' WHERE {dgv.Columns[0].HeaderText} = '{dgv.Rows[e.RowIndex].Cells[0].Value}'");
                if (addingRowsDict.Count <= 0)
                    UpdateDGVFromDB();
            }
        }

        private void guna2DataGridViewСтуденты_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 0)
                primarykeyvalue = (sender as Guna2DataGridView).CurrentCell.Value.ToString();
            if (lastcellvalue != null)
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

        private void guna2TileButton1_Click(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            ConfirmButtonSeek(sender as Guna2TileButton);

            dgv.Rows.Add();
            int rowIndex = dgv.Rows.Count - 1;
            DataGridViewRow newRow = dgv.Rows[rowIndex];
            addingRowsDict[rowIndex] = newRow;

            foreach (DataGridViewColumn clmn in dgv.Columns)
            {
                if (new string[] { "AutoInc", "0" }.Contains(clmn.DataPropertyName)) clmn.Visible = addingRowsDict.Count <= 0;
            }

            if (addingRowsDict.Count > 0)
                currentconfirmbutton.Enabled = true;
        }

        private void ConfirmButtonSeek(Guna2TileButton gtb)
        {
            foreach (Guna2TileButton cgtb in gtb.Parent.Controls)
            {
                if (cgtb.Tag.ToString() == "Confirm")
                    currentconfirmbutton = cgtb;
            }
        }

        private void guna2TileButton2_Click(object sender, EventArgs e)
        {
            ConfirmButtonSeek(sender as Guna2TileButton);
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0]; // Получаем текущий DataGridView
            string primarykey = dgv.Columns[0].HeaderText;

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
                            VoidsMain.InsDelUpdRequest($@"DELETE FROM {buttonsAndMainPanels[currentbutton].Tag} WHERE {primarykey} = '{dgv.Rows[rowIndex].Cells[0].Value}'");
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

                    dgv.Rows.RemoveAt(rowIndex); // Удаление строки из DataGridView
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
            foreach (DataGridViewColumn clmn in dgv.Columns)
            {
                if (new string[] { "AutoInc", "0" }.Contains(clmn.DataPropertyName)) clmn.Visible = addingRowsDict.Count <= 0;
            }
        }

        private void guna2TileButton3_Click(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            string tableName = buttonsAndMainPanels[currentbutton].Tag.ToString();
            bool isAutoInc = dgv.Columns[0].DataPropertyName == "AutoInc";
            int maxId = 0;

            // Получаем имя столбца первичного ключа
            string pkColumn = dgv.Columns[0].HeaderText;
            if (isAutoInc)
            {
                string mappedPk = VoidsMain.columnheadertexts.ContainsKey(pkColumn)
                    ? VoidsMain.columnheadertexts[pkColumn]
                    : pkColumn;

                DataTable result = VoidsMain.SelectRequestAsDataTable($"SELECT MAX({mappedPk}) AS MaxId FROM {tableName}");
                if (result.Rows.Count > 0 && result.Rows[0]["MaxId"] != DBNull.Value)
                    maxId = Convert.ToInt32(result.Rows[0]["MaxId"]);
            }

            // Сохраняем только строки из addingRowsDict
            foreach (var kvp in addingRowsDict)
            {
                DataGridViewRow row = kvp.Value;
                List<string> columnNames = new List<string>();
                List<string> values = new List<string>();

                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    var col = dgv.Columns[i];
                    string dpn = col.DataPropertyName;

                    // Пропускаем столбцы с DataPropertyName == "0"
                    if (dpn == "0")
                        continue;

                    // Первому столбцу при необходимости ставим автоинкремент
                    object val = row.Cells[i].Value;
                    if (i == 0 && isAutoInc)
                    {
                        val = ++maxId;
                        row.Cells[i].Value = val;
                    }

                    // Имя колонки: маппим из columnheadertexts
                    string colName = VoidsMain.columnheadertexts.ContainsKey(col.HeaderText)
                        ? VoidsMain.columnheadertexts[col.HeaderText]
                        : col.HeaderText;
                    columnNames.Add(colName);

                    // Значение
                    if (dgv.AccessibleDescription.ToString() == "groupneed" && (colName == "group_name" || colName == "expulsion_group"))
                    {
                        values.Add($"\'{guna2ComboBox1.SelectedItem}\'");
                    }
                    else if (val == null || val == DBNull.Value)
                    {
                        values.Add("NULL");
                    }
                    else if (dpn == "Date" && DateTime.TryParse(val.ToString(), out DateTime dt))
                    {
                        values.Add($"'{dt:yyyy-MM-dd}'");
                    }
                    else
                    {
                        values.Add($"'{MySqlHelper.EscapeString(val.ToString())}'");
                    }
                }

                // Строим и выполняем SQL-запрос
                string query = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", values)});";
                try
                {
                    VoidsMain.InsDelUpdRequest(query); // Твоя функция выполнения SQL
                }
                catch
                {
                    VoidsMain.MessageBoxCustomShow("Ошибка запроса", "Не удалось вставить данные. Возможно несоответствие ключей или типов.");
                    return;
                }
            }

            // Завершающие действия
            addingRowsDict.Clear();
            guna2DataGridViewСтуденты_SelectionChanged(dgv, null);
            currentconfirmbutton.Enabled = false;
            foreach (DataGridViewColumn clmn in dgv.Columns)
            {
                if (new string[] { "AutoInc", "0" }.Contains(clmn.DataPropertyName)) clmn.Visible = addingRowsDict.Count <= 0;
            }
        }

        private void guna2DataGridViewРасписание1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Guna2DataGridView dgv = sender as Guna2DataGridView;

            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "Date")
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

            if (dgv.Columns[e.ColumnIndex].DataPropertyName.ToString().ToUpper().Contains("SELECT"))
            {
                if (addingRowsDict.Count > 0 && !addingRowsDict.Values.Contains(dgv.Rows[e.RowIndex]))
                {
                    return;
                }
                if (dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                    lastcellvalue = string.Empty;
                else
                    lastcellvalue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Choosing(dgv.Columns[e.ColumnIndex].DataPropertyName.ToString(), guna2ComboBox1.SelectedItem.ToString());
            }
        }

        private string Choosing(string query, params object[] add)
        {
            ActionsForm actf = new ActionsForm();
            actf.query = query;
            actf.addition = add;
            var a = actf.ShowDialog();
            if (a == DialogResult.OK)
            {
                return actf.value;
            }
            else
                return lastcellvalue;
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
                currentcell.Value = e.Start.ToString("yyyy-MM-dd"); // Просто задаем дату
                currentcell = null;
                monthCalendar1.Hide();
            }
        }

        private void guna2TileButton19_Click(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            if (currentbutton == guna2TileButtonРасписание)
                dgv = buttonsAndDGV[currentbutton][1];
            VoidsMain.ReportWord(dgv);
        }

        private void guna2TileButton18_Click(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            if (currentbutton == guna2TileButtonРасписание)
                dgv = buttonsAndDGV[currentbutton][1];
            VoidsMain.ReportExcel(dgv);
        }

        private void guna2TileButtonВыход_Click(object sender, EventArgs e)
        {
            Guna2DataGridView dgv = buttonsAndDGV[currentbutton][0];
            if (currentbutton == guna2TileButtonРасписание)
                dgv = buttonsAndDGV[currentbutton][1];
            VoidsMain.ReportTXT(dgv);
        }
    }
}
