using System;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace Vulpes0
{
    public partial class AddForm : Form
    {
        private string activeTable;
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Маркетплейс.accdb;";

        // Объявляем с запасом 5 полей ввода для любых таблиц
        private TextBox txtField1 = new TextBox();
        private TextBox txtField2 = new TextBox();
        private TextBox txtField3 = new TextBox();
        private TextBox txtField4 = new TextBox();
        private TextBox txtField5 = new TextBox();

        private Label lblField1 = new Label();
        private Label lblField2 = new Label();
        private Label lblField3 = new Label();
        private Label lblField4 = new Label();
        private Label lblField5 = new Label();

        public AddForm(string tableName)
        {
            InitializeComponent();
            activeTable = tableName;
            BuildCustomInterface();
        }

        private void BuildCustomInterface()
        {
            this.Text = $"Добавить запись: {activeTable}";
            this.Size = new Size(550, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lblHeader = new Label();
            lblHeader.Text = "Заполните поля для новой записи";
            lblHeader.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblHeader.ForeColor = Color.FromArgb(40, 44, 80);
            lblHeader.Location = new Point(30, 20);
            lblHeader.Size = new Size(400, 30);
            this.Controls.Add(lblHeader);

            Button btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAdd.BackColor = Color.FromArgb(66, 133, 244);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Size = new Size(120, 40);
            btnAdd.Location = new Point(140, 310);
            btnAdd.Click += BtnAdd_Click;
            this.Controls.Add(btnAdd);

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCancel.BackColor = Color.FromArgb(219, 68, 85);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Size = new Size(120, 40);
            btnCancel.Location = new Point(280, 310);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            // Защита от пробелов и регистра букв в именах вкладок
            string tName = activeTable.Trim().ToLower();

            if (tName == "товары")
            {
                CreateInputRow(lblField1, txtField1, "Артикул:", 80);
                CreateInputRow(lblField2, txtField2, "Название:", 120);
                CreateInputRow(lblField3, txtField3, "Размер:", 160);
                CreateInputRow(lblField4, txtField4, "Цвет:", 200);
                CreateInputRow(lblField5, txtField5, "Цена:", 240);
            }
            else if (tName == "корзина")
            {
                CreateInputRow(lblField1, txtField1, "ID Пользователя:", 80);
                CreateInputRow(lblField2, txtField2, "ID Товара:", 120);
                CreateInputRow(lblField3, txtField3, "Количество (kolvo):", 160);
            }
            else if (tName == "заказы")
            {
                CreateInputRow(lblField1, txtField1, "Номер заказа:", 80);
                CreateInputRow(lblField2, txtField2, "Статус:", 120);
                CreateInputRow(lblField3, txtField3, "Сумма:", 160);
                CreateInputRow(lblField4, txtField4, "ID Пользователя:", 200);
                CreateInputRow(lblField5, txtField5, "ID Курьера:", 240);
            }
            else if (tName == "состав заказа")
            {
                CreateInputRow(lblField1, txtField1, "ID Заказа:", 80);
                CreateInputRow(lblField2, txtField2, "ID Товара:", 120);
                CreateInputRow(lblField3, txtField3, "Количество (kolvo):", 160);
                CreateInputRow(lblField4, txtField4, "Цена (priceIN):", 200);
            }
            else if (tName == "доставка")
            {
                CreateInputRow(lblField1, txtField1, "ID Заказа:", 80);
                CreateInputRow(lblField2, txtField2, "Адрес:", 120);
                CreateInputRow(lblField3, txtField3, "Стоимость (TovMoney):", 160);
                CreateInputRow(lblField4, txtField4, "Дата (ГГГГ-ММ-ДД):", 200);
            }
            else // Страховка для пользователей или других вкладок
            {
                CreateInputRow(lblField1, txtField1, "ФИО (fio):", 80);
                CreateInputRow(lblField2, txtField2, "Телефон:", 120);
                CreateInputRow(lblField3, txtField3, "Адрес:", 160);
                CreateInputRow(lblField4, txtField4, "Роль:", 200);
            }
        }

        private void CreateInputRow(Label lbl, TextBox txt, string labelText, int y)
        {
            lbl.Text = labelText;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.TextAlign = ContentAlignment.MiddleRight;
            lbl.Location = new Point(20, y);
            lbl.Size = new Size(180, 25);
            lbl.Visible = true;

            txt.Font = new Font("Segoe UI", 10);
            txt.Location = new Point(210, y);
            txt.Size = new Size(260, 25);
            txt.Visible = true;

            this.Controls.Add(lbl);
            this.Controls.Add(txt);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string query = "";
            string tName = activeTable.Trim().ToLower();

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    if (tName == "товары")
                    {
                        query = "INSERT INTO Товары (artikul, [name], [size], color, price) VALUES (?, ?, ?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", txtField1.Text);
                            cmd.Parameters.AddWithValue("?", txtField2.Text);
                            cmd.Parameters.AddWithValue("?", txtField3.Text);
                            cmd.Parameters.AddWithValue("?", txtField4.Text);
                            cmd.Parameters.AddWithValue("?", Convert.ToDecimal(txtField5.Text));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (tName == "корзина")
                    {
                        query = "INSERT INTO Корзина (id_user, id_tov, kolvo) VALUES (?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField1.Text));
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField2.Text));
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField3.Text));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (tName == "заказы")
                    {
                        decimal autoSum = 0;
                        query = "INSERT INTO Заказы (nomer, status, [sum], id_user, id_courier) VALUES (?, ?, ?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", txtField1.Text);
                            cmd.Parameters.AddWithValue("?", txtField2.Text);
                            cmd.Parameters.AddWithValue("?", autoSum);
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField4.Text));
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField5.Text));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (tName == "состав заказа")
                    {
                        int idTov = Convert.ToInt32(txtField2.Text);
                        decimal priceFromCatalog = 0;

                        string getPriceQuery = "SELECT price FROM Товары WHERE id_tov = ?";
                        using (OleDbCommand checkCmd = new OleDbCommand(getPriceQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("?", idTov);
                            object result = checkCmd.ExecuteScalar();
                            if (result != null) priceFromCatalog = Convert.ToDecimal(result);
                        }

                        query = "INSERT INTO СоставЗаказа (id_order, id_tov, kolvo, priceIN) VALUES (?, ?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField1.Text));
                            cmd.Parameters.AddWithValue("?", idTov);
                            cmd.Parameters.AddWithValue("?", Convert.ToInt32(txtField3.Text));
                            cmd.Parameters.AddWithValue("?", priceFromCatalog);
                            cmd.ExecuteNonQuery();
                        }

                        int idOrder = Convert.ToInt32(txtField1.Text);
                        string updateOrderSumQuery = "UPDATE Заказы SET [sum] = (SELECT SUM(kolvo * priceIN) FROM СоставЗаказа WHERE id_order = ?) WHERE id_order = ?";
                        using (OleDbCommand updateCmd = new OleDbCommand(updateOrderSumQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("?", idOrder);
                            updateCmd.Parameters.AddWithValue("?", idOrder);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else if (tName == "доставка")
                    {
                        int idOrder = Convert.ToInt32(txtField1.Text);
                        decimal autoDeliverySum = 300;
                        query = "INSERT INTO Доставка (id_order, address, TovMoney, DelyveryDate) VALUES (?, ?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", idOrder);
                            cmd.Parameters.AddWithValue("?", txtField2.Text);
                            cmd.Parameters.AddWithValue("?", autoDeliverySum);
                            cmd.Parameters.AddWithValue("?", Convert.ToDateTime(txtField4.Text));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else // Пользователи (ИСПРАВЛЕНО: io заменено на fio)
                    {
                        query = "INSERT INTO Пользователи (fio, phone, address, [role]) VALUES (?, ?, ?, ?)";
                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("?", txtField1.Text);
                            cmd.Parameters.AddWithValue("?", txtField2.Text);
                            cmd.Parameters.AddWithValue("?", txtField3.Text);
                            cmd.Parameters.AddWithValue("?", txtField4.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Запись успешно сохранена и все автополя пересчитаны!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения в базу Access:\n" + ex.Message,
                "Ошибка SQL / Базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}