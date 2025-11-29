using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;
using UD_WForms;

namespace UD_WForms.Forms
{
    public partial class AirportsForm : Form
    {
        public void RefreshData()
        {
            LoadAirports();
        }

        private IAirportService _airportService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;

        public AirportsForm()
        {
            InitializeComponent();
            _airportService = new AirportService();
            LoadAirports();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление аэропортами";
            this.Size = new System.Drawing.Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(700, 400);

            // Панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.Padding = new Padding(10);

            txtSearch = new TextBox();
            txtSearch.Location = new System.Drawing.Point(10, 10);
            txtSearch.Size = new System.Drawing.Size(200, 20);
            txtSearch.PlaceholderText = "Поиск по названию, коду IATA, городу...";

            btnSearch = new Button();
            btnSearch.Text = "Найти";
            btnSearch.Location = new System.Drawing.Point(220, 10);
            btnSearch.Size = new System.Drawing.Size(75, 23);
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button();
            btnAdd.Text = "Добавить аэропорт";
            btnAdd.Location = new System.Drawing.Point(310, 10);
            btnAdd.Size = new System.Drawing.Size(140, 23);
            btnAdd.Click += BtnAdd_Click;

            searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnAdd });
            this.Controls.Add(searchPanel);

            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            DataGridViewHelper.ConfigureDataGridView(dataGridView);
            DataGridViewHelper.AddActionButtons(dataGridView);
            dataGridView.CellClick += DataGridView_CellClick;
            this.Controls.Add(dataGridView);

            // Панель кнопок
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10);

            btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(10, 10);
            btnRefresh.Size = new System.Drawing.Size(80, 30);
            btnRefresh.Click += (s, e) => LoadAirports();

            btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new System.Drawing.Point(100, 10);
            btnClose.Size = new System.Drawing.Size(80, 30);
            btnClose.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { btnRefresh, btnClose });
            this.Controls.Add(buttonPanel);

            this.ResumeLayout(false);
        }

        private void LoadAirports()
        {
            try
            {
                var airports = _airportService.GetAllAirports();
                dataGridView.DataSource = airports;
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аэропортов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridView()
        {
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["AirportId"].HeaderText = "ID";
                dataGridView.Columns["Name"].HeaderText = "Название аэропорта";
                dataGridView.Columns["IATACode"].HeaderText = "Код IATA";
                dataGridView.Columns["Country"].HeaderText = "Страна";
                dataGridView.Columns["City"].HeaderText = "Город";
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var airports = _airportService.SearchAirports(txtSearch.Text);
                dataGridView.DataSource = airports;
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowAirportForm();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
            {
                int airportId = (int)dataGridView.Rows[e.RowIndex].Cells["AirportId"].Value;
                ShowAirportForm(airportId);
            }
            else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
            {
                int airportId = (int)dataGridView.Rows[e.RowIndex].Cells["AirportId"].Value;
                DeleteAirport(airportId);
            }
        }

        private void ShowAirportForm(int airportId = 0)
        {
            using (var form = new AirportForm(airportId, _airportService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadAirports();
                }
            }
        }

        private void DeleteAirport(int airportId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот аэропорт?",
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
    }
}