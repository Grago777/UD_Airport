using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using UD_WForms.Services;
using UD_WForms.Models;

namespace UD_WForms.Forms
{
    public partial class SelectFlightForm : Form
    {
        private IFlightService _flightService;
        private List<Flight> _flights;
        private Flight _selectedFlight;
        private DataGridView dataGridView;

        public Flight SelectedFlight => _selectedFlight;

        public SelectFlightForm()
        {
            InitializeComponent();
            _flightService = ServiceLocator.GetService<IFlightService>();
            LoadFlights();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Выбор рейса для продажи билета";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Панель с инструкцией
            var lblInfo = new Label()
            {
                Text = "Выберите рейс для продажи билета:\n(отображаются только рейсы 'По расписанию' с будущей датой вылета)",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.LightYellow
            };

            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.ReadOnly = true;
            dataGridView.MultiSelect = false;
            dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            // Кнопки
            var btnSelect = new Button() { Text = "Выбрать", Size = new System.Drawing.Size(100, 30) };
            btnSelect.Click += BtnSelect_Click;

            var btnCancel = new Button() { Text = "Отмена", Size = new System.Drawing.Size(100, 30) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            var buttonPanel = new Panel() { Dock = DockStyle.Bottom, Height = 50 };
            buttonPanel.Controls.Add(btnSelect);
            buttonPanel.Controls.Add(btnCancel);

            // Расположение кнопок
            btnSelect.Location = new System.Drawing.Point(250, 10);
            btnCancel.Location = new System.Drawing.Point(360, 10);

            this.Controls.AddRange(new Control[] { lblInfo, dataGridView, buttonPanel });
            this.ResumeLayout(false);
        }

        private void LoadFlights()
        {
            try
            {
                _flights = _flightService.GetAllFlights()
                    .Where(f => f.Status == "По расписанию" && f.DepartureDate > DateTime.Now)
                    .OrderBy(f => f.DepartureDate)
                    .ToList();

                dataGridView.DataSource = _flights;
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рейсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridView()
        {
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["FlightNumber"].HeaderText = "Рейс";
                dataGridView.Columns["FlightNumber"].Width = 80;

                dataGridView.Columns["Airline"].HeaderText = "Авиакомпания";
                dataGridView.Columns["Airline"].Width = 120;

                dataGridView.Columns["Aircraft"].HeaderText = "Самолет";
                dataGridView.Columns["Aircraft"].Width = 100;

                dataGridView.Columns["DepartureDate"].HeaderText = "Вылет";
                dataGridView.Columns["DepartureDate"].Width = 120;
                dataGridView.Columns["DepartureDate"].DefaultCellStyle.Format = "dd.MM.yy HH:mm";

                dataGridView.Columns["ArrivalDate"].HeaderText = "Прибытие";
                dataGridView.Columns["ArrivalDate"].Width = 120;
                dataGridView.Columns["ArrivalDate"].DefaultCellStyle.Format = "dd.MM.yy HH:mm";

                dataGridView.Columns["EconomySeats"].HeaderText = "Эконом";
                dataGridView.Columns["EconomySeats"].Width = 60;

                dataGridView.Columns["BusinessSeats"].HeaderText = "Бизнес";
                dataGridView.Columns["BusinessSeats"].Width = 60;
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SelectFlight(e.RowIndex);
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                SelectFlight(dataGridView.SelectedRows[0].Index);
            }
            else
            {
                MessageBox.Show("Выберите рейс", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SelectFlight(int rowIndex)
        {
            _selectedFlight = _flights[rowIndex];
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}