using ACE.Entity.Enum;
using ACE.WorldBuilder.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACE.WorldBuilder.Forms
{
    public partial class CharactersForm : Form
    {
        public CharactersForm()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            ReloadCharacters();
        }

        private void DoRefresh_Click(object sender, EventArgs e)
        {
            ReloadCharacters();
        }

        private void ReloadCharacters()
        {
            Cursor.Current = Cursors.WaitCursor;

            grid.Rows.Clear();
            grid.Columns.Clear();
            grid.Columns.Add("accountID", "Account ID");
            grid.Columns.Add("characterID", "Character ID");
            grid.Columns.Add("name", "Name");
            grid.Columns.Add("level", "Level");

            var chars = CharacterManager.GetAllCharacters();
            foreach (var ch in chars)
            {
                grid.Rows.Add(new object[] { ch.AccountId, ch.Id, ch.Name, ch.Level });
            }

            Cursor.Current = Cursors.Default;
        }
    }
}
