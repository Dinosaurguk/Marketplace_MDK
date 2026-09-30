using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Vulpes0.Program;

namespace Vulpes0
{
    public partial class AuthForm : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Маркетплейс.accdb;";
        public AuthForm()
        {
            InitializeComponent();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Получаем текущую вкладку и её размеры
            TabPage currentTab = tabControl1.TabPages[e.Index];
            Rectangle tabBounds = tabControl1.GetTabRect(e.Index);

            Brush backBrush;
            Brush textBrush = Brushes.White; // Белый цвет текста на вкладках

            // Красим верхние корешки (кнопки вкладок): 0 - Войти, 1 - Регистрация
            if (e.Index == 0)
            {
                backBrush = new SolidBrush(Color.FromArgb(25, 118, 210)); // Синий
                currentTab.BackColor = Color.FromArgb(240, 244, 248);     // Фон страницы Войти
            }
            else
            {
                backBrush = new SolidBrush(Color.FromArgb(46, 125, 50));  // Зеленый
                currentTab.BackColor = Color.FromArgb(242, 248, 242);     // Фон страницы Регистрация
            }

            // Рисуем цветной прямоугольник вкладки
            e.Graphics.FillRectangle(backBrush, tabBounds);

            // Центрируем и пишем текст
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(currentTab.Text, new Font("Segoe UI", 10, FontStyle.Bold),
                                  textBrush, tabBounds, sf);

            backBrush.Dispose();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLoginIn.Text) || string.IsNullOrWhiteSpace(txtPasswordIn.Text))
            {
                MessageBox.Show("Заполните логин и пароль!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    // Соединяем Учетки и Пользователи через INNER JOIN, чтобы сразу забрать Роль и ФИО
                    string authQuery = "SELECT Учетки.id_user, Пользователи.role, Пользователи.fio " +
                                       "FROM Учетки INNER JOIN Пользователи ON Учетки.id_user = Пользователи.id_user " +
                                       "WHERE Учетки.login = ? AND Учетки.[password] = ?";

                    using (OleDbCommand cmd = new OleDbCommand(authQuery, conn))
                    {
                        // В OleDb параметры подставляются строго по порядку знаков '?'
                        cmd.Parameters.AddWithValue("?", txtLoginIn.Text.Trim());
                        cmd.Parameters.AddWithValue("?", txtPasswordIn.Text.Trim());

                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Если нашли строку — данные верны
                            {
                                // Записываем данные в глобальную сессию
                                UserSession.IdUser = Convert.ToInt32(reader["id_user"]);
                                UserSession.Role = reader["role"].ToString();
                                UserSession.Fio = reader["fio"].ToString();

                                MessageBox.Show($"Успешный вход! Добро пожаловать, {UserSession.Fio} ({UserSession.Role})",
                                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Скрываем форму входа и запускаем главную форму
                                this.Hide();
                                Form1 mainForm = new Form1();
                                mainForm.ShowDialog();
                                this.Close(); // Закрываем форму авторизации после закрытия главной
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка авторизации:\n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLoginReg.Text) || string.IsNullOrWhiteSpace(txtPasswordReg.Text) ||
    string.IsNullOrWhiteSpace(txtFioReg.Text) || cmbRoleReg.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля: Логин, Пароль, ФИО и Роль!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    // Проверяем, нет ли уже такого логина в базе, чтобы избежать дубликатов
                    string checkQuery = "SELECT COUNT(*) FROM Учетки WHERE login = ?";
                    using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("?", txtLoginReg.Text.Trim());
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("Этот логин уже занят! Выберите другой.", "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // ШАГ 1: Создаем запись в таблице Пользователи
                    string userInsert = "INSERT INTO Пользователи (fio, phone, address, [role]) VALUES (?, ?, ?, ?)";
                    int newUserId = 0;

                    using (OleDbCommand cmdUser = new OleDbCommand(userInsert, conn))
                    {
                        cmdUser.Parameters.AddWithValue("?", txtFioReg.Text.Trim());
                        cmdUser.Parameters.AddWithValue("?", txtPhoneReg.Text.Trim());
                        cmdUser.Parameters.AddWithValue("?", txtAddressReg.Text.Trim());
                        cmdUser.Parameters.AddWithValue("?", cmbRoleReg.SelectedItem.ToString());
                        cmdUser.ExecuteNonQuery();

                        // ХИТРОСТЬ: Сразу же забираем только что созданный Счетчик (id_user) в Access
                        using (OleDbCommand cmdId = new OleDbCommand("SELECT @@IDENTITY", conn))
                        {
                            newUserId = Convert.ToInt32(cmdId.ExecuteScalar());
                        }
                    }

                    // ШАГ 2: Создаем учетную запись, привязанную к этому id_user
                    string authInsert = "INSERT INTO Учетки (id_user, login, [password]) VALUES (?, ?, ?)";
                    using (OleDbCommand cmdAuth = new OleDbCommand(authInsert, conn))
                    {
                        cmdAuth.Parameters.AddWithValue("?", newUserId);
                        cmdAuth.Parameters.AddWithValue("?", txtLoginReg.Text.Trim());
                        cmdAuth.Parameters.AddWithValue("?", txtPasswordReg.Text.Trim());
                        cmdAuth.ExecuteNonQuery();
                    }

                    MessageBox.Show("Регистрация успешно завершена! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Переключаем преподский интерфейс на вкладку "Войти" для удобства
                    tabControl1.SelectedIndex = 0;
                    txtLoginIn.Text = txtLoginReg.Text;
                    txtPasswordIn.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации:\n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

    }
}
