using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace KuratorHelper_main
{
    internal class VoidsMain
    {

        public const string connectionstring = "server=localhost;port=3306;database=kuratorhelper;user=root;"; // Строка подключения к базе данных MySQL
        // Словарь для записей названия колонок, для перевода с английского на русский
        public static Dictionary<string, string> columnheadertexts = new Dictionary<string, string>()
        {
            { "Зачетка", "student_card" },
            { "Фамилия", "last_name" },
            { "Имя", "first_name" },
            { "Отчество", "middle_name" },
            { "Пол", "gender" },
            { "Дата рождения", "birth_date" },
            { "Статус", "status" },
            { "Группа", "group_name" },
            { "Адрес прописки", "registered_address_id" },
            { "Адрес проживания", "actual_address_id" },

            { "Группа отчисления", "expulsion_group" },
            { "Дата отчисления", "expulsion_date" },
            { "Приказ отчисления", "expulsion_order" },
            { "Группа зачисления", "restoration_group" },
            { "Дата зачисления", "restoration_date" },
            { "Приказ зачисления", "restoration_order" },

            { "Город", "city" },
            { "Улица", "street" },
            { "Дом", "house" },
            { "Квартира", "apartment" },

            { "Серия", "passport_series" },
            { "Номер паспорта", "passport_number" },
            { "ИНН", "inn" },
            { "СНИЛС", "snils" },
            { "Воинский учет", "military_status" },

            { "Специальность", "specialty" },
            { "Преподаватель", "id_tutor" },

            { "Номер пары", "lesson_number" },
            { "Предмет", "subject_name" },
            { "Неделя", "week" },
            { "День недели", "weekday" },

            { "Логин", "login" },
            { "Пароль", "password" }
        };

        // Выполнение SELECT-запроса и возврат результата в виде DataTable
        public static DataTable SelectRequestAsDataTable(string request)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(connectionstring))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(request, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                    reader.Close();
                }
            }

            foreach (DataColumn dc in dataTable.Columns)
            {
                if (columnheadertexts.Keys.Contains(dc.ColumnName))
                    dc.ColumnName = columnheadertexts[dc.ColumnName];
            }
            return dataTable;
        }

        public static void AssignRowsIfColumnCountMatches(DataGridView dataGridView, DataTable dataTable)
        {
            if (dataGridView == null || dataTable == null)
                return;

            if (dataGridView.ColumnCount == dataTable.Columns.Count)
            {
                dataGridView.Rows.Clear(); // очищаем существующие строки

                // Присваиваем строки
                foreach (DataRow row in dataTable.Rows)
                {
                    dataGridView.Rows.Add(row.ItemArray);
                }

                // Устанавливаем формат отображения дат
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    if (dataTable.Columns[i].DataType == typeof(DateTime))
                    {
                        dataGridView.Columns[i].DefaultCellStyle.Format = "yyyy-MM-dd";
                    }
                }
            }
            else
            {
                VoidsMain.MessageBoxCustomShow("Ошибка", "Количество столбцов в DataGridView и DataTable не совпадает.");
            }
        }

        // Выполнение SELECT-запроса и возврат результата в виде List<string[]>
        public static List<string[]> SelectRequestAsList(string query)
        {
            List<string[]> result = new List<string[]>();

            using (MySqlConnection connection = new MySqlConnection(connectionstring))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    int columnCount = reader.FieldCount;

                    while (reader.Read())
                    {
                        string[] rowValues = new string[columnCount];
                        for (int i = 0; i < columnCount; i++)
                        {
                            rowValues[i] = reader[i].ToString();
                        }
                        result.Add(rowValues);
                    }
                }
            }

            return result;
        }

        // Метод для выполнения INSERT, DELETE, UPDATE запросов
        public static void InsDelUpdRequest(string request)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionstring))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(request, connection);
                command.ExecuteNonQuery();
            }
        }

        // Метод для вызова кастомного MessageBox
        public static DialogResult MessageBoxCustomShow(string title, string text, bool needconfirm = false)
        {
            MessageBoxCustom cfrmsg = new MessageBoxCustom();
            cfrmsg.title = title;
            cfrmsg.text = text;
            cfrmsg.needconfirm = needconfirm;
            cfrmsg.ShowDialog();
            return cfrmsg.DialogResult;
        }

        public static void ReportWord(Guna2DataGridView dgv)
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта в Word.");
                return;
            }

            try
            {
                Word.Application wordApp = new Word.Application();
                wordApp.Visible = true;
                Word.Document doc = wordApp.Documents.Add();

                // Заголовок
                Word.Paragraph paraTitle = doc.Content.Paragraphs.Add();
                paraTitle.Range.Text = $"Отчет о таблице \"{dgv.Name}\"";
                paraTitle.Range.Font.Size = 16;
                paraTitle.Range.Font.Bold = 1;
                paraTitle.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                paraTitle.Range.InsertParagraphAfter();

                // Таблица
                int rowCount = dgv.Rows.Count;
                int columnCount = dgv.Columns.Count;

                Word.Range range = doc.Bookmarks.get_Item("\\endofdoc").Range;
                Word.Table table = doc.Tables.Add(range, rowCount + 1, columnCount);
                table.Borders.Enable = 1;
                table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitContent);
                table.Range.Font.Size = 12;
                table.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;

                // Заголовки столбцов
                for (int c = 0; c < columnCount; c++)
                {
                    var headerCell = table.Cell(1, c + 1);
                    headerCell.Range.Text = dgv.Columns[c].HeaderText;
                    headerCell.Range.Bold = 1;
                    headerCell.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerCell.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray20;
                }

                // Строки данных
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        var value = dgv.Rows[r].Cells[c].Value?.ToString() ?? "";
                        var cell = table.Cell(r + 2, c + 1);
                        cell.Range.Text = value;
                        cell.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }
                }

                // Ориентация страницы (опционально)
                doc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;

                // Показать Word
                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBoxCustomShow("Ошибка!", "Ошибка при экспорте в Word: " + ex.Message);
            }
        }

        public static void ReportExcel(Guna2DataGridView dgv)
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта в Excel.");
                return;
            }

            try
            {
                Excel.Application excelApp = new Excel.Application();
                excelApp.Visible = true;
                Excel.Workbook workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.Sheets[1];
                worksheet.Name = $"Отчет о таблице \"{dgv.Name}\"";

                int rowStart = 2;
                int colCount = dgv.Columns.Count;

                // Заголовок
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, colCount]];
                titleRange.Merge();
                titleRange.Value = $"Отчет о таблице \"{dgv.Name}\"";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки столбцов
                for (int c = 0; c < colCount; c++)
                {
                    worksheet.Cells[rowStart, c + 1] = dgv.Columns[c].HeaderText;
                    worksheet.Cells[rowStart, c + 1].Font.Bold = true;
                    worksheet.Cells[rowStart, c + 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                }

                // Строки данных
                for (int r = 0; r < dgv.Rows.Count; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        var value = dgv.Rows[r].Cells[c].Value?.ToString() ?? "";
                        worksheet.Cells[r + rowStart + 1, c + 1] = value;
                    }
                }

                // Автоширина столбцов
                worksheet.Columns.AutoFit();

                // Опционально — авторамка таблицы
                var fullRange = worksheet.Range[
                    worksheet.Cells[rowStart, 1],
                    worksheet.Cells[dgv.Rows.Count + rowStart, colCount]
                ];
                fullRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            }
            catch (Exception ex)
            {
                MessageBoxCustomShow("Ошибка!","Ошибка при экспорте в Excel: " + ex.Message);
            }
        }
        public static void ReportTXT(Guna2DataGridView dgv)
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить как",
                Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv",
                FileName = $"Отчет о таблице \"{dgv.Name}\".txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Заголовок таблицы
                        sw.WriteLine($"Отчет о таблице \"{dgv.Name}\"");
                        sw.WriteLine();

                        // Заголовки столбцов
                        for (int i = 0; i < dgv.Columns.Count; i++)
                        {
                            sw.Write(dgv.Columns[i].HeaderText);
                            if (i < dgv.Columns.Count - 1)
                                sw.Write("; ");
                        }
                        sw.WriteLine();

                        // Строки данных
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                for (int i = 0; i < dgv.Columns.Count; i++)
                                {
                                    var cellValue = row.Cells[i].Value?.ToString()?.Replace(";", ",") ?? "";
                                    sw.Write(cellValue);
                                    if (i < dgv.Columns.Count - 1)
                                        sw.Write("; ");
                                }
                                sw.WriteLine();
                            }
                        }
                    }

                    MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                }
            }
        }
    }
}
