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
        string OptionsFilePath;
        public InitForm(InputField.InputEnd EndAction, Action QuitAction, string FilePath)
        {
            InitializeComponent();
            List<string> Options = new List<string>(File.ReadAllLines(FilePath));
            for (int i = Options.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(Options[i]))
                    Options.RemoveAt(i);
            }
            OptionsFilePath = FilePath;
            ServerOptions.Items.AddRange(Options.ToArray());
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

        private void RESET_Click(object sender, EventArgs e)
        {
            File.Delete(OptionsFilePath);
            ServerOptions.Text = "";
            this.Close();
        }
    }
}
