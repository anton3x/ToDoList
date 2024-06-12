using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projeto.Views
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void BtnRegister_OnClick(object sender, RoutedEventArgs e)
        {
            string username = txtbUsername.Text;
            string email = txtbEmail.Text;
            string password = txtbPassword.Password;
            string confirmPassword = txtbConfirmPassword.Password;

            if (!IsValidEmail(email))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Invalid Email";
                messageBox.Content = "Please enter a valid email address.";
                messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                messageBox.CloseButtonText = "OK";
                messageBox.ShowDialogAsync();
                return;
            }

            if (!IsValidPassword(password))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.";
                messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                messageBox.CloseButtonText = "OK";
                messageBox.ShowDialogAsync();
                return;
            }

            if (password != confirmPassword)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = "Passwords do not match.";
                messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                messageBox.CloseButtonText = "OK";
                messageBox.ShowDialogAsync();

                return;
            }

            if (RegistrarUsuario(username, email, password))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Success";
                messageBox.Content = "Account created successfully!";
                messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Success;
                messageBox.CloseButtonText = "OK";
                messageBox.ShowDialogAsync();

                Login login = new Login();
                login.Show();
                this.Close();
            }
            else
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = "An account with this username and/or email already exists.";
                messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                messageBox.CloseButtonText = "OK";
                messageBox.ShowDialogAsync();
            }
        }

            private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Verifica se a senha tem pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula e um número
            if (password.Length < 8)
                return false;

            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigits = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsDigit(c)) hasDigits = true;

                if (hasUpperCase && hasLowerCase && hasDigits)
                    return true;
            }

            return false;
        }

        private bool RegistrarUsuario(string username, string email, string password)
        {
            try{
                string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkUserQuery = "SELECT COUNT(*) FROM utilizador WHERE username = @username OR email = @email";
                    using (SqlCommand checkCmd = new SqlCommand(checkUserQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        checkCmd.Parameters.AddWithValue("@email", email);
                        int userCount = (int)checkCmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            return false;
                        }
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    string insertUserQuery = "INSERT INTO utilizador (nome, username, email, hashPassword, fotografia, linguagem) VALUES (@nome, @username, @email, @hashPassword, @fotografia, @linguagem)";
                    using (SqlCommand insertCmd = new SqlCommand(insertUserQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@nome", Environment.UserName);
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@email", email);
                        insertCmd.Parameters.AddWithValue("@hashPassword", hashedPassword);
                        insertCmd.Parameters.AddWithValue("@fotografia", "pack://application:,,,/Dados/noPhoto.jpg");
                        insertCmd.Parameters.AddWithValue("@linguagem", "pt-PT");
                        insertCmd.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log a exceção, se necessário, e retornar falso para indicar falha
                Console.WriteLine("Erro de SQL: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Log a exceção, se necessário, e retornar falso para indicar falha
                Console.WriteLine("Erro: " + ex.Message);
                return false;
            }
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();

        }
    }
}
