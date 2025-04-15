using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;

namespace KuratorHelper_main
{
    public class CustomGuna2DataGridView : Guna2DataGridView
    {
        [Category("Custom Queries")]
        public string QuerySelect
        {
            get => _querySelect;
            set
            {
                _querySelect = value;
                QuerySelectCommand.query = value;
            }
        }

        [Category("Custom Queries")]
        public string QueryInsert
        {
            get => _queryInsert;
            set
            {
                _queryInsert = value;
                QueryInsertCommand.query = value;
            }
        }

        [Category("Custom Queries")]
        public string QueryUpdate
        {
            get => _queryUpdate;
            set
            {
                _queryUpdate = value;
                QueryUpdateCommand.query = value;
            }
        }

        [Category("Custom Queries")]
        public string QueryDelete
        {
            get => _queryDelete;
            set
            {
                _queryDelete = value;
                QueryDeleteCommand.query = value;
            }
        }

        private string _querySelect;
        private string _queryInsert;
        private string _queryUpdate;
        private string _queryDelete;

        [Browsable(false)]
        public CustomQuery QuerySelectCommand { get; set; }
        [Browsable(false)]
        public CustomQuery QueryInsertCommand { get; set; }
        [Browsable(false)]
        public CustomQuery QueryUpdateCommand { get; set; }
        [Browsable(false)]
        public CustomQuery QueryDeleteCommand { get; set; }

        private bool _readOnlyForeignKey;

        [Category("Custom Props")]
        public bool ReadOnlyForeignKey
        {
            get => _readOnlyForeignKey;
            set
            {
                _readOnlyForeignKey = value;
                if (this.Columns.Count > 0)
                    this.Columns[0].ReadOnly = value;
            }
        }

        public CustomGuna2DataGridView()
        {
            QuerySelectCommand = new CustomQuery { TargetGrid = this };
            QueryInsertCommand = new CustomQuery { TargetGrid = this };
            QueryUpdateCommand = new CustomQuery { TargetGrid = this };
            QueryDeleteCommand = new CustomQuery { TargetGrid = this };
        }

        public class CustomQuery
        {
            public string query { get; set; }
            public CustomGuna2DataGridView TargetGrid { get; set; }

            public void ExecuteQuerry(string connectionstring = MainForm.connectionstring)
            {
                if (string.IsNullOrWhiteSpace(query)) return;

                DataTable dataTable = new DataTable();
                using (MySqlConnection connection = new MySqlConnection(connectionstring))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                        reader.Close();
                    }
                }

                foreach (DataColumn dc in dataTable.Columns)
                {
                    if (VoidsMain.columnheadertexts.Keys.Contains(dc.ColumnName))
                        dc.ColumnName = VoidsMain.columnheadertexts[dc.ColumnName];
                }

                TargetGrid.Columns.Clear();
                TargetGrid.DataSource = dataTable;

                if (TargetGrid.Columns.Count > 0)
                    TargetGrid.Columns[0].ReadOnly = TargetGrid.ReadOnlyForeignKey;
            }
        }
    }
}
