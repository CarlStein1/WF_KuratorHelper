using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuratorHelper_main
{
    public class CustomDataGridView : Guna2DataGridView
    {
        private string _QuerrySelect;
        private string _QuerryInsert;
        private string _QuerryUpdate;
        private bool _NeedActionForm;
        private bool _ReadOnlyForeignKey;

        [Category("Custom Props")]
        public string QuerrySelect
        {
            get { return _QuerrySelect; }
            set
            {
                _QuerrySelect = value;
            }
        }
        public string QuerryInsert
        {
            get { return _QuerryInsert; }
            set
            {
                _QuerryInsert = value;
            }
        }
        public string QuerryUpdate
        {
            get { return _QuerryUpdate; }
            set
            {
                _QuerryUpdate = value;
            }
        }
        public bool NeedActionForm
        {
            get { return _NeedActionForm; }
            set
            {
                _NeedActionForm = value;
            }
        }
        public bool ReadOnlyForeignKey
        {
            get { return _ReadOnlyForeignKey; }
            set
            {
                _ReadOnlyForeignKey = value;
                this.Columns[0].ReadOnly = value;
            }
        }
    }
}
