using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Projeto.Models;
using Wpf.Ui.Controls;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for EditarAlerta.xaml
    /// </summary>
    public partial class EditarAlerta : Window
    {
        private App app;
        public EditarAlerta()
        {
            InitializeComponent();
            app = App.Current as App;
            this.DataContext = this;
            app.model.AlertaEditado += EditarAlertaFeito;
            ToggleBtnEstado_OnChecked(this ,null);
            datePAlerta.Language = System.Windows.Markup.XmlLanguage.GetLanguage(app.model.linguaApp);
        }

        private void EditarAlertaFeito()
        {
            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
            mg.Title = Application.Current.FindResource("AlertaTitle") as string;
            mg.Content = Application.Current.FindResource("AlertaContent") as string;
            mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
            mg.CloseButtonAppearance = ControlAppearance.Info;
            mg.ShowDialogAsync();
        }

        private void DatePInicio_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (datePAlerta.SelectedDate < DateTime.Today)
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("DatasTitle") as string;
                mg.Content = Application.Current.FindResource("DatasContentInicioMenorHoje") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                datePAlerta.SelectedDate = DateTime.Today;
            }
        }

        private void BtnCancelar_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        private void BtnEditar_OnClick(object sender, RoutedEventArgs e)
        {
            Alerta alerta = (Alerta)this.DataContext;
            alerta.Tipos.Clear();
            if (btnEmail.IsChecked == true)
            {
                alerta.Tipos.Add(TipoAlerta.Email);
            }
            if (btnWindows.IsChecked == true)
            {
               alerta.Tipos.Add(TipoAlerta.AlertaWindows);
            }

            alerta.Data_Hora = new DateTime(datePAlerta.SelectedDate.Value.Year, datePAlerta.SelectedDate.Value.Month,
                datePAlerta.SelectedDate.Value.Day, timePickerHoraAlerta.Value.Value.Hour,
                timePickerHoraAlerta.Value.Value.Minute, timePickerHoraAlerta.Value.Value.Second);
            alerta.Desligado = toggleBtnEstado.IsChecked == true ? false : true;

            app.model.EditarAlerta(alerta);
            this.DialogResult = true;
        }


        private void ToggleBtnEstado_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Alerta)DataContext).Desligado == false)
            {
                toggleBtnEstado.IsChecked = true;
            }
            else
            {
                toggleBtnEstado.IsChecked = false;
            }
        }

        private void BtnEmail_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Alerta)DataContext).Tipos.Contains(TipoAlerta.Email) == true)
            {
                btnEmail.IsChecked = true;
            }
            else
            {
                btnEmail.IsChecked = false;
            }
        }

        private void BtnWindows_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Alerta)DataContext).Tipos.Contains(TipoAlerta.AlertaWindows) == true)
            {
                btnWindows.IsChecked = true;
            }
            else
            {
                btnWindows.IsChecked = false;
            }
        }
        private void BtnEmail_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnWindows.IsChecked == true)
            {
                btnEmail.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                btnEmail.IsChecked = true;
            }
        }

        private void BtnWindows_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnEmail.IsChecked == true)
            {
                btnWindows.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                btnWindows.IsChecked = true;
            }
        }

        private void EditarAlerta_OnClosing(object? sender, CancelEventArgs e)
        {
            app.model.AlertaEditado -= EditarAlertaFeito;
        }

        private void ToggleBtnEstado_OnChecked(object sender, RoutedEventArgs e)
        {
            btnEmail.IsEnabled = true;
            btnWindows.IsEnabled = true;
        }

        private void ToggleBtnEstado_OnUnchecked(object sender, RoutedEventArgs e)
        {
            btnEmail.IsEnabled = false;
            btnWindows.IsEnabled = false;
        }

    }
    public class ConverterMensagem : IValueConverter
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
    public class ConverterId : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int idValue)
            {
                return Application.Current.FindResource("txtAlerta") + idValue.ToString();
            }
            return "Alerta";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
