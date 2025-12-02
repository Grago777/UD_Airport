using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class AirportsForm : Form
    {
        private IAirportService _airportService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;
        private ComboBox cmbCountryFilter;
        private Label lblTotalAirports;
        private Label lblCountriesCount;

        public AirportsForm()
        {
            InitializeComponent();
            _airportService = ServiceLocator.GetService<IAirportService>();

            dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
            LoadAirports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление аэропортами";
            this.Size = new System.Drawing.Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Padding = new Padding(10);

            // Основной контейнер
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // Панель фильтров
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Статистика
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Таблица
            mainLayout.ColumnCount = 1;

            // 1. Панель поиска и фильтров
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.BackColor = System.Drawing.Color.Honeydew;
            searchPanel.Padding = new Padding(5);

            // Первая строка
            var lblSearch = new Label() { Text = "Поиск:", Left = 5, Top = 10, Width = 45 };
            txtSearch = new TextBox() { Left = 50, Top = 8, Width = 150, PlaceholderText = "название, код, город..." };

            btnSearch = new Button() { Text = "Найти", Left = 210, Top = 8, Size = new System.Drawing.Size(60, 23) };
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button() { Text = "➕ Добавить", Left = 280, Top = 8, Size = new System.Drawing.Size(90, 23) };
            btnAdd.Click += BtnAdd_Click;

            // Вторая строка
            var lblCountry = new Label() { Text = "Страна:", Left = 5, Top = 40, Width = 45 };
            cmbCountryFilter = new ComboBox() { Left = 50, Top = 38, Width = 120 };
            cmbCountryFilter.Items.Add("Все страны");
            cmbCountryFilter.SelectedIndex = 0;
            cmbCountryFilter.SelectedIndexChanged += Filter_Changed;

            btnRefresh = new Button() { Text = "🔄 Обновить", Left = 180, Top = 38, Size = new System.Drawing.Size(80, 23) };
            btnRefresh.Click += (s, e) => LoadAirports();

            btnClose = new Button() { Text = "Закрыть", Left = 270, Top = 38, Size = new System.Drawing.Size(60, 23) };
            btnClose.Click += (s, e) => this.Close();

            searchPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, btnSearch, btnAdd,
                lblCountry, cmbCountryFilter, btnRefresh, btnClose
            });

            // 2. Панель статистики
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;
            statsPanel.BackColor = System.Drawing.Color.AliceBlue;
            statsPanel.Padding = new Padding(5);

            var lblStats = new Label() { Text = "Статистика:", Left = 5, Top = 5, Width = 60, Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold) };
            lblTotalAirports = new Label() { Text = "Аэропортов: 0", Left = 70, Top = 5, Width = 80 };
            lblCountriesCount = new Label() { Text = "Стран: 0", Left = 160, Top = 5, Width = 60 };

            statsPanel.Controls.AddRange(new Control[] {
                lblStats, lblTotalAirports, lblCountriesCount
            });

            // 3. DataGridView - основное пространство
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

            // Кнопки действий
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

            // Добавляем в layout
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

        private void LoadAirports()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var airports = _airportService.GetAllAirports();
                ApplyFilters(airports);
                LoadCountriesFilter(airports);
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки аэропортов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCountriesFilter(List<Airport> airports)
        {
            try
            {
                var countries = airports.Select(a => a.Country).Distinct().OrderBy(c => c).ToList();
                cmbCountryFilter.Items.Clear();
                cmbCountryFilter.Items.Add("Все страны");
                foreach (var country in countries)
                {
                    cmbCountryFilter.Items.Add(country);
                }
                cmbCountryFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фильтра стран: {ex.Message}");
            }
        }

        private void ApplyFilters(List<Airport> airports)
        {
            try
            {
                var filteredAirports = airports.AsEnumerable();

                // Фильтр по стране
                string selectedCountry = cmbCountryFilter.SelectedItem?.ToString();
                if (selectedCountry != "Все страны" && !string.IsNullOrEmpty(selectedCountry))
                {
                    filteredAirports = filteredAirports.Where(a => a.Country == selectedCountry);
                }

                // Поиск
                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredAirports = filteredAirports.Where(a =>
                        (a.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (a.IATACode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (a.City?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (a.Country?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    );
                }

                var result = filteredAirports.ToList();
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

                SetColumnPropertyIfExists("AirportId", "ID", 50);
                SetColumnPropertyIfExists("Name", "Название", 150);
                SetColumnPropertyIfExists("IATACode", "Код IATA", 70);
                SetColumnPropertyIfExists("Country", "Страна", 100);
                SetColumnPropertyIfExists("City", "Город", 100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования: {ex.Message}");
            }
        }

        private void UpdateStatistics(List<Airport> airports)
        {
            try
            {
                int total = airports.Count;
                int countries = airports.Select(a => a.Country).Distinct().Count();

                lblTotalAirports.Text = $"Аэропортов: {total}";
                lblCountriesCount.Text = $"Стран: {countries}";

                lblTotalAirports.ForeColor = total > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
                lblCountriesCount.ForeColor = countries > 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Gray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilters(_airportService.GetAllAirports());
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters(_airportService.GetAllAirports());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowAirportForm();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                if (!dataGridView.Columns.Contains("AirportId")) return;

                int airportId = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells["AirportId"].Value);

                if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
                {
                    ShowAirportForm(airportId);
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
                {
                    DeleteAirport(airportId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки клика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAirportForm(int airportId = 0)
        {
            try
            {
                using (var form = new AirportForm(airportId, _airportService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadAirports();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteAirport(int airportId)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить аэропорт ID {airportId}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_airportService.DeleteAirport(airportId))
                    {
                        MessageBox.Show("Аэропорт успешно удален", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAirports();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void RefreshData()
        {
            LoadAirports();
        }

        private void SetColumnPropertyIfExists(string columnName, string headerText, int width)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].HeaderText = headerText;
                dataGridView.Columns[columnName].Width = width;
            }
        }
    }
}