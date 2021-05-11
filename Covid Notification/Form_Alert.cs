using Covid_Notification.Properties;
using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace Covid_Notification
{
    public partial class Form_Alert : Form
    {
        public Form_Alert()
        {
            InitializeComponent();
           
        }
        public enum enumAction
        {
            wait,
            start,
            close,
            waitALot
        }

        public enum enmType
        {
            Success,
            Warning,
        }

        private Form_Alert.enumAction action;

        private int x, y;

        private void button1_Click(object sender, EventArgs e)
        {
            timerAlert1.Interval = 1;
            action = enumAction.close;
        }

        private void timerAlert1_Tick(object sender, EventArgs e)
        {
            switch (this.action)
            {
                case enumAction.wait:
                    timerAlert1.Interval = 20000;
                    action = enumAction.close;
                    break;
                case Form_Alert.enumAction.start:
                    this.timerAlert1.Interval = 1;
                    this.Opacity += 0.1;
                    if (this.x < this.Location.X)
                    {
                        this.Left--;
                    }
                    else
                    {
                        if (this.Opacity == 1.0)
                        {
                            action = Form_Alert.enumAction.wait;
                        }
                    }
                    break;
                case enumAction.close:
                    timerAlert1.Interval = 1;
                    this.Opacity -= 0.1;

                    this.Left -= 3;
                    if (base.Opacity == 0.0)
                    {
                        base.Close();
                    }
                    break;
                case enumAction.waitALot:
                    timerAlert1.Interval = 20000;
                    action = enumAction.close;
                    break;
            }
        }

        private void Form_Alert_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerAlert1.Stop();
        }

        private void Form_Alert_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerAlert1.Dispose();
            timerAlert1 = null;
        }

       
        private void Form_Alert_MouseEnter(object sender, EventArgs e)
        {
            //if (this.Opacity == 1.0)
            //{
            //    action = Form_Alert.enumAction.waitALot;
            //}
        }

        private void Form_Alert_Load(object sender, EventArgs e)
        {
            SoundPlayer audio = new SoundPlayer(Resources.popup);
            audio.Play();
        }

      
        public void showAlert(string msg, int totalCount, enmType type)
        {
            this.Opacity = 0.0;
            this.StartPosition = FormStartPosition.Manual;
            string fname;

            for (int i = 1; i < 10; i++)
            {
                fname = "alert" + i.ToString();
                Form_Alert frm = (Form_Alert)Application.OpenForms[fname];

                if (frm == null)
                {
                    this.Name = fname;
                    this.x = Screen.PrimaryScreen.WorkingArea.Width - this.Width + 15;
                    this.y = Screen.PrimaryScreen.WorkingArea.Height - this.Height * i - 5 * i;
                    this.Location = new Point(this.x, this.y);
                    break;

                }

            }
            switch (type)
            {
                case enmType.Success:
                    this.pictureBox1.Image = Resources.success;
                    break;
                case enmType.Warning:
                    this.pictureBox1.Image = Resources.warning;
                    break;
            }
            this.x = Screen.PrimaryScreen.WorkingArea.Width - base.Width - 5;

            this.textBox1.Text = msg;
            this.label3.Text = totalCount.ToString();

            this.Show();
            this.action = enumAction.start;
            this.timerAlert1.Interval = 1;
            this.timerAlert1.Start();
        }
    }
}
