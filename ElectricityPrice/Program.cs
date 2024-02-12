using System;
using System.Drawing;
using System.Runtime.Caching;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using ElectricityPrice.Models;
using Newtonsoft.Json;

namespace ElectricityPrice
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static readonly HttpClient _client = new HttpClient();
        static PriceData _priceData = new PriceData();
        static System.Timers.Timer _timer = new System.Timers.Timer(60000);
        static int _lastHour = DateTime.Now.Hour;
        static ElectricityPrice _priceForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _priceData = GetPriceDataAsync().Result;

            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Start();

            _priceForm = new ElectricityPrice();

            SetFormLabels();
            SetFormStyles();

            Application.Run(_priceForm);
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (_lastHour < DateTime.Now.Hour || (_lastHour == 23 && DateTime.Now.Hour == 0)) {
                _lastHour = DateTime.Now.Hour;
                UpdateForm();  
            }
        }

        private static void UpdateForm()
        {
            Action action = delegate()
            {
                SetFormLabels();
                _priceForm.Icon = GetFormIcon();
            };

            // Need to do this way to access the same thread the form was originally created on
            _priceForm.BeginInvoke(action);
        }

        private static void SetFormLabels()
        {
            _priceForm.label1.Text = GetPriceLabelText(0);
            _priceForm.label2.Text = GetPriceLabelText(1);
            _priceForm.label3.Text = GetPriceLabelText(2);
            _priceForm.label4.Text = GetPriceLabelText(3);
            _priceForm.label5.Text = GetPriceLabelText(4);
            _priceForm.label6.Text = GetPriceLabelText(5);
            _priceForm.label7.Text = GetPriceLabelText(6);

            _priceForm.label10.Text = GetTimeLabelText(0);
            _priceForm.label11.Text = GetTimeLabelText(1);
            _priceForm.label12.Text = GetTimeLabelText(2);
            _priceForm.label13.Text = GetTimeLabelText(3);
            _priceForm.label14.Text = GetTimeLabelText(4);
            _priceForm.label15.Text = GetTimeLabelText(5);
            _priceForm.label16.Text = GetTimeLabelText(6);
        }

        private static void SetFormStyles()
        {
            _priceForm.Text = "Sähkön hinta";
            _priceForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            _priceForm.MinimizeBox = true;
            _priceForm.MaximizeBox = true;
            _priceForm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _priceForm.ShowInTaskbar = true;
            _priceForm.Icon = GetFormIcon();
        }

        private static string GetTimeLabelText(int hoursToAdd)
        {
            return DateTime.Now.AddHours(hoursToAdd).Hour.ToString("00.##") + "-" + DateTime.Now.AddHours(hoursToAdd + 1).Hour.ToString("00.##");
        }

        private static string GetPriceLabelText(int hoursToAdd)
        {
            return GetHourPrice(GetFormattedDate(hoursToAdd)) + " c/kWh";
        }

        private static string GetHourPrice(string date)
        {
            if (_priceData != null && _priceData.PriceValues.Count > 0)
            {
                var priceVal = Math.Round(_priceData.PriceValues.FirstOrDefault(p => p.StartDate == date).PriceValue, 1);
                return priceVal.ToString("0.0");
            }

            return "-";
        }

        private static string GetFormattedDate(int hoursFromNow)
        {
            return DateTime.UtcNow.AddHours(hoursFromNow).ToString("yyyy-MM-ddTHH") + ":00:00.000Z";
        }

        private static Icon GetFormIcon()
        {
            var imgBitmap = Properties.Resources.Circle_icons_bolt_svg;

            using (Graphics g = Graphics.FromImage(imgBitmap))
            {
                var currentPrice = GetHourPrice(GetFormattedDate(0));
                g.DrawString(currentPrice, new Font("Arial Black", 200, FontStyle.Bold), Brushes.White, new PointF(0f, 0f));
            }

            return BitmapToIcon(imgBitmap);
        }

        private static Icon BitmapToIcon(Bitmap bitmap)
        { 
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private static async Task<PriceData> GetPriceDataAsync()
        {
            try
            {
                var response = _client.GetAsync("https://api.porssisahko.net/v1/latest-prices.json").Result;
                response.EnsureSuccessStatusCode();
                var stringRes = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PriceData>(stringRes);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
