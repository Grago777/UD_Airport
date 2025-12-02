using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class PassengersForm : Form
    {
        private IPassengerService _passengerService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblTotalPassengers;
        private Label lblWithEmail;
        private Label lblWithPhone;

        public PassengersForm()
        {
            InitializeComponent();
            _passengerService = ServiceLocator.GetService<IPassengerService>();

            dataGridView.DataBindingComplete += DataGridView_DataBindingComplete;
            LoadPassengers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление пассажирами";
            this.Size = new System.Drawing.Size(1000, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Padding = new Padding(10);

            // Основной контейнер
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // Панель поиска
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Статистика
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Таблица
            mainLayout.ColumnCount = 1;

            // 1. Панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.BackColor = System.Drawing.Color.LavenderBlush;
            searchPanel.Padding = new Padding(5);

            var lblSearch = new Label() { Text = "Поиск:", Left = 5, Top = 10, Width = 45 };
            txtSearch = new TextBox() { Left = 50, Top = 8, Width = 200, PlaceholderText = "ФИО, телефон, email, паспорт..." };

            btnSearch = new Button() { Text = "Найти", Left = 260, Top = 8, Size = new System.Drawing.Size(60, 23) };
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button() { Text = "➕ Добавить", Left = 330, Top = 8, Size = new System.Drawing.Size(90, 23) };
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button() { Text = "🔄 Обновить", Left = 430, Top = 8, Size = new System.Drawing.Size(80, 23) };
            btnRefresh.Click += (s, e) => LoadPassengers();

            btnClose = new Button() { Text = "Закрыть", Left = 520, Top = 8, Size = new System.Drawing.Size(80, 23) };
            btnClose.Click += (s, e) => this.Close();

            searchPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, btnSearch, btnAdd, btnRefresh, btnClose
            });

            // 2. Панель статистики
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;
            statsPanel.BackColor = System.Drawing.Color.AliceBlue;
            statsPanel.Padding = new Padding(5);

            var lblStats = new Label() { Text = "Статистика:", Left = 5, Top = 5, Width = 60, Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold) };
            lblTotalPassengers = new Label() { Text = "Пассажиров: 0", Left = 70, Top = 5, Width = 80 };
            lblWithEmail = new Label() { Text = "С email: 0", Left = 160, Top = 5, Width = 60 };
            lblWithPhone = new Label() { Text = "С телефоном: 0", Left = 230, Top = 5, Width = 90 };

            statsPanel.Controls.AddRange(new Control[] {
                lblStats, lblTotalPassengers, lblWithEmail, lblWithPhone
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

        private void LoadPassengers()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var passengers = _passengerService.GetAllPassengers();
                ApplyFilters(passengers);
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка загрузки пассажиров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters(List<Passenger> passengers)
        {
            try
            {
                var filteredPassengers = passengers.AsEnumerable();

                // Поиск
                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredPassengers = filteredPassengers.Where(p =>
                        (p.FullName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.PhoneNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.Email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.PassportData?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    );
                }

                var result = filteredPassengers.ToList();
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

                SetColumnPropertyIfExists("PassengerId", "ID", 50);
                SetColumnPropertyIfExists("FullName", "ФИО", 150);
                SetColumnPropertyIfExists("PhoneNumber", "Телефон", 100);
                SetColumnPropertyIfExists("Email", "Email", 120);
                SetColumnPropertyIfExists("PassportData", "Паспорт", 120);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка форматирования: {ex.Message}");
            }
        }

        private void UpdateStatistics(List<Passenger> passengers)
        {
            try
            {
                int total = passengers.Count;
                int withEmail = passengers.Count(p => !string.IsNullOrEmpty(p.Email));
                int withPhone = passengers.Count(p => !string.IsNullOrEmpty(p.PhoneNumber));

                lblTotalPassengers.Text = $"Пассажиров: {total}";
                lblWithEmail.Text = $"С email: {withEmail}";
                lblWithPhone.Text = $"С телефоном: {withPhone}";

                lblTotalPassengers.ForeColor = total > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
                lblWithEmail.ForeColor = withEmail > 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Gray;
                lblWithPhone.ForeColor = withPhone > 0 ? System.Drawing.Color.DarkBlue : System.Drawing.Color.Gray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilters(_passengerService.GetAllPassengers());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowPassengerForm();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                if (!dataGridView.Columns.Contains("PassengerId")) return;

                int passengerId = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells["PassengerId"].Value);

                if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
                {
                    ShowPassengerForm(passengerId);
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
                {
                    DeletePassenger(passengerId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки клика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPassengerForm(int passengerId = 0)
        {
            try
            {
                using (var form = new PassengerForm(passengerId, _passengerService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadPassengers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeletePassenger(int passengerId)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить пассажира ID {passengerId}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_passengerService.DeletePassenger(passengerId))
                    {
                        MessageBox.Show("Пассажир успешно удален", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPassengers();
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
            LoadPassengers();
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