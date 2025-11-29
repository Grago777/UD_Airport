using System;
using System.Windows.Forms;
using UD_WForms.Services;
using UD_WForms.Models;
using UD_WForms.Controls;

namespace UD_WForms.Forms
{
    public partial class PassengersForm : Form
    {
        public void RefreshData()
        {
            LoadPassengers();
        }

        private IPassengerService _passengerService;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnRefresh;
        private Button btnClose;

        public PassengersForm()
        {
            _passengerService = ServiceLocator.GetService<IPassengerService>();
            InitializeComponent();
            LoadPassengers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Управление пассажирами";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 400);

            // Панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.Padding = new Padding(10);

            txtSearch = new TextBox();
            txtSearch.Location = new System.Drawing.Point(10, 10);
            txtSearch.Size = new System.Drawing.Size(200, 20);
            txtSearch.PlaceholderText = "Поиск по ФИО, телефону, email...";

            btnSearch = new Button();
            btnSearch.Text = "Найти";
            btnSearch.Location = new System.Drawing.Point(220, 10);
            btnSearch.Size = new System.Drawing.Size(75, 23);
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button();
            btnAdd.Text = "Добавить пассажира";
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
            btnRefresh.Click += (s, e) => LoadPassengers();

            btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new System.Drawing.Point(100, 10);
            btnClose.Size = new System.Drawing.Size(80, 30);
            btnClose.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { btnRefresh, btnClose });
            this.Controls.Add(buttonPanel);

            this.ResumeLayout(false);
        }

        private void LoadPassengers()
        {
            try
            {
                var passengers = _passengerService.GetAllPassengers();
                dataGridView.DataSource = passengers;
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пассажиров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridView()
        {
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["PassengerId"].HeaderText = "ID";
                dataGridView.Columns["FullName"].HeaderText = "ФИО";
                dataGridView.Columns["PhoneNumber"].HeaderText = "Телефон";
                dataGridView.Columns["Email"].HeaderText = "Email";
                dataGridView.Columns["PassportData"].HeaderText = "Паспортные данные";
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var passengers = _passengerService.SearchPassengers(txtSearch.Text);
                dataGridView.DataSource = passengers;
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
            ShowPassengerForm();
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridView.Columns[e.ColumnIndex].Name == "Edit")
            {
                int passengerId = (int)dataGridView.Rows[e.RowIndex].Cells["PassengerId"].Value;
                ShowPassengerForm(passengerId);
            }
            else if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
            {
                int passengerId = (int)dataGridView.Rows[e.RowIndex].Cells["PassengerId"].Value;
                DeletePassenger(passengerId);
            }
        }

        private void ShowPassengerForm(int passengerId = 0)
        {
            var form = new PassengerForm(passengerId, _passengerService);
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadPassengers();
                }
            }
        }

        private void DeletePassenger(int passengerId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этого пассажира?",
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
    }
}