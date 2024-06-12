using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BCrypt.Net;
using Microsoft.Data.SqlClient;
using Projeto.Models; // Certifique-se de instalar via NuGet

namespace Projeto.Views
{
    public partial class Login : Window
    {
        private App app;
        public Login()
        {
            InitializeComponent();
            app = App.Current as App;
        }

        private void BtnLogin_OnClick(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
            string username = UsernameTextBox.Text;
            string password = passwordBox.Password; // Senha inserida pelo usuário

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT hashPassword FROM utilizador WHERE username = @username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var hashFromDatabase = cmd.ExecuteScalar() as string;

                    if (hashFromDatabase != null && BCrypt.Net.BCrypt.Verify(password, hashFromDatabase))
                    {
                        // A senha está correta, agora obter os detalhes do perfil do usuário
                        string profileQuery = "SELECT ID, nome, email, fotografia,linguagem FROM utilizador WHERE username = @username";
                        using (SqlCommand profileCmd = new SqlCommand(profileQuery, conn))
                        {
                            profileCmd.Parameters.AddWithValue("@username", username);
                            using (SqlDataReader reader = profileCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int id = reader.GetInt32(reader.GetOrdinal("ID"));
                                    string nome = reader.GetString(reader.GetOrdinal("nome"));
                                    string email = reader.GetString(reader.GetOrdinal("email"));
                                    string fotografia = reader.GetString(reader.GetOrdinal("fotografia"));
                                    string linguagem = reader.GetString(reader.GetOrdinal("linguagem"));

                                    Image image = new Image();
                                    image.Source = new BitmapImage(new Uri(fotografia));

                                    MainWindow main = new MainWindow(); // Supondo que MainWindow é sua janela principal

                                    app.model.AtualizarLinguagem(linguagem);
                                    app.model.AtualizarPerfil(new Perfil()
                                    {
                                        Id = id,
                                        Nome = nome,
                                        Email = email,
                                        Fotografia = image
                                    });

                                    main.Show();
                                    this.Close();
                                }
                                else
                                {
                                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                                    messageBox.Title = "Error";
                                    messageBox.Content = "Error obtaining details about user profile.";
                                    messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                                    messageBox.CloseButtonText = "OK";
                                    messageBox.ShowDialogAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Title = "Error";
                        messageBox.Content = "Login failed! Incorrect username or password.";
                        messageBox.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution;
                        messageBox.CloseButtonText = "OK";
                        messageBox.ShowDialogAsync();
                    }
                }
            }
        }

        private void BtnRegister_OnClick(object sender, RoutedEventArgs e)
        {
            Register register = new Register();
            register.Show();
            this.Close();
        }

    }
}