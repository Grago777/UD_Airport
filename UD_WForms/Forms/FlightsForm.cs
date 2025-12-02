using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class FlightsForm : Form
    {
        private IFlightService _flightService;
        private IAirportService _airportService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;
        private Button btnExport;
        private ComboBox cmbStatusFilter;
        private ComboBox cmbAirlineFilter;
        private DateTimePicker dtpDateFilter;
        private CheckBox chkUseDateFilter;
        private Label lblTotalFlights;
        private Label lblActiveFlights;

        public FlightsForm()
        {
            InitializeComponent();
            _flightService = ServiceLocator.GetService<IFlightService>();
            _airportService = ServiceLocator.GetService<IAirportService>();

            dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
            LoadFlights();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление рейсами";
            this.Size = new System.Drawing.Size(1000, 600); // Уменьшил размер формы
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Padding = new Padding(10);

            // Основной контейнер с TableLayoutPanel для лучшего управления layout
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Панель фильтров
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Статистика
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Таблица
            mainLayout.ColumnCount = 1;

            // 1. Панель поиска и фильтров - компактная
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.BackColor = System.Drawing.Color.Lavender;
            searchPanel.Padding = new Padding(5);

            // Первая строка - поиск и основные кнопки
            var lblSearch = new Label() { Text = "Поиск:", Left = 5, Top = 10, Width = 45 };
            txtSearch = new TextBox() { Left = 50, Top = 8, Width = 150, PlaceholderText = "номер, авиакомпания..." };

            btnSearch = new Button() { Text = "Найти", Left = 210, Top = 8, Size = new System.Drawing.Size(60, 23) };
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button() { Text = "➕ Добавить", Left = 280, Top = 8, Size = new System.Drawing.Size(90, 23) };
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button() { Text = "🔄 Обновить", Left = 380, Top = 8, Size = new System.Drawing.Size(80, 23) };
            btnRefresh.Click += (s, e) => LoadFlights();

            // Вторая строка - фильтры
            var lblStatus = new Label() { Text = "Статус:", Left = 5, Top = 40, Width = 45 };
            cmbStatusFilter = new ComboBox() { Left = 50, Top = 38, Width = 100 };
            cmbStatusFilter.Items.AddRange(new string[] { "Все", "По расписанию", "Задержан", "Отменен", "Вылетел", "Прибыл" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += Filter_Changed;

            var lblAirline = new Label() { Text = "Авиакомпания:", Left = 160, Top = 40, Width = 75 };
            cmbAirlineFilter = new ComboBox() { Left = 235, Top = 38, Width = 120 };
            cmbAirlineFilter.Items.Add("Все");
            cmbAirlineFilter.SelectedIndex = 0;
            cmbAirlineFilter.SelectedIndexChanged += Filter_Changed;

            var lblDate = new Label() { Text = "Дата:", Left = 365, Top = 40, Width = 35 };
            dtpDateFilter = new DateTimePicker() { Left = 400, Top = 38, Width = 90, Format = DateTimePickerFormat.Short };
            dtpDateFilter.ValueChanged += Filter_Changed;

            chkUseDateFilter = new CheckBox() { Text = "Фильтр по дате", Left = 500, Top = 40, Width = 150 };
            chkUseDateFilter.CheckedChanged += Filter_Changed;

            // Третья строка - дополнительные кнопки
            btnExport = new Button() { Text = "📊 Экспорт", Left = 5, Top = 70, Size = new System.Drawing.Size(80, 23) };
            btnExport.Click += BtnExport_Click;

            btnClose = new Button() { Text = "Закрыть", Left = 95, Top = 70, Size = new System.Drawing.Size(80, 23) };
            btnClose.Click += (s, e) => this.Close();

            searchPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, btnSearch, btnAdd, btnRefresh,
                lblStatus, cmbStatusFilter, lblAirline, cmbAirlineFilter,
                lblDate, dtpDateFilter, chkUseDateFilter,
                btnExport, btnClose
            });

            // 2. Панель статистики - очень компактная
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;
            statsPanel.BackColor = System.Drawing.Color.AliceBlue;
            statsPanel.Padding = new Padding(5);

            var lblStats = new Label() { Text = "Статистика:", Left = 5, Top = 5, Width = 60, Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold) };
            lblTotalFlights = new Label() { Text = "Всего: 0", Left = 70, Top = 5, Width = 50 };
            lblActiveFlights = new Label() { Text = "Активных: 0", Left = 130, Top = 5, Width = 70 };
            var lblDelayed = new Label() { Text = "Задержано: 0", Left = 210, Top = 5, Width = 70 };
            var lblCancelled = new Label() { Text = "Отменено: 0", Left = 290, Top = 5, Width = 70 };

            statsPanel.Controls.AddRange(new Control[] {
                lblStats, lblTotalFlights, lblActiveFlights, lblDelayed, lblCancelled
            });

            // 3. DataGridView - занимает основное пространство
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.BackgroundColor = System.Drawing.Color.White;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.CellClick += DataGridView_CellClick;

            // Добавляем кнопки действий
            var editColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = " ",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 40
            };

            var deleteColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = " ",
                Text = "🗑️",
                UseColumnTextForButtonValue = true,
                Width = 40
            };

            dataGridView.Columns.Add(editColumn);
            dataGridView.Columns.Add(deleteColumn);

            // Добавляем все в основной layout
            mainLayout.Controls.Add(searchPanel, 0, 0);
            mainLayout.Controls.Add(statsPanel, 0, 1);
            mainLayout.Controls.Add(dataGridView, 0, 2);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            FormatDataGridView();
        }

        private void LoadFlights()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var flights = _flightService.GetAllFlights();
                ApplyFilters(flights);
                LoadAirlinesFilter(flights);

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки рейсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAirlinesFilter(List<Flight> flights)
        {
            try
            {
                var airlines = flights.Select(f => f.Airline).Distinct().OrderBy(a => a).ToList();
                cmbAirlineFilter.Items.Clear();
                cmbAirlineFilter.Items.Add("Все");
                foreach (var airline in airlines)
                {
                    cmbAirlineFilter.Items.Add(airline);
                }
                cmbAirlineFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фильтра авиакомпаний: {ex.Message}");
            }
        }

        private void ApplyFilters(List<Flight> flights)
        {
            try
            {
                var filteredFlights = flights.AsEnumerable();

                // Фильтр по статусу
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString();
                if (selectedStatus != "Все" && !string.IsNullOrEmpty(selectedStatus))
                {
                    filteredFlights = filteredFlights.Where(f => f.Status == selectedStatus);
                }

                // Фильтр по авиакомпании
                string selectedAirline = cmbAirlineFilter.SelectedItem?.ToString();
                if (selectedAirline != "Все" && !string.IsNullOrEmpty(selectedAirline))
                {
                    filteredFlights = filteredFlights.Where(f => f.Airline == selectedAirline);
                }

                // Фильтр по дате
                if (chkUseDateFilter.Checked)
                {
                    filteredFlights = filteredFlights.Where(f => f.DepartureDate.Date == dtpDateFilter.Value.Date);
                }

                // Поиск
                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredFlights = filteredFlights.Where(f =>
                        (f.FlightNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (f.Airline?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (f.Aircraft?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    );
                }

                var result = filteredFlights.ToList();
                dataGridView.DataSource = null;
                dataGridView.DataSource = result;
                UpdateStatistics(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridView()
        {
            try
            {
                if (dataGridView.Columns.Count == 0) return;

                // Устанавливаем свойства колонок
                SetColumnPropertyIfExists("FlightNumber", "Номер", 80);
                SetColumnPropertyIfExists("FlightType", "Тип", 60);
                SetColumnPropertyIfExists("Aircraft", "Самолет", 100);
                SetColumnPropertyIfExists("DepartureDate", "Вылет", 120);
                SetColumnPropertyIfExists("ArrivalDate", "Прибытие", 120);
                SetColumnPropertyIfExists("FlightTime", "Время", 60);
                SetColumnPropertyIfExists("Status", "Статус", 80);
                SetColumnPropertyIfExists("EconomySeats", "Эконом", 50);
                SetColumnPropertyIfExists("BusinessSeats", "Бизнес", 50);
                SetColumnPropertyIfExists("Airline", "Авиакомпания", 100);

                // Форматирование дат
                if (dataGridView.Columns.Contains("DepartureDate"))
                {
                    dataGridView.Columns["DepartureDate"].DefaultCellStyle.Format = "dd.MM.yy HH:mm";
                }

                if (dataGridView.Columns.Contains("ArrivalDate"))
                {
                    dataGridView.Columns["ArrivalDate"].DefaultCellStyle.Format = "dd.MM.yy HH:mm";
                }

                // Скрываем ID аэропортов
                SetColumnVisibilityIfExists("DepartureAirportId", false);
                SetColumnVisibilityIfExists("ArrivalAirportId", false);

                // Подписываемся на события форматирования
                dataGridView.CellFormatting -= DataGridView_CellFormatting;
                dataGridView.CellFormatting += DataGridView_CellFormatting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования DataGridView: {ex.Message}");
            }
        }

        private void SetColumnPropertyIfExists(string columnName, string headerText, int width)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].HeaderText = headerText;
                dataGridView.Columns[columnName].Width = width;
            }
        }

        private void SetColumnVisibilityIfExists(string columnName, bool visible)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].Visible = visible;
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (!dataGridView.Columns.Contains("Status")) return;

                var column = dataGridView.Columns[e.ColumnIndex];
                if (column.Name == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString();
                    switch (status)
                    {
                        case "По расписанию":
                            e.CellStyle.BackColor = System.Drawing.Color.PaleGreen;
                            e.CellStyle.ForeColor = System.Drawing.Color.DarkGreen;
                            break;
                        case "Задержан":
                            e.CellStyle.BackColor = System.Drawing.Color.LightYellow;
                            e.CellStyle.ForeColor = System.Drawing.Color.OrangeRed;
                            break;
                        case "Отменен":
                            e.CellStyle.BackColor = System.Drawing.Color.LightCoral;
                            e.CellStyle.ForeColor = System.Drawing.Color.DarkRed;
                            break;
                        case "Вылетел":
                            e.CellStyle.BackColor = System.Drawing.Color.LightBlue;
                            e.CellStyle.ForeColor = System.Drawing.Color.DarkBlue;
                            break;
                        case "Прибыл":
                            e.CellStyle.BackColor = System.Drawing.Color.LightGray;
                            e.CellStyle.ForeColor = System.Drawing.Color.Black;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования ячейки: {ex.Message}");
            }
        }

        private void UpdateStatistics(List<Flight> flights)
        {
            try
            {
                int total = flights.Count;
                int active = flights.Count(f => f.Status == "По расписанию" || f.Status == "Вылетел");
                int delayed = flights.Count(f => f.Status == "Задержан");
                int cancelled = flights.Count(f => f.Status == "Отменен");

                lblTotalFlights.Text = $"Всего: {total}";
                lblActiveFlights.Text = $"Активных: {active}";

                // Обновляем остальные labels статистики
                foreach (Control control in this.Controls[0].Controls[1].Controls) // statsPanel
                {
                    if (control is Label label)
                    {
                        if (label.Text.StartsWith("Задержано:"))
                            label.Text = $"Задержано: {delayed}";
                        else if (label.Text.StartsWith("Отменено:"))
                            label.Text = $"Отменено: {cancelled}";
                    }
                }

                // Обновляем цвет статистики
                lblTotalFlights.ForeColor = total > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
                lblActiveFlights.ForeColor = active > 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Gray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления статистики: {ex.Message}");
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilters(_flightService.GetAllFlights());
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters(_flightService.GetAllFlights());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowFlightForm();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                if (!dataGridView.Columns.Contains("FlightNumber")) return;

                string flightNumber = dataGridView.Rows[e.RowIndex].Cells["FlightNumber"].Value?.ToString();
                if (string.IsNullOrEmpty(flightNumber))
                {
                    Console.WriteLine("Номер рейса не найден в строке");
                    return;
                }

                Console.WriteLine($"Выбран рейс для редактирования: {flightNumber}");

                if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
                {
                    Console.WriteLine($"Открытие формы редактирования для рейса: {flightNumber}");
                    ShowFlightForm(flightNumber);
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
                {
                    DeleteFlight(flightNumber);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клика: {ex.Message}");
                MessageBox.Show($"Ошибка обработки клика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowFlightForm(string flightNumber = null)
        {
            try
            {
                using (var form = new FlightForm(flightNumber, _flightService, _airportService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadFlights();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteFlight(string flightNumber)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить рейс {flightNumber}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_flightService.DeleteFlight(flightNumber))
                    {
                        MessageBox.Show("Рейс успешно удален", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadFlights();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                saveDialog.Title = "Экспорт рейсов";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine("Номер рейса;Тип;Самолет;Вылет;Прибытие;Время;Статус;Эконом;Бизнес;Авиакомпания");

                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string GetSafeValue(string columnName)
                            {
                                if (dataGridView.Columns.Contains(columnName) && row.Cells[columnName].Value != null)
                                    return row.Cells[columnName].Value.ToString();
                                return "";
                            }

                            writer.WriteLine(
                                $"{GetSafeValue("FlightNumber")};" +
                                $"{GetSafeValue("FlightType")};" +
                                $"{GetSafeValue("Aircraft")};" +
                                $"{GetSafeValue("DepartureDate")};" +
                                $"{GetSafeValue("ArrivalDate")};" +
                                $"{GetSafeValue("FlightTime")};" +
                                $"{GetSafeValue("Status")};" +
                                $"{GetSafeValue("EconomySeats")};" +
                                $"{GetSafeValue("BusinessSeats")};" +
                                $"{GetSafeValue("Airline")}"
                            );
                        }
                    }

                    MessageBox.Show($"Данные успешно экспортированы", "Экспорт",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshData()
        {
            LoadFlights();
        }
    }
}