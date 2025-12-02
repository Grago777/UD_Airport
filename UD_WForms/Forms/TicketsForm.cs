using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class TicketsForm : Form
    {
        private ITicketService _ticketService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;
        private Button btnExport;
        private ComboBox cmbStatusFilter;
        private ComboBox cmbClassFilter;
        private Label lblTotalTickets;
        private Label lblTotalRevenue;

        public TicketsForm()
        {
            InitializeComponent();
            _ticketService = ServiceLocator.GetService<ITicketService>();

            dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
            LoadTickets();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление билетами";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Padding = new Padding(10);

            // Основной контейнер с TableLayoutPanel
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Панель фильтров
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Статистика
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Таблица
            mainLayout.ColumnCount = 1;

            // 1. Панель поиска и фильтров
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.BackColor = System.Drawing.Color.Lavender;
            searchPanel.Padding = new Padding(5);

            // Первая строка - поиск и кнопки
            var lblSearch = new Label() { Text = "Поиск:", Left = 5, Top = 10, Width = 45 };
            txtSearch = new TextBox() { Left = 50, Top = 8, Width = 150, PlaceholderText = "номер билета, рейса..." };

            btnSearch = new Button() { Text = "Найти", Left = 210, Top = 8, Size = new System.Drawing.Size(60, 23) };
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button() { Text = "➕ Продать", Left = 280, Top = 8, Size = new System.Drawing.Size(80, 23) };
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button() { Text = "🔄 Обновить", Left = 370, Top = 8, Size = new System.Drawing.Size(80, 23) };
            btnRefresh.Click += (s, e) => LoadTickets();

            // Вторая строка - фильтры
            var lblStatus = new Label() { Text = "Статус:", Left = 5, Top = 40, Width = 45 };
            cmbStatusFilter = new ComboBox() { Left = 50, Top = 38, Width = 100 };
            cmbStatusFilter.Items.AddRange(new string[] { "Все", "Активен", "Использован", "Возвращен", "Отменен" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += Filter_Changed;

            var lblClass = new Label() { Text = "Класс:", Left = 160, Top = 40, Width = 40 };
            cmbClassFilter = new ComboBox() { Left = 200, Top = 38, Width = 80 };
            cmbClassFilter.Items.AddRange(new string[] { "Все", "Эконом", "Бизнес", "Первый" });
            cmbClassFilter.SelectedIndex = 0;
            cmbClassFilter.SelectedIndexChanged += Filter_Changed;

            // Третья строка - кнопки
            btnExport = new Button() { Text = "📊 Экспорт", Left = 5, Top = 70, Size = new System.Drawing.Size(80, 23) };
            btnExport.Click += BtnExport_Click;

            btnClose = new Button() { Text = "Закрыть", Left = 95, Top = 70, Size = new System.Drawing.Size(80, 23) };
            btnClose.Click += (s, e) => this.Close();

            searchPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, btnSearch, btnAdd, btnRefresh,
                lblStatus, cmbStatusFilter, lblClass, cmbClassFilter,
                btnExport, btnClose
            });

            // 2. Панель статистики
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;
            statsPanel.BackColor = System.Drawing.Color.AliceBlue;
            statsPanel.Padding = new Padding(5);

            var lblStats = new Label() { Text = "Статистика:", Left = 5, Top = 5, Width = 60, Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold) };
            lblTotalTickets = new Label() { Text = "Билетов: 0", Left = 70, Top = 5, Width = 60 };
            lblTotalRevenue = new Label() { Text = "Выручка: 0 ₽", Left = 140, Top = 5, Width = 100 };

            statsPanel.Controls.AddRange(new Control[] {
                lblStats, lblTotalTickets, lblTotalRevenue
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

        private void LoadTickets()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var tickets = _ticketService.GetAllTickets();
                ApplyFilters(tickets);
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки билетов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters(List<Ticket> tickets)
        {
            try
            {
                var filteredTickets = tickets.AsEnumerable();

                // Фильтр по статусу
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString();
                if (selectedStatus != "Все" && !string.IsNullOrEmpty(selectedStatus))
                {
                    filteredTickets = filteredTickets.Where(t => t.Status == selectedStatus);
                }

                // Фильтр по классу
                string selectedClass = cmbClassFilter.SelectedItem?.ToString();
                if (selectedClass != "Все" && !string.IsNullOrEmpty(selectedClass))
                {
                    filteredTickets = filteredTickets.Where(t => t.Class == selectedClass);
                }

                // Поиск
                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredTickets = filteredTickets.Where(t =>
                        (t.TicketNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (t.FlightNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    );
                }

                var result = filteredTickets.ToList();
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

                SetColumnPropertyIfExists("RecordNumber", "№", 50);
                SetColumnPropertyIfExists("TicketNumber", "№ билета", 100);
                SetColumnPropertyIfExists("FlightNumber", "Рейс", 80);
                SetColumnPropertyIfExists("PassengerId", "Пассажир ID", 80);
                SetColumnPropertyIfExists("Class", "Класс", 70);
                SetColumnPropertyIfExists("Status", "Статус", 80);
                SetColumnPropertyIfExists("Luggage", "Багаж", 60);
                SetColumnPropertyIfExists("Price", "Цена", 80);

                // Форматирование цены
                if (dataGridView.Columns.Contains("Price"))
                {
                    dataGridView.Columns["Price"].DefaultCellStyle.Format = "N2";
                    dataGridView.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Подписываемся на события форматирования
                dataGridView.CellFormatting -= DataGridView_CellFormatting;
                dataGridView.CellFormatting += DataGridView_CellFormatting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования: {ex.Message}");
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                if (dataGridView.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString();
                    switch (status)
                    {
                        case "Активен":
                            e.CellStyle.BackColor = System.Drawing.Color.PaleGreen;
                            e.CellStyle.ForeColor = System.Drawing.Color.DarkGreen;
                            break;
                        case "Возвращен":
                            e.CellStyle.BackColor = System.Drawing.Color.LightCoral;
                            e.CellStyle.ForeColor = System.Drawing.Color.DarkRed;
                            break;
                        case "Отменен":
                            e.CellStyle.BackColor = System.Drawing.Color.LightGray;
                            e.CellStyle.ForeColor = System.Drawing.Color.Black;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования: {ex.Message}");
            }
        }

        private void UpdateStatistics(List<Ticket> tickets)
        {
            try
            {
                int total = tickets.Count;
                decimal revenue = tickets.Sum(t => t.Price);

                lblTotalTickets.Text = $"Билетов: {total}";
                lblTotalRevenue.Text = $"Выручка: {revenue:N2} ₽";

                lblTotalTickets.ForeColor = total > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
                lblTotalRevenue.ForeColor = revenue > 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Gray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilters(_ticketService.GetAllTickets());
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters(_ticketService.GetAllTickets());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowSellTicketForm();
        }
        private void ShowSellTicketForm()
        {
            try
            {
                using (var form = new SellTicketForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadTickets(); // Обновляем список билетов после продажи
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы продажи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                if (!dataGridView.Columns.Contains("RecordNumber")) return;

                int recordNumber = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells["RecordNumber"].Value);

                if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
                {
                    ShowEditTicketForm(recordNumber);
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
                {
                    DeleteTicket(recordNumber);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки клика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowEditTicketForm(int recordNumber)
        {
            try
            {
                var ticketService = ServiceLocator.GetService<ITicketService>();
                var passengerService = ServiceLocator.GetService<IPassengerService>();
                var flightService = ServiceLocator.GetService<IFlightService>();

                using (var form = new TicketEditForm(recordNumber, ticketService, passengerService, flightService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadTickets();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTicket(int recordNumber)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить билет №{recordNumber}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_ticketService.DeleteTicket(recordNumber))
                    {
                        MessageBox.Show("Билет успешно удален", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTickets();
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
                saveDialog.Title = "Экспорт билетов";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine("№;Номер билета;Рейс;ID пассажира;Класс;Статус;Багаж;Цена");

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
                                $"{GetSafeValue("RecordNumber")};" +
                                $"{GetSafeValue("TicketNumber")};" +
                                $"{GetSafeValue("FlightNumber")};" +
                                $"{GetSafeValue("PassengerId")};" +
                                $"{GetSafeValue("Class")};" +
                                $"{GetSafeValue("Status")};" +
                                $"{GetSafeValue("Luggage")};" +
                                $"{GetSafeValue("Price")}"
                            );
                        }
                    }

                    MessageBox.Show("Данные успешно экспортированы", "Экспорт",
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
            LoadTickets();
        }

        // Вспомогательные методы
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