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

            guna2DataGridViewРасписание2.Rows.Add("Английский\nЛузина И.А");
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
        private void UpdateDGVFromDB(Guna2DataGridView dgv = null)
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
                                if (ctrl3 is Guna2DataGridView)
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
                                        Guna2DataGridView currentdgv = ctrl3 as Guna2DataGridView;
                                        if (!String.IsNullOrEmpty(currentdgv.Tag.ToString()))
                                        {
                                            currentdgv.Columns.Clear();
                                            currentdgv.DataSource = VoidsMain.SelectRequestAsDataTable(string.Format(currentdgv.Tag.ToString(), guna2ComboBox1.SelectedItem?.ToString() ?? ""));
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

            if (gtb.Text == "Верхняя")
                guna2DataGridViewРасписание2.Tag = "SELECT s.lesson_number AS 'Номер пары', MAX(CASE WHEN s.weekday = 'ПН' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', " +
                    "LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') ELSE '' END) AS 'ПН', MAX(CASE WHEN s.weekday = 'ВТ' " +
                    "THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') ELSE '' END) AS 'ВТ', " +
                    "MAX(CASE WHEN s.weekday = 'СР' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'СР', MAX(CASE WHEN s.weekday = 'ЧТ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'ЧТ', MAX(CASE WHEN s.weekday = 'ПТ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'ПТ', MAX(CASE WHEN s.weekday = 'СБ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'СБ' FROM schedule s JOIN teachers t ON s.tutor_id = t.tutor_id WHERE s.week = 'Верхняя' AND s.group_name = '{0}' " +
                    "GROUP BY s.lesson_number ORDER BY s.lesson_number;";
            else
                guna2DataGridViewРасписание2.Tag = "SELECT s.lesson_number AS 'Номер пары', MAX(CASE WHEN s.weekday = 'ПН' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', " +
                    "LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') ELSE '' END) AS 'ПН', MAX(CASE WHEN s.weekday = 'ВТ' " +
                    "THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') ELSE '' END) AS 'ВТ', " +
                    "MAX(CASE WHEN s.weekday = 'СР' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'СР', MAX(CASE WHEN s.weekday = 'ЧТ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'ЧТ', MAX(CASE WHEN s.weekday = 'ПТ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'ПТ', MAX(CASE WHEN s.weekday = 'СБ' THEN CONCAT(s.subject_name, '\n', t.last_name, ' ', LEFT(t.first_name, 1), '.', LEFT(t.middle_name, 1), '.') " +
                    "ELSE '' END) AS 'СБ' FROM schedule s JOIN teachers t ON s.tutor_id = t.tutor_id WHERE s.week = 'Нижняя' AND s.group_name = '{0}' " +
                    "GROUP BY s.lesson_number ORDER BY s.lesson_number;";
            UpdateDGVFromDB(guna2DataGridViewРасписание2);
        }

        private void guna2DataGridViewСтуденты_SelectionChanged(object sender, EventArgs e)
        {
            if ((sender as Guna2DataGridView).SelectedRows.Count > 0)
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
                            ctrl1.Text = ctrl1.AccessibleDescription.ToString() + " " + VoidsMain.SelectRequestAsList(string.Format(ctrl1.Tag.ToString(), (sender as DataGridView).SelectedRows[0].Cells[0].Value))[0][0];
                        }
                    }
                }
            }
        }
    }
}
