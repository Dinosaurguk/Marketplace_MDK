using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using static Vulpes0.Program;

namespace Vulpes0
{
    public partial class CardForm : Form
    {
        // ПОЛЯ КЛАССА — объявляются БЕЗ значений
        private string connectionString;
        private int idTov;

        // ВСЕ ОСТАЛЬНЫЕ КОНТРОЛЫ — как было
        private Label lblName = new Label();
        private Label lblArtikul = new Label();
        private Label lblSize = new Label();
        private Label lblColor = new Label();
        private Label lblStock = new Label();
        private Label lblPrice = new Label();

        // КОНСТРУКТОР
        public CardForm(int idTov)
        {
            InitializeComponent();
            connectionString = Properties.Settings.Default.МаркетплейсConnectionString;
            this.idTov = idTov;              // <-- ЭТА СТРОКА ОБЯЗАТЕЛЬНА
            BuildInterface();
            LoadProduct();
        }

        private void BuildInterface()
        {
            this.Text = "Карточка товара";
            this.Size = new Size(500, 660);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // заглушка
            Panel imgPlaceholder = new Panel();
            imgPlaceholder.BackColor = Color.FromArgb(240, 240, 240);
            imgPlaceholder.Location = new Point(20, 20);
            imgPlaceholder.Size = new Size(440, 240);

            Label lblPlaceholder = new Label();
            lblPlaceholder.Text = "Здесь могла быть ваша реклама";
            lblPlaceholder.Font = new Font("Segoe UI", 14, FontStyle.Italic);
            lblPlaceholder.ForeColor = Color.Gray;
            lblPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
            lblPlaceholder.Dock = DockStyle.Fill;
            imgPlaceholder.Controls.Add(lblPlaceholder);
            this.Controls.Add(imgPlaceholder);

            // Название
            lblName.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(40, 44, 80);
            lblName.Location = new Point(20, 275);
            lblName.Size = new Size(440, 55);
            this.Controls.Add(lblName);

            // Артикул
            lblArtikul.Font = new Font("Segoe UI", 10);
            lblArtikul.ForeColor = Color.Gray;
            lblArtikul.Location = new Point(20, 335);
            lblArtikul.Size = new Size(440, 22);
            this.Controls.Add(lblArtikul);

            // Размер и цвет (в две колонки)
            lblSize.Font = new Font("Segoe UI", 11);
            lblSize.Location = new Point(20, 365);
            lblSize.Size = new Size(220, 25);
            this.Controls.Add(lblSize);

            lblColor.Font = new Font("Segoe UI", 11);
            lblColor.Location = new Point(240, 365);
            lblColor.Size = new Size(220, 25);
            this.Controls.Add(lblColor);

            // Наличие
            lblStock.Font = new Font("Segoe UI", 11);
            lblStock.Location = new Point(20, 400);
            lblStock.Size = new Size(440, 25);
            this.Controls.Add(lblStock);

            // Цена
            lblPrice.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblPrice.ForeColor = Color.FromArgb(200, 30, 60);
            lblPrice.Location = new Point(20, 440);
            lblPrice.Size = new Size(440, 50);
            this.Controls.Add(lblPrice);

            // Добавить в корзину
            if (UserSession.Role == "Покупатель")
            {
                Button btnAddToCart = new Button();
                btnAddToCart.Text = "Добавить в корзину";
                btnAddToCart.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                btnAddToCart.BackColor = Color.FromArgb(128, 40, 200);
                btnAddToCart.ForeColor = Color.White;
                btnAddToCart.FlatStyle = FlatStyle.Flat;
                btnAddToCart.Size = new Size(440, 50);
                btnAddToCart.Location = new Point(20, 505);
                btnAddToCart.Click += BtnAddToCart_Click;
                this.Controls.Add(btnAddToCart);
            }

            // Кнопка Закрыть
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Font = new Font("Segoe UI", 10);
            btnClose.BackColor = Color.FromArgb(220, 220, 220);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Size = new Size(440, 35);
            btnClose.Location = new Point(20, 570);
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadProduct()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT artikul, [name], [size], color, price, stock FROM Товары WHERE id_tov = ?";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("?", idTov);
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblName.Text = reader["name"].ToString();
                                lblArtikul.Text = "Артикул: " + reader["artikul"].ToString();
                                lblSize.Text = "Размер: " + reader["size"].ToString();
                                lblColor.Text = "Цвет: " + reader["color"].ToString();
                                lblStock.Text = "В наличии: " + reader["stock"].ToString() + " шт.";

                                decimal price = Convert.ToDecimal(reader["price"]);
                                lblPrice.Text = price.ToString("N0") + " ₽";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товара:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    // Проверяем наличие на складе
                    int stock = 0;
                    using (OleDbCommand cmd = new OleDbCommand("SELECT stock FROM Товары WHERE id_tov = ?", conn))
                    {
                        cmd.Parameters.AddWithValue("?", idTov);
                        object r = cmd.ExecuteScalar();
                        if (r != null) stock = Convert.ToInt32(r);
                    }

                    if (stock <= 0)
                    {
                        MessageBox.Show("Товара нет в наличии!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Проверяем, есть ли уже этот товар в корзине
                    string checkQuery = "SELECT id_korz, kolvo FROM Корзина WHERE id_user = ? AND id_tov = ?";
                    using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("?", UserSession.IdUser);
                        checkCmd.Parameters.AddWithValue("?", idTov);

                        using (OleDbDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int idKorz = Convert.ToInt32(reader["id_korz"]);
                                int currentKolvo = Convert.ToInt32(reader["kolvo"]);
                                reader.Close();

                                using (OleDbCommand upd = new OleDbCommand("UPDATE Корзина SET kolvo = ? WHERE id_korz = ?", conn))
                                {
                                    upd.Parameters.AddWithValue("?", currentKolvo + 1);
                                    upd.Parameters.AddWithValue("?", idKorz);
                                    upd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                reader.Close();
                                using (OleDbCommand ins = new OleDbCommand("INSERT INTO Корзина (id_user, id_tov, kolvo) VALUES (?, ?, ?)", conn))
                                {
                                    ins.Parameters.AddWithValue("?", UserSession.IdUser);
                                    ins.Parameters.AddWithValue("?", idTov);
                                    ins.Parameters.AddWithValue("?", 1);
                                    ins.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении в корзину:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}