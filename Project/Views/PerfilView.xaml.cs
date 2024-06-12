using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Projeto;
using Projeto.Models;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Projeto.Views
{
    /// <summary>
    /// Interaction logic for Perfil.xaml
    /// </summary>
    public partial class PerfilView : Window
    {
        private App app;
        public PerfilView()
        {
            InitializeComponent();
            app = App.Current as App;
            AtualizarComponentesGraficas();
            app.model.AtualizacaoPerfilFeita += AtualizacaoPerfilFeita;

        }

        private void AtualizarComponentesGraficas()
        {
            txtbNome.Text = app.model.perfil.Nome;
            txtbEmail.Text = app.model.perfil.Email;
            if (app.model.perfil.Fotografia != null)
                imagePerfilTemp.Source = app.model.perfil.Fotografia.Source;
        }
        private void AtualizacaoPerfilFeita()
        {
            var mg = new Wpf.Ui.Controls.MessageBox
            {
                Title = (string)Application.Current.FindResource("Atualizacao") as string,
                Content = (string)Application.Current.FindResource("AtualizacaoBemSucedida") as string,
                CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText"),
            CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Info
        };
            mg.ShowDialogAsync();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Cria uma instância de OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Define o filtro para permitir apenas arquivos de imagem
                Filter = (string)Application.Current.FindResource("ImagensFilter") as string + " | *.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            // Exibe a caixa de diálogo de arquivo e verifica se o usuário selecionou um arquivo
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Cria um URI a partir do caminho do arquivo selecionado
                    Uri fileUri = new Uri(openFileDialog.FileName);

                    // Cria um BitmapImage a partir do URI
                    BitmapImage bitmapImage = new BitmapImage(fileUri);

                    if (imagePerfilTemp == null)
                    {
                        throw new Exception(" A Imagem não foi inicializada");
                    }
                    // Define a fonte da imagem do controle de imagem para o BitmapImage
                    imagePerfilTemp.Source = bitmapImage;

                    // Congela o BitmapImage para tornar a imagem somente leitura e melhorar o desempenho
                    bitmapImage.Freeze();
                }
                catch (Exception ex)
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                    mg.Content = (string)Application.Current.FindResource("ErroCarregarImagem") as string;
                    mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                    mg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Info;
                    mg.ShowDialogAsync();
                }
            }
        }




        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(txtbNome.Text.Length == 0 || txtbEmail.Text.Length == 0)
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                mg.Content = (string)Application.Current.FindResource("PreenchaTodosOsCampos") as string;
                mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                mg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Info;
                mg.ShowDialogAsync();
                return;
            }
            else
            {
                app.model.AtualizarInformacoesPerfil(txtbNome.Text, txtbEmail.Text, imagePerfilTemp, languageComboBox.SelectedIndex);
            
                this.DialogResult = true;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AtualizarComponentesGraficas();
            this.DialogResult = true;
        }

        private void PerfilView_OnClosing(object? sender, CancelEventArgs e)
        {
            app.model.AtualizacaoPerfilFeita -= AtualizacaoPerfilFeita;
        }

        private void txtbNome_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtbNome.Text.Length >= 16)
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                mg.Content = (string)Application.Current.FindResource("NomeMuitoGrande") as string;
                mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                mg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Info;
                mg.ShowDialogAsync();
                txtbNome.Text = "";
            }


        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Usa regex para validar o formato do email
                string emailPattern = @"^[^.@\s]+@[^.@\s]+\.[^@.\s]+$";
                bool isValid = Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);

                if (!isValid)
                    return false;

                // Verifica se o endereço é válido
                MailAddress address = new MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void txtbEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!IsValidEmail(txtbEmail.Text))
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                mg.Content = (string)Application.Current.FindResource("EmailInvalido") as string;
                mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                mg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Info;
                mg.ShowDialogAsync();
                txtbEmail.Text = "";
            }
        }

        private void LanguageComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            switch (app.model.linguaApp)
            {
                case "en-US":
                    languageComboBox.SelectedIndex = 1;
                    break;
                case "pt-PT":
                    languageComboBox.SelectedIndex = 0;
                    break;
                case "es-ES":
                    languageComboBox.SelectedIndex = 2;
                    break;
                case "fr-FR":
                    languageComboBox.SelectedIndex = 3;
                    break;
                case "de-DE":
                    languageComboBox.SelectedIndex = 4;
                    break;
                case "it-IT":
                    languageComboBox.SelectedIndex = 5;
                    break;
                default:
                    languageComboBox.SelectedIndex = 0;
                    break;
            }
        }
    }
}