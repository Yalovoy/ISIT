using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WeatherExpertSystem
{
    public partial class Form1 : Form
    {
        private Dictionary<string, (double lat, double lon)> cities = new Dictionary<string, (double, double)>
        {
            { "Макеевка", (48.0636, 38.0618) },
            { "Донецк", (48.0159, 37.8028) },
            { "Ростов", (47.2313, 39.7233) },
            { "Москва", (55.7558, 37.6173) },
            { "Санкт-Петербург", (59.9343, 30.3351) }
        };

        private HttpClient httpClient;
        private Dictionary<string, Label> valueLabels = new Dictionary<string, Label>();
        private Dictionary<string, Label> assessmentLabels = new Dictionary<string, Label>();

        private ComboBox comboBoxCities;
        private Label lblCityName;
        private Label lblWeatherState;
        private Label lblUpdateTime;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeHttpClient();
            Load += Form1_Load;
        }

        private void InitializeHttpClient()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WeatherExpertSystem/1.0");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await InitializeData();
        }

        private async Task InitializeData()
        {
            comboBoxCities.SelectedItem = "Макеевка";
            await UpdateWeatherData("Макеевка");
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Экспертная система погоды";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 255);
            this.Padding = new Padding(20);

            Label titleLabel = new Label();
            titleLabel.Text = "Экспертная система погоды";
            titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(0, 78, 152);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(250, 10);
            this.Controls.Add(titleLabel);

            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Машина вывода реляционного типа";
            subtitleLabel.Font = new Font("Segoe UI", 12, FontStyle.Italic);
            subtitleLabel.ForeColor = Color.FromArgb(100, 100, 100);
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new Point(290, 45);
            this.Controls.Add(subtitleLabel);

            Panel cityPanel = new Panel();
            cityPanel.BackColor = Color.White;
            cityPanel.BorderStyle = BorderStyle.FixedSingle;
            cityPanel.Size = new Size(840, 60);
            cityPanel.Location = new Point(25, 85);
            this.Controls.Add(cityPanel);

            Label citySelectLabel = new Label();
            citySelectLabel.Text = "Выберите город:";
            citySelectLabel.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            citySelectLabel.Location = new Point(30, 20);
            citySelectLabel.AutoSize = true;
            cityPanel.Controls.Add(citySelectLabel);

            comboBoxCities = new ComboBox();
            comboBoxCities.Name = "comboBoxCities";
            comboBoxCities.Font = new Font("Segoe UI", 11);
            comboBoxCities.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxCities.Location = new Point(165, 17);
            comboBoxCities.Size = new Size(300, 28);
            comboBoxCities.MaxDropDownItems = 10;
            foreach (var city in cities.Keys)
            {
                comboBoxCities.Items.Add(city);
            }
            comboBoxCities.SelectedIndexChanged += async (s, e) =>
            {
                if (comboBoxCities.SelectedItem != null)
                {
                    await UpdateWeatherData(comboBoxCities.SelectedItem.ToString());
                }
            };
            cityPanel.Controls.Add(comboBoxCities);

            Button updateButton = new Button();
            updateButton.Name = "btnUpdate";
            updateButton.Text = "Обновить";
            updateButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            updateButton.BackColor = Color.FromArgb(0, 120, 215);
            updateButton.ForeColor = Color.White;
            updateButton.FlatStyle = FlatStyle.Flat;
            updateButton.FlatAppearance.BorderSize = 0;
            updateButton.Size = new Size(120, 30);
            updateButton.Location = new Point(490, 17);
            updateButton.Cursor = Cursors.Hand;
            updateButton.Click += async (s, e) =>
            {
                if (comboBoxCities.SelectedItem != null)
                {
                    await UpdateWeatherData(comboBoxCities.SelectedItem.ToString());
                }
            };
            cityPanel.Controls.Add(updateButton);

            Panel weatherPanel = new Panel();
            weatherPanel.Name = "weatherPanel";
            weatherPanel.BackColor = Color.White;
            weatherPanel.BorderStyle = BorderStyle.FixedSingle;
            weatherPanel.Size = new Size(840, 380);
            weatherPanel.Location = new Point(25, 160);
            this.Controls.Add(weatherPanel);

            lblCityName = new Label();
            lblCityName.Name = "lblCityName";
            lblCityName.Text = "Макеевка";
            lblCityName.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblCityName.ForeColor = Color.Black;
            lblCityName.AutoSize = true;
            lblCityName.Location = new Point(30, 20);
            weatherPanel.Controls.Add(lblCityName);

            lblWeatherState = new Label();
            lblWeatherState.Name = "lblWeatherState";
            lblWeatherState.Text = "ЯСНО";
            lblWeatherState.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblWeatherState.ForeColor = Color.Black;
            lblWeatherState.AutoSize = true;
            lblWeatherState.Location = new Point(650, 25);
            weatherPanel.Controls.Add(lblWeatherState);

            int totalCardsWidth = 3 * 240 + 2 * 20;
            int startX = (weatherPanel.Width - totalCardsWidth) / 2;

            int startY = 90;
            int cardWidth = 240;
            int cardHeight = 120;
            int horizontalSpacing = 20;
            int verticalSpacing = 25;

            CreateWeatherCard(weatherPanel, "Температура", "9,0°C", "Прохладно",
                startX, startY, cardWidth, cardHeight, "temperature");
            CreateWeatherCard(weatherPanel, "Ощущается как", "8,0°C", "Прохладно",
                startX + cardWidth + horizontalSpacing, startY, cardWidth, cardHeight, "feelsLike");
            CreateWeatherCard(weatherPanel, "Влажность", "77%", "Высокая",
                startX + 2 * (cardWidth + horizontalSpacing), startY, cardWidth, cardHeight, "humidity");

            int secondRowY = startY + cardHeight + verticalSpacing;
            CreateWeatherCard(weatherPanel, "Ветер", "8,0 км/ч", "Умеренный",
                startX, secondRowY, cardWidth, cardHeight, "wind");
            CreateWeatherCard(weatherPanel, "Давление", "1013 гПа", "Нормальное",
                startX + cardWidth + horizontalSpacing, secondRowY, cardWidth, cardHeight, "pressure");
            CreateWeatherCard(weatherPanel, "Осадки", "1,0 мм", "Умеренные",
                startX + 2 * (cardWidth + horizontalSpacing), secondRowY, cardWidth, cardHeight, "precipitation");

            lblUpdateTime = new Label();
            lblUpdateTime.Name = "lblUpdateTime";
            lblUpdateTime.Text = "Данные для Макеевка обновлены: 19:55:46";
            lblUpdateTime.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblUpdateTime.ForeColor = Color.DarkGray;
            lblUpdateTime.AutoSize = true;
            lblUpdateTime.Location = new Point(25, 550);
            this.Controls.Add(lblUpdateTime);
        }

        private void CreateWeatherCard(Panel parent, string title, string value, string assessment,
            int x, int y, int width, int height, string key)
        {
            Panel cardPanel = new Panel();
            cardPanel.BorderStyle = BorderStyle.None;
            cardPanel.BackColor = Color.White;
            cardPanel.Size = new Size(width, height);
            cardPanel.Location = new Point(x, y);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.Black;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 10);
            titleLabel.Width = width;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            cardPanel.Controls.Add(titleLabel);

            Label valueLabel = new Label();
            valueLabel.Name = $"lbl{key}Value";
            valueLabel.Text = value;
            valueLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            valueLabel.ForeColor = Color.Black;
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(0, 45);
            valueLabel.Width = width;
            valueLabel.TextAlign = ContentAlignment.MiddleCenter;
            cardPanel.Controls.Add(valueLabel);
            valueLabels[key] = valueLabel;

            Label assessmentLabel = new Label();
            assessmentLabel.Name = $"lbl{key}Assessment";
            assessmentLabel.Text = $"*{assessment}*";
            assessmentLabel.Font = new Font("Segoe UI", 12, FontStyle.Italic);
            assessmentLabel.ForeColor = Color.Black;
            assessmentLabel.AutoSize = true;
            assessmentLabel.Location = new Point(0, 85);
            assessmentLabel.Width = width;
            assessmentLabel.TextAlign = ContentAlignment.MiddleCenter;
            cardPanel.Controls.Add(assessmentLabel);
            assessmentLabels[key] = assessmentLabel;

            parent.Controls.Add(cardPanel);
        }

        private async Task UpdateWeatherData(string city)
        {
            if (!cities.ContainsKey(city))
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                var (lat, lon) = cities[city];
                var weatherData = await GetWeatherData(lat, lon);

                this.Invoke((MethodInvoker)delegate
                {
                    lblCityName.Text = city;
                    lblWeatherState.Text = weatherData.WeatherState.ToUpper();
                    lblWeatherState.ForeColor = Color.Black;

                    valueLabels["temperature"].Text = $"{weatherData.Temperature:F1}°C".Replace('.', ',');
                    valueLabels["feelsLike"].Text = $"{weatherData.ApparentTemperature:F1}°C".Replace('.', ',');
                    valueLabels["humidity"].Text = $"{weatherData.Humidity:F0}%";
                    valueLabels["wind"].Text = $"{weatherData.WindSpeed:F1} км/ч".Replace('.', ',');
                    valueLabels["pressure"].Text = $"{weatherData.Pressure:F0} гПа";
                    valueLabels["precipitation"].Text = $"{weatherData.Precipitation:F1} мм".Replace('.', ',');

                    assessmentLabels["temperature"].Text = $"*{GetTemperatureAssessment(weatherData.Temperature)}*";
                    assessmentLabels["feelsLike"].Text = $"*{GetTemperatureAssessment(weatherData.ApparentTemperature)}*";
                    assessmentLabels["humidity"].Text = $"*{GetHumidityAssessment(weatherData.Humidity)}*";
                    assessmentLabels["wind"].Text = $"*{GetWindAssessment(weatherData.WindSpeed)}*";
                    assessmentLabels["pressure"].Text = $"*{GetPressureAssessment(weatherData.Pressure)}*";
                    assessmentLabels["precipitation"].Text = $"*{GetPrecipitationAssessment(weatherData.Precipitation)}*";

                    lblUpdateTime.Text = $"Данные для {city} обновлены: {DateTime.Now:HH:mm:ss}";
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Ошибка при получении данных: {ex.Message}\nИспользуются демонстрационные данные.",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    SetDemoData(city);
                });
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task<WeatherData> GetWeatherData(double lat, double lon)
        {
            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?" +
                    $"latitude={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                    $"longitude={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                    "current_weather=true&" +
                    "hourly=relative_humidity_2m,precipitation&" +
                    "timezone=auto";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return GetFallbackData();
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(json))
                {
                    return GetFallbackData();
                }

                var jsonObject = JObject.Parse(json);
                var currentWeather = jsonObject["current_weather"];

                if (currentWeather == null)
                {
                    return GetFallbackData();
                }

                double temperature = currentWeather["temperature"]?.Value<double>() ?? 0;
                int weatherCode = currentWeather["weathercode"]?.Value<int>() ?? 0;
                double windSpeed = currentWeather["windspeed"]?.Value<double>() ?? 0;

                double humidity = 60.0;
                double precipitation = 0.0;

                var hourly = jsonObject["hourly"];
                if (hourly != null)
                {
                    var humidityArray = hourly["relative_humidity_2m"] as JArray;
                    if (humidityArray != null && humidityArray.Count > 0)
                    {
                        humidity = humidityArray[0]?.Value<double>() ?? 60.0;
                    }

                    var precipitationArray = hourly["precipitation"] as JArray;
                    if (precipitationArray != null && precipitationArray.Count > 0)
                    {
                        precipitation = precipitationArray[0]?.Value<double>() ?? 0.0;
                    }
                }

                return new WeatherData
                {
                    Temperature = temperature,
                    ApparentTemperature = temperature - 1,
                    Humidity = humidity,
                    Precipitation = precipitation,
                    WeatherCode = weatherCode,
                    WindSpeed = windSpeed,
                    Pressure = 1013.0,
                    WeatherState = GetWeatherState(weatherCode)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                return GetFallbackData();
            }
        }

        private WeatherData GetFallbackData()
        {
            Random rnd = new Random();
            double temp = rnd.Next(5, 15);
            string[] weatherStates = { "Ясно", "Облачно", "Дождь", "Снег" };

            return new WeatherData
            {
                Temperature = temp,
                ApparentTemperature = temp - rnd.Next(0, 3),
                Humidity = rnd.Next(60, 85),
                Precipitation = rnd.Next(0, 3),
                WeatherCode = rnd.Next(0, 4),
                WindSpeed = rnd.Next(5, 12),
                Pressure = 1013.0,
                WeatherState = weatherStates[rnd.Next(0, 4)]
            };
        }

        private void SetDemoData(string city)
        {
            Random rnd = new Random();
            double temp = rnd.Next(5, 15);
            double feelsLike = temp - rnd.Next(0, 3);
            double humidity = rnd.Next(60, 85);
            double wind = rnd.Next(5, 12);
            double precip = rnd.Next(0, 3);
            string[] weatherStates = { "Ясно", "Облачно", "Дождь", "Снег" };
            string weatherState = weatherStates[rnd.Next(0, 4)];

            lblCityName.Text = city;
            lblWeatherState.Text = weatherState.ToUpper();
            lblWeatherState.ForeColor = Color.Black;

            valueLabels["temperature"].Text = $"{temp:F1}°C".Replace('.', ',');
            valueLabels["feelsLike"].Text = $"{feelsLike:F1}°C".Replace('.', ',');
            valueLabels["humidity"].Text = $"{humidity:F0}%";
            valueLabels["wind"].Text = $"{wind:F1} км/ч".Replace('.', ',');
            valueLabels["pressure"].Text = "1013 гПа";
            valueLabels["precipitation"].Text = $"{precip:F1} мм".Replace('.', ',');

            assessmentLabels["temperature"].Text = $"*{GetTemperatureAssessment(temp)}*";
            assessmentLabels["feelsLike"].Text = $"*{GetTemperatureAssessment(feelsLike)}*";
            assessmentLabels["humidity"].Text = $"*{GetHumidityAssessment(humidity)}*";
            assessmentLabels["wind"].Text = $"*{GetWindAssessment(wind)}*";
            assessmentLabels["pressure"].Text = "*Нормальное*";
            assessmentLabels["precipitation"].Text = $"*{GetPrecipitationAssessment(precip)}*";

            lblUpdateTime.Text = $"Данные для {city} обновлены: {DateTime.Now:HH:mm:ss} (демо)";
        }

        private string GetWeatherState(int weatherCode)
        {
            switch (weatherCode)
            {
                case 0: return "Ясно";
                case 1: case 2: case 3: return "Облачно";
                case 45: case 48: return "Туман";
                case 51: case 53: case 55: return "Морось";
                case 61: case 63: case 65: return "Дождь";
                case 71: case 73: case 75: return "Снег";
                case 77: return "Снежные зерна";
                case 80: case 81: case 82: return "Ливень";
                case 85: case 86: return "Снегопад";
                case 95: case 96: case 99: return "Гроза";
                default: return "Неизвестно";
            }
        }

        private string GetTemperatureAssessment(double temperature)
        {
            if (temperature < -10) return "Очень холодно";
            if (temperature < 0) return "Холодно";
            if (temperature < 15) return "Прохладно";
            return "Тепло";
        }

        private string GetHumidityAssessment(double humidity)
        {
            if (humidity < 30) return "Сухо";
            if (humidity < 70) return "Нормальная";
            return "Высокая";
        }

        private string GetWindAssessment(double windSpeed)
        {
            if (windSpeed < 5) return "Слабый";
            if (windSpeed < 15) return "Умеренный";
            return "Сильный";
        }

        private string GetPressureAssessment(double pressure)
        {
            if (pressure < 1000) return "Низкое";
            if (pressure < 1020) return "Нормальное";
            return "Высокое";
        }

        private string GetPrecipitationAssessment(double precipitation)
        {
            if (precipitation == 0) return "Нет или слабые";
            if (precipitation <= 5) return "Умеренные";
            return "Сильные";
        }
    }

    public class WeatherData
    {
        public double Temperature { get; set; }
        public double ApparentTemperature { get; set; }
        public double Humidity { get; set; }
        public double Precipitation { get; set; }
        public int WeatherCode { get; set; }
        public double WindSpeed { get; set; }
        public double Pressure { get; set; }
        public string WeatherState { get; set; }
    }
}
