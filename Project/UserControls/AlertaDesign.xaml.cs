using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Projeto.Models;
using Wpf.Ui.Controls;


namespace Projeto
{
    /// <summary>
    /// Interaction logic for AlertaDesign.xaml
    /// </summary>
    public partial class AlertaDesign : UserControl
    {
        private App app;
        public AlertaDesign()
        {
            InitializeComponent();
            app = App.Current as App;
        }

        private void BtnConfiguracoesAlerta_Onclick(object sender, RoutedEventArgs e)
        {
            app.ViewEditarAlerta = new EditarAlerta();
            app.ViewEditarAlerta.DataContext = this.DataContext;
            app.ViewEditarAlerta.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            app.ViewEditarAlerta.ShowDialog();
        }

        private async void BtnEliminarAlerta_Onclick(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
            mg.Title = "Confirmação de eliminação";
            mg.Content = "Tem a certeza que quer eliminar o alerta ?";
            mg.PrimaryButtonText = "Sim";
            mg.CloseButtonText = "Não";

            var result = await mg.ShowDialogAsync();

            // Verificar a resposta do usuário
            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                // Se o usuário clicou em "Sim"
                app.model.EliminarAlerta(this.DataContext as Alerta);
            }
        }

    }

    public class MessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string message)
            {
                if (message == "Alerta de Nao Realizacao")
                {
                    return Application.Current.FindResource("alertaNaoRealizacao");
                }
                else if (message == "Alerta de Antecipacao")
                {
                    return Application.Current.FindResource("alertaAntecipacao");
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}