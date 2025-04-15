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

namespace KuratorHelper_main
{
    internal class VoidsMain
    {

        // Словарь для записей названия колонок, для перевода с английского на русский
        public static Dictionary<string, string> columnheadertexts = new Dictionary<string, string>()
        {
            { "student_card", "Зачетка" },
            { "last_name", "Фамилия"},
            { "first_name", "Имя" },
            { "middle_name", "Отчество" },
            { "gender", "Пол" },
            { "birth_date", "Дата рождения" },
            { "status", "Статус" },
            { "group_name", "Группа" },
            { "registered_address_id", "Адрес прописки" },
            { "actual_address_id", "Адрес проживания" },

            { "expulsion_group", "Группа отчисления" },
            { "expulsion_date", "Дата отчисления" },
            { "expulsion_order", "Приказ отчисления" },
            { "restoration_group", "Группа зачисления" },
            { "restoration_date", "Дата зачисления" },
            { "restoration_order", "Приказ зачисления" },

            { "address_id", "id_адреса" },
            { "city", "Город" },
            { "street", "Улица" },
            { "house", "Дом" },
            { "apartment", "Квартира"},

            { "passport_series", "Серия"},
            { "passport_number", "Номер паспорта"},
            { "inn", "ИНН"},
            { "snils", "СНИЛС"},
            { "military_status", "Воинский учет"},

            { "specialty", "Специальность"},
            { "teacher", "Учитель"},

            { "day_id", "id_дня"},
            { "lesson_number", "Номер пары"},
            { "subject_name", "Предмет"},
            { "week", "Неделя"},
            { "weekday", "День недели"},

            { "login", "Логин"},
            { "password", "Пароль"}
        };

        // Метод переключения панелей
        public static void ChangePanel(Dictionary<Guna2TileButton, Guna2GradientPanel> dict, object sender)
        {
            foreach (Guna2GradientPanel panels in dict.Values)
            {
                // Скрываем все панели, кроме той, которая связана с нажатой кнопкой
                panels.Visible = panels == dict[sender as Guna2TileButton];
            }
        }

        // Выполнение SELECT-запроса и возврат результата в виде DataTable
        public static DataTable SelectRequestAsDataTable(string request, string connectionstring)
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

        // Выполнение SELECT-запроса и возврат результата в виде List<string[]>
        public static List<string[]> SelectRequestAsList(string query, string connectionstring)
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
        public static void InsDelUpdRequest(string request, string connectionstring)
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
    }
}
