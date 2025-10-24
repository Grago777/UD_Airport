namespace UD_WForms
{
    // MainForm.Designer.cs (частичный код)
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDepartureCity;
        private System.Windows.Forms.TextBox txtArrivalCity;
        private System.Windows.Forms.DateTimePicker dateTimePickerDeparture;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnBookTicket;
        private System.Windows.Forms.Button btnShowBookings;
        private System.Windows.Forms.DataGridView dataGridViewFlights;
        private System.Windows.Forms.Button btnRefresh;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Text = "Авиакасса - Поиск рейсов";

            // Создание и настройка элементов управления
            CreateControls();
        }

        private void CreateControls()
        {
            // Label для города вылета
            label1 = new Label();
            label1.Text = "Город вылета:";
            label1.Location = new System.Drawing.Point(20, 20);
            label1.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(label1);

            // TextBox для города вылета
            txtDepartureCity = new TextBox();
            txtDepartureCity.Location = new System.Drawing.Point(120, 20);
            txtDepartureCity.Size = new System.Drawing.Size(150, 20);
            this.Controls.Add(txtDepartureCity);

            // Label для города прилета
            label2 = new Label();
            label2.Text = "Город прилета:";
            label2.Location = new System.Drawing.Point(280, 20);
            label2.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(label2);

            // TextBox для города прилета
            txtArrivalCity = new TextBox();
            txtArrivalCity.Location = new System.Drawing.Point(380, 20);
            txtArrivalCity.Size = new System.Drawing.Size(150, 20);
            this.Controls.Add(txtArrivalCity);

            // Label для даты вылета
            label3 = new Label();
            label3.Text = "Дата вылета:";
            label3.Location = new System.Drawing.Point(540, 20);
            label3.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(label3);

            // DateTimePicker для даты вылета
            dateTimePickerDeparture = new DateTimePicker();
            dateTimePickerDeparture.Location = new System.Drawing.Point(640, 20);
            dateTimePickerDeparture.Size = new System.Drawing.Size(150, 20);
            dateTimePickerDeparture.Format = DateTimePickerFormat.Short;
            this.Controls.Add(dateTimePickerDeparture);

            // Кнопка поиска
            btnSearch = new Button();
            btnSearch.Text = "Поиск рейсов";
            btnSearch.Location = new System.Drawing.Point(20, 60);
            btnSearch.Size = new System.Drawing.Size(100, 30);
            btnSearch.Click += new EventHandler(btnSearch_Click);
            this.Controls.Add(btnSearch);

            // Кнопка обновления
            btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(130, 60);
            btnRefresh.Size = new System.Drawing.Size(80, 30);
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            this.Controls.Add(btnRefresh);

            // Кнопка бронирования
            btnBookTicket = new Button();
            btnBookTicket.Text = "Бронировать";
            btnBookTicket.Location = new System.Drawing.Point(220, 60);
            btnBookTicket.Size = new System.Drawing.Size(100, 30);
            btnBookTicket.Click += new EventHandler(btnBookTicket_Click);
            this.Controls.Add(btnBookTicket);

            // Кнопка показа бронирований
            btnShowBookings = new Button();
            btnShowBookings.Text = "Мои бронирования";
            btnShowBookings.Location = new System.Drawing.Point(330, 60);
            btnShowBookings.Size = new System.Drawing.Size(120, 30);
            //btnShowBookings.Click += new EventHandler(btnShowBookings_Click);
            this.Controls.Add(btnShowBookings);

            // DataGridView для отображения рейсов
            dataGridViewFlights = new DataGridView();
            dataGridViewFlights.Location = new System.Drawing.Point(20, 100);
            dataGridViewFlights.Size = new System.Drawing.Size(850, 450);
            dataGridViewFlights.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewFlights.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewFlights.ReadOnly = true;
            this.Controls.Add(dataGridViewFlights);
        }
    }
}
