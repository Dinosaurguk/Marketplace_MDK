using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using static Vulpes0.Program;

namespace Vulpes0
{
    public partial class CheckoutForm : Form
    {
        private string connectionString;

        private DataGridView gridCart = new DataGridView();
        private TextBox txtAddress = new TextBox();
        private TextBox txtPhone = new TextBox();
        private ComboBox cmbPayment = new ComboBox();
        private Label lblSum = new Label();
        private Label lblDelivery = new Label();
        private Label lblTotal = new Label();
        private Button btnPay = new Button();

        private decimal cartSum = 0;
        private const decimal DELIVERY_COST = 300m;
        public CheckoutForm()
        {
            InitializeComponent();
            connectionString = Properties.Settings.Default.МаркетплейсConnectionString;
            BuildInterface();
            LoadCart();
        }

        private void BuildInterface()
        {
            this.Text = "Оформление заказа";
            this.Size = new Size(650, 620);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lblHead = new Label();
            lblHead.Text = "Ваш заказ";
            lblHead.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblHead.Location = new Point(20, 15);
            lblHead.Size = new Size(400, 25);
            this.Controls.Add(lblHead);

            gridCart.Location = new Point(20, 45);
            gridCart.Size = new Size(600, 200);
            gridCart.ReadOnly = true;
            gridCart.AllowUserToAddRows = false;
            gridCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.Controls.Add(gridCart);

            // Адрес
            Label l1 = new Label();
            l1.Text = "Адрес доставки:";
            l1.Font = new Font("Segoe UI", 10);
            l1.Location = new Point(20, 265);
            l1.Size = new Size(160, 25);
            this.Controls.Add(l1);

            txtAddress.Location = new Point(190, 265);
            txtAddress.Size = new Size(430, 25);
            txtAddress.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtAddress);

            // Телефон
            Label l2 = new Label();
            l2.Text = "Телефон:";
            l2.Font = new Font("Segoe UI", 10);
            l2.Location = new Point(20, 300);
            l2.Size = new Size(160, 25);
            this.Controls.Add(l2);

            txtPhone.Location = new Point(190, 300);
            txtPhone.Size = new Size(430, 25);
            txtPhone.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtPhone);

            // Способ оплаты
            Label l3 = new Label();
            l3.Text = "Способ оплаты:";
            l3.Font = new Font("Segoe UI", 10);
            l3.Location = new Point(20, 335);
            l3.Size = new Size(160, 25);
            this.Controls.Add(l3);

            cmbPayment.Location = new Point(190, 335);
            cmbPayment.Size = new Size(430, 25);
            cmbPayment.Font = new Font("Segoe UI", 10);
            cmbPayment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayment.Items.Add("Карта");
            cmbPayment.Items.Add("Наличные при получении");
            cmbPayment.SelectedIndex = 0;
            this.Controls.Add(cmbPayment);

            // Итоги
            lblSum.Font = new Font("Segoe UI", 10);
            lblSum.Location = new Point(20, 385);
            lblSum.Size = new Size(600, 22);
            this.Controls.Add(lblSum);

            lblDelivery.Font = new Font("Segoe UI", 10);
            lblDelivery.Location = new Point(20, 410);
            lblDelivery.Size = new Size(600, 22);
            this.Controls.Add(lblDelivery);

            lblTotal.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(200, 30, 60);
            lblTotal.Location = new Point(20, 440);
            lblTotal.Size = new Size(600, 30);
            this.Controls.Add(lblTotal);

            // Кнопка "Оплатить"
            btnPay.Text = "Оплатить";
            btnPay.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnPay.BackColor = Color.FromArgb(46, 125, 50);
            btnPay.ForeColor = Color.White;
            btnPay.FlatStyle = FlatStyle.Flat;
            btnPay.Size = new Size(280, 50);
            btnPay.Location = new Point(20, 490);
            btnPay.Click += BtnPay_Click;
            this.Controls.Add(btnPay);

            // Кнопка "Отмена"
            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Font = new Font("Segoe UI", 11);
            btnCancel.BackColor = Color.FromArgb(219, 68, 85);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Size = new Size(280, 50);
            btnCancel.Location = new Point(340, 490);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            // Автозаполнение адреса и телефона из профиля
            PrefillUserData();
        }

        private void PrefillUserData()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string q = "SELECT phone, address FROM Пользователи WHERE id_user = ?";
                    using (OleDbCommand cmd = new OleDbCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("?", UserSession.IdUser);
                        using (OleDbDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                txtPhone.Text = r["phone"]?.ToString() ?? "";
                                txtAddress.Text = r["address"]?.ToString() ?? "";
                            }
                        }
                    }
                }
            }
            catch { /* если не получилось — не страшно */ }
        }

        private void LoadCart()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    string q = @"SELECT Товары.name AS Название, Товары.[size] AS Размер, 
                                        Товары.color AS Цвет, Товары.price AS Цена, 
                                        Корзина.kolvo AS Колво,
                                        (Товары.price * Корзина.kolvo) AS Сумма
                                 FROM Корзина INNER JOIN Товары ON Корзина.id_tov = Товары.id_tov
                                 WHERE Корзина.id_user = ?";

                    using (OleDbDataAdapter da = new OleDbDataAdapter(q, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("?", UserSession.IdUser);
                        System.Data.DataTable dt = new System.Data.DataTable();
                        da.Fill(dt);
                        gridCart.DataSource = dt;

                        cartSum = 0;
                        foreach (System.Data.DataRow row in dt.Rows)
                            cartSum += Convert.ToDecimal(row["Сумма"]);
                    }
                }

                if (cartSum == 0)
                {
                    MessageBox.Show("Корзина пуста! Добавьте товары перед оформлением.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                lblSum.Text = $"Сумма товаров: {cartSum:N0} ₽";
                lblDelivery.Text = $"Стоимость доставки: {DELIVERY_COST:N0} ₽";
                lblTotal.Text = $"Итого к оплате: {(cartSum + DELIVERY_COST):N0} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Введите адрес доставки!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Введите телефон!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult r = MessageBox.Show(
                $"Оплатить {(cartSum + DELIVERY_COST):N0} ₽ способом \"{cmbPayment.SelectedItem}\"?",
                "Платёж", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r != DialogResult.Yes) return;

            string nomer = "";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    // 1. ПРОВЕРКА НАЛИЧИЯ
                    string checkStock = @"SELECT Товары.[name], Товары.stock, Корзина.kolvo
                                          FROM Корзина INNER JOIN Товары ON Корзина.id_tov = Товары.id_tov
                                          WHERE Корзина.id_user = ?";
                    using (OleDbCommand cmd = new OleDbCommand(checkStock, conn))
                    {
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = UserSession.IdUser;
                        using (OleDbDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                string name = rd["name"].ToString();
                                int stock = Convert.ToInt32(rd["stock"]);
                                int kolvo = Convert.ToInt32(rd["kolvo"]);
                                if (stock < kolvo)
                                {
                                    MessageBox.Show($"Товара \"{name}\" нет в наличии в нужном количестве!",
                                                    "Товара нет в наличии", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }
                    }

                    // 2. НОМЕР ЗАКАЗА
                    int newId = 1;
                    using (OleDbCommand cmd = new OleDbCommand("SELECT MAX(id_order) FROM Заказы", conn))
                    {
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                            newId = Convert.ToInt32(res) + 1;
                    }
                    nomer = $"MP-{DateTime.Now.Year}-{newId:D3}";
                    decimal totalSum = cartSum + DELIVERY_COST;

                    // 3. ВСТАВКА ЗАКАЗА — id_courier = 0 вместо DBNull
                    string insOrder = "INSERT INTO Заказы (nomer, status, [sum], id_user, id_courier) VALUES (?, ?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(insOrder, conn))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = nomer;
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = "Новый";
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = totalSum;
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = UserSession.IdUser;
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = 0;
                        cmd.ExecuteNonQuery();
                    }

                    // 4. ПЕРЕНОСИМ ТОВАРЫ И СПИСЫВАЕМ СО СКЛАДА
                    var items = new System.Collections.Generic.List<(int idTov, int kolvo, decimal price)>();
                    string getItems = @"SELECT Корзина.id_tov, Корзина.kolvo, Товары.price
                                        FROM Корзина INNER JOIN Товары ON Корзина.id_tov = Товары.id_tov
                                        WHERE Корзина.id_user = ?";
                    using (OleDbCommand getCmd = new OleDbCommand(getItems, conn))
                    {
                        getCmd.Parameters.Add("?", OleDbType.Integer).Value = UserSession.IdUser;
                        using (OleDbDataReader rd = getCmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                items.Add((
                                    Convert.ToInt32(rd["id_tov"]),
                                    Convert.ToInt32(rd["kolvo"]),
                                    Convert.ToDecimal(rd["price"])
                                ));
                            }
                        }
                    }

                    foreach (var it in items)
                    {
                        using (OleDbCommand ins = new OleDbCommand(
                            "INSERT INTO СоставЗаказа (id_order, id_tov, kolvo, priceIN) VALUES (?, ?, ?, ?)", conn))
                        {
                            ins.Parameters.Add("?", OleDbType.Integer).Value = newId;
                            ins.Parameters.Add("?", OleDbType.Integer).Value = it.idTov;
                            ins.Parameters.Add("?", OleDbType.Integer).Value = it.kolvo;
                            ins.Parameters.Add("?", OleDbType.Currency).Value = it.price;
                            ins.ExecuteNonQuery();
                        }

                        using (OleDbCommand upd = new OleDbCommand(
                            "UPDATE Товары SET stock = stock - ? WHERE id_tov = ?", conn))
                        {
                            upd.Parameters.Add("?", OleDbType.Integer).Value = it.kolvo;
                            upd.Parameters.Add("?", OleDbType.Integer).Value = it.idTov;
                            upd.ExecuteNonQuery();
                        }
                    }

                    // 5. ДОСТАВКА
                    string insDeliv = "INSERT INTO Доставка (id_order, address, TovMoney, DelyveryDate) VALUES (?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(insDeliv, conn))
                    {
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = newId;
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtAddress.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = DELIVERY_COST;
                        cmd.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now.AddDays(7);
                        cmd.ExecuteNonQuery();
                    }

                    // 6. ЧИСТИМ КОРЗИНУ
                    using (OleDbCommand cmd = new OleDbCommand("DELETE FROM Корзина WHERE id_user = ?", conn))
                    {
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = UserSession.IdUser;
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    $"Оплата прошла успешно!\n\nНомер вашего заказа: {nomer}\n\nСтатус: Новый\nОжидайте сборки и назначения курьера.",
                    "Заказ оформлен", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}