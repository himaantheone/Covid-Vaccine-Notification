using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace Covid_Notification
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient;
        public Form1()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength < 6)
                errorProvider1.SetError(textBox1, "Please provide a valid PINCODE");
            else
                errorProvider1.SetError(textBox1, "");
        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                timer1.Dispose();
            }
            var success = true;
            foreach (Control c in this.Controls)
            {
                if (!string.IsNullOrWhiteSpace(errorProvider1.GetError(c)) || !string.IsNullOrEmpty(errorProvider2.GetError(c)))
                    success = false;
            }
            if (success)
            {
                int minutes = 0;
                int.TryParse(textBox2.Text, out minutes);
                timer1.Interval = minutes == 0 ? (int)TimeSpan.FromMinutes(5).TotalMilliseconds : (int)TimeSpan.FromMinutes(minutes).TotalMilliseconds; 
                timer1.Enabled = true;
                var responses = GetData(comboBox1.SelectedIndex, textBox1.Text);
                var createmssg = CreateMessage(responses);
                if (!string.IsNullOrWhiteSpace(createmssg.mssg))
                    this.Alert(createmssg.mssg, createmssg.totalCount, Form_Alert.enmType.Success);
                else
                    this.Alert($"No slots open currently will search in background every {textBox2.Text} minutes and send notification only when slots are available", 0, Form_Alert.enmType.Warning);
            }
        }

        private CreateMessage CreateMessage(IEnumerable<CovidResponseModel> responses)
        {
            var createmssg = new StringBuilder();
            var mssgObj = new CreateMessage();

            foreach (var response in responses)
            {
                var vaccineCostString = "";
                if (response.Fee_type.ToLower() == "paid")
                {
                    response.Vaccine_fees.ToList().ForEach(x =>
                    {
                        vaccineCostString += $" Name : {x.vaccine} and Price : {x.fee}, ";
                    });
                }

                var dayAndSlot = new StringBuilder();
                response.Sessions.ToList().ForEach(x =>
                {
                    if (x.Available_capacity > 0)
                    {
                        mssgObj.totalCount = mssgObj.totalCount + x.Available_capacity;
                        dayAndSlot.AppendLine($"Date : {x.Date}, SlotsFree : {x.Available_capacity}, Vaccine : {x.Vaccine},");
                    }
                });
                createmssg.AppendLine($"Hospital Name and Address : {response.Name + response.Address}");
                createmssg.Append($"{dayAndSlot}");
                createmssg.AppendLine($"Type : {response.Fee_type}, VaccineCost : {vaccineCostString} \n");
            }
            mssgObj.mssg = createmssg?.ToString();
            return mssgObj;
        }
        private  IEnumerable<CovidResponseModel> GetData(int index, string pincode)
        {

            // need to change this to a recursive function with option of next days added in form in next update.
            var presentDate = DateTime.UtcNow.ToString("dd-MM-yyyy");
            var url = $"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByPin?pincode={pincode}&date={presentDate}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@$"{url}");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var content = new StreamReader(response.GetResponseStream());
            var resultData = JsonConvert.DeserializeObject< dynamic>(content.ReadToEnd());
            IEnumerable<CovidResponseModel> covidData = Enumerable.Empty<CovidResponseModel>();

            var next7DayDate = DateTime.UtcNow.AddDays(7).ToString("dd-MM-yyyy");
            var url7days = $"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByPin?pincode={pincode}&date={next7DayDate}";
            HttpWebRequest request7day = (HttpWebRequest)WebRequest.Create(@$"{url7days}");
            HttpWebResponse responseNext7Day = (HttpWebResponse)request7day.GetResponse();
            var contentNext7Day = new StreamReader(responseNext7Day.GetResponseStream());
            var resultDatanext7Day = JsonConvert.DeserializeObject<dynamic>(contentNext7Day.ReadToEnd());
            IEnumerable<CovidResponseModel> covidDatanext7Day = Enumerable.Empty<CovidResponseModel>();

            if (!(response.StatusCode == HttpStatusCode.OK) || !(responseNext7Day.StatusCode == HttpStatusCode.OK))
                return covidData;

            covidData = resultData.centers.ToObject<IEnumerable<CovidResponseModel>>();
            covidDatanext7Day = resultDatanext7Day.centers.ToObject<IEnumerable<CovidResponseModel>>();

            var concatenateCovidData = covidData.Concat(covidDatanext7Day);
            if (index == 0)
                return concatenateCovidData.Where(x => x.Sessions.Any(y => y.Min_age_limit == 18) && x.Sessions.Any(y => y.Available_capacity > 0));
            else
                return concatenateCovidData.Where(x => x.Sessions.Any(y => y.Available_capacity > 0));
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if(!Char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private  void timer1_Tick(object sender, EventArgs e)
        {
            var responses = GetData(comboBox1.SelectedIndex, textBox1.Text);
            var createmssg = CreateMessage(responses);
            if (!string.IsNullOrWhiteSpace(createmssg.mssg))
                this.Alert(createmssg.mssg, createmssg.totalCount, Form_Alert.enmType.Success);
          
               
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!Char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int i = 0;
            int.TryParse(textBox2.Text, out i);
            if(i == 0)
                errorProvider2.SetError(textBox2, "Interval should be greater than 0");
            else
                errorProvider2.SetError(textBox2, "");
        }

        public void Alert(string message, int totalCount, Form_Alert.enmType type)
        {
            Form_Alert alertForm = new Form_Alert();
            alertForm.showAlert(message, totalCount, type);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Dispose();
            timer1 = null;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

            bool cursorNotInBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);

            if (this.WindowState == FormWindowState.Minimized && cursorNotInBar)
            {
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
                this.Hide();
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.notifyIcon1.Visible = false;
            this.Visible = true;
            this.Show();
        }
    }
}
