using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Database_Control
{
    public partial class InitForm : Form
    {
        InputField.InputEnd EndAction;
        Action QuitAction;
        public InitForm(InputField.InputEnd EndAction, Action QuitAction, string FilePath)
        {
            InitializeComponent();
            string[] Options = File.ReadAllLines(FilePath);
            ServerOptions.Items.AddRange(Options);
            this.FilePath.Text = FilePath;
            this.EndAction = EndAction;
            this.QuitAction = QuitAction;
        }

        private void InitForm_Load(object sender, EventArgs e)
        {

        }

        private void QuitBtn_Click(object sender, EventArgs e)
        {
            QuitAction();
            this.Close();
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            EndAction(ServerOptions.Text);
            this.Close();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }
    }
}
