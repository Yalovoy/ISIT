using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorldMapCountryInfo
{
    public partial class Form1 : Form
    {
        private GMapControl gMapControl;
        private Panel infoPanel;
        private PictureBox flagPictureBox;
        private Label countryNameLabel;
        private Label currencyLabel;
        private Label populationLabel;
        private GMapOverlay markersOverlay;
        private HttpClient httpClient;

        private readonly (string name, double lat, double lng)[] countries = new[]
        {
            ("Россия", 61.5240, 105.3188),
            ("США", 37.0902, -95.7129),
            ("Китай", 35.8617, 104.1954),
            ("Германия", 51.1657, 10.4515),
            ("Франция", 46.2276, 2.2137),
            ("Великобритания", 55.3781, -3.4360),
            ("Япония", 36.2048, 138.2529),
            ("Индия", 20.5937, 78.9629),
            ("Бразилия", -14.2350, -51.9253),
            ("Канада", 56.1304, -106.3468)
        };

        public Form1()
        {
            InitializeComponent();

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", "WorldMapCountryInfo/1.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            InitializeMap();
            InitializeInfoPanel();
        }

        private void InitializeMap()
        {
            gMapControl = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = GMapProviders.GoogleMap,
                Position = new PointLatLng(20, 0), 
                MinZoom = 2,
                MaxZoom = 18,
                Zoom = 3,
                ShowCenter = false,
                DragButton = MouseButtons.Left
            };

            gMapControl.OnMarkerClick += GMapControl_OnMarkerClick;
            gMapControl.MouseClick += GMapControl_MouseClick;

            markersOverlay = new GMapOverlay("markers");
            gMapControl.Overlays.Add(markersOverlay);

            AddCountryMarkers();

            this.Controls.Add(gMapControl);
        }

        private void AddCountryMarkers()
        {
            foreach (var country in countries)
            {
                var marker = new GMarkerGoogle(
                    new PointLatLng(country.lat, country.lng),
                    GMarkerGoogleType.blue_dot);

                marker.ToolTipText = country.name;
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.Tag = country.name; 
                markersOverlay.Markers.Add(marker);
            }
        }

        private void InitializeInfoPanel()
        {
            infoPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var titleLabel = new Label
            {
                Text = "Информация о стране",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            flagPictureBox = new PictureBox
            {
                Size = new Size(200, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                Location = new Point(50, 50),
                Parent = infoPanel
            };

            countryNameLabel = new Label
            {
                Text = "Выберите страну на карте",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, 180),
                Size = new Size(280, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Parent = infoPanel
            };

            currencyLabel = new Label
            {
                Text = "Валюта: -",
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                Location = new Point(10, 220),
                Size = new Size(280, 25),
                Parent = infoPanel
            };

            populationLabel = new Label
            {
                Text = "Население: -",
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                Location = new Point(10, 250),
                Size = new Size(280, 25),
                Parent = infoPanel
            };

            infoPanel.Controls.Add(titleLabel);
            this.Controls.Add(infoPanel);
        }

        private async void GMapControl_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (item.Tag != null && item.Tag is string countryName)
            {
                await LoadCountryInfo(countryName);
            }
        }

        private async void GMapControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                await GetCountryByCoordinates(point.Lat, point.Lng);
            }
        }

        private async Task GetCountryByCoordinates(double lat, double lng)
        {
            try
            {
                string url = $"https://api.bigdatacloud.net/data/reverse-geocode-client?latitude={lat}&longitude={lng}&localityLanguage=ru";

                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);


                var country = json["countryName"]?.ToString() ??
                             json["country"]?.ToString() ??
                             json["address"]?["country"]?.ToString();

                if (!string.IsNullOrEmpty(country))
                {
                    await LoadCountryInfo(country);
                }
                else
                {

                    await TryAlternativeGeocoding(lat, lng);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных по координатам: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task TryAlternativeGeocoding(double lat, double lng)
        {
            try
            {
                string url = $"http://api.geonames.org/countryCode?lat={lat}&lng={lng}&username=demo";

                var response = await httpClient.GetStringAsync(url);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    string countryCode = response.Trim();
                    await GetCountryNameByCode(countryCode);
                }
            }
            catch
            {
                MessageBox.Show("Не удалось определить страну по координатам", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task GetCountryNameByCode(string countryCode)
        {
            try
            {
                string url = $"https://restcountries.com/v3.1/alpha/{countryCode}";
                var response = await httpClient.GetStringAsync(url);
                var jsonArray = JArray.Parse(response);

                if (jsonArray.Count > 0)
                {
                    var countryData = jsonArray[0];
                    var countryName = countryData["name"]?["common"]?.ToString();

                    if (!string.IsNullOrEmpty(countryName))
                    {
                        await LoadCountryInfo(countryName);
                        return;
                    }
                }
            }
            catch
            { 

            }

            MessageBox.Show("Не удалось определить страну по координатам", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task LoadCountryInfo(string countryName)
        {
            try
            {
                countryNameLabel.Text = countryName;
                currencyLabel.Text = "Загрузка...";
                populationLabel.Text = "Загрузка...";
                flagPictureBox.Image = null;

                string countryData = await TryGetCountryData(countryName);

                if (!string.IsNullOrEmpty(countryData))
                {
                    ParseAndDisplayCountryInfo(countryData, countryName);
                }
                else
                {
                    ShowErrorInfo(countryName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                ShowErrorInfo(countryName);
            }
        }

        private async Task<string> TryGetCountryData(string countryName)
        {
            var apiUrls = new[]
            {
        $"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(countryName)}?fullText=true",
        
        $"https://restcountries.com/v2/name/{Uri.EscapeDataString(countryName)}",
        
        $"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(countryName)}",
        
        $"https://restcountries.com/v3.1/translation/{Uri.EscapeDataString(countryName)}"
    };

            foreach (var apiUrl in apiUrls)
            {
                try
                {
                    var response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        continue;
                    }

                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        private void ParseAndDisplayCountryInfo(string jsonData, string originalName)
        {
            try
            {
                var jsonArray = JArray.Parse(jsonData);

                if (jsonArray.Count > 0)
                {
                    var countryData = jsonArray[0];

                    string displayName = originalName;

                    var translations = countryData["translations"];
                    if (translations != null && translations["rus"] != null)
                    {
                        displayName = translations["rus"]["common"]?.ToString() ?? originalName;
                    }
                    else if (countryData["name"] != null)
                    {
                        var nameObj = countryData["name"];
                        displayName = nameObj["common"]?.ToString() ??
                                     nameObj["official"]?.ToString() ?? originalName;
                    }

                    string currency = "Неизвестно";
                    var currencies = countryData["currencies"];
                    if (currencies != null && currencies.HasValues)
                    {
                        var firstCurrency = (currencies as JObject)?.First;
                        if (firstCurrency != null)
                        {
                            currency = firstCurrency.First?["name"]?.ToString() ??
                                      firstCurrency.First?["symbol"]?.ToString() ?? "Неизвестно";
                        }
                    }

                    long population = 0;
                    if (countryData["population"] != null)
                    {
                        population = countryData["population"].Value<long>();
                    }

                    string capital = countryData["capital"]?.First?.ToString() ?? "Неизвестно";

                    string region = countryData["region"]?.ToString() ?? "Неизвестно";

                    string flagUrl = countryData["flags"]?["png"]?.ToString() ??
                                   countryData["flag"]?.ToString();
                    countryNameLabel.Text = displayName;
                    currencyLabel.Text = $"Валюта: {currency}";
                    populationLabel.Text = $"Население: {FormatPopulation(population)} чел.";

                    if (!string.IsNullOrEmpty(flagUrl))
                    {
                        _ = LoadFlagImage(flagUrl);
                    }
                    else
                    {
                        flagPictureBox.Image = null;
                    }

                    return;
                }
            }
            catch
            {
            }

            ShowBasicInfo(originalName);
        }

        private void ShowBasicInfo(string countryName)
        {
            var basicInfo = new Dictionary<string, (string currency, long population, string flagCode)>
    {
        {"Россия", ("Российский рубль (RUB)", 146150789, "ru")},
        {"США", ("Доллар США (USD)", 331002651, "us")},
        {"Китай", ("Китайский юань (CNY)", 1439323776, "cn")},
        {"Германия", ("Евро (EUR)", 83783942, "de")},
        {"Франция", ("Евро (EUR)", 65273511, "fr")},
        {"Великобритания", ("Фунт стерлингов (GBP)", 67886011, "gb")},
        {"Япония", ("Японская иена (JPY)", 126476461, "jp")},
        {"Индия", ("Индийская рупия (INR)", 1380004385, "in")},
        {"Бразилия", ("Бразильский реал (BRL)", 212559417, "br")},
        {"Канада", ("Канадский доллар (CAD)", 37742154, "ca")}
    };

            if (basicInfo.TryGetValue(countryName, out var info))
            {
                countryNameLabel.Text = countryName;
                currencyLabel.Text = $"Валюта: {info.currency}";
                populationLabel.Text = $"Население: {FormatPopulation(info.population)} чел.";

                string flagUrl = $"https://flagcdn.com/w320/{info.flagCode}.png";
                _ = LoadFlagImage(flagUrl);
            }
            else
            {
                ShowErrorInfo(countryName);
            }
        }

        private void ShowErrorInfo(string countryName)
        {
            countryNameLabel.Text = countryName;
            currencyLabel.Text = "Данные о валюте недоступны";
            populationLabel.Text = "Данные о населении недоступны";
            flagPictureBox.Image = null;

            MessageBox.Show($"Не удалось загрузить подробную информацию о {countryName}.\nПроверьте подключение к интернету.",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task LoadFlagImage(string url)
        {
            try
            {
                using (var stream = await httpClient.GetStreamAsync(url))
                {
                    flagPictureBox.Image = Image.FromStream(stream);
                }
            }
            catch
            {
                flagPictureBox.Image = null;
            }
        }

        private string FormatPopulation(long population)
        {
            if (population >= 1000000000)
                return $"{(population / 1000000000.0):F1} млрд";
            else if (population >= 1000000)
                return $"{(population / 1000000.0):F1} млн";
            else if (population >= 1000)
                return $"{(population / 1000.0):F1} тыс";
            else
                return population.ToString();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            httpClient?.Dispose();
            base.OnFormClosing(e);
        }
    }
}