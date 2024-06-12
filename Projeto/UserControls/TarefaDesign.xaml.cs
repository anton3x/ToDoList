using Projeto.Models;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for TarefaDesign.xaml
    /// </summary>
    public partial class TarefaDesign : UserControl
    {
        private App app;
        private bool listaAlertasAntecipacaoMaisDoQue0Elementos { get; set; }
        private bool listaAlertasNaoRealizacaoMaisDoQue0Elementos { get; set; }
        public string AlertaDeAntecipacao { get; set; }
        public string AlertaDeNaoRealizacao { get; set; }

        public TarefaDesign()
        {
            InitializeComponent();
            app = App.Current as App;
        }

        private void BtnConfiguracoesTarefa_Onclick(object sender, RoutedEventArgs e)
        {
            EditarTarefa editarTarefa = new EditarTarefa();
            editarTarefa.DataContext = this.DataContext;
            editarTarefa.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            editarTarefa.ShowDialog();
        }

        private void BtnCentralAlertasTarefa_Onclick(object sender, RoutedEventArgs e)
        {
            CentralAlertas centralAlertas = new CentralAlertas((Tarefa)this.DataContext);
            centralAlertas.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ((Tarefa)this.DataContext).AtualizarAlertasTodos();
            centralAlertas.ShowDialog();
        }

        private void BtnEliminarTarefa_Onclick(object sender, RoutedEventArgs e)
        {
            app.model.EliminarElementoLista(this.DataContext as Tarefa);
        }

        private void RadioBtnCheckTarefa_OnClick(object sender, RoutedEventArgs e)
        {
            Tarefa tarefa = (Tarefa)this.DataContext;
            int contador = 0;

            foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
            {
                contador++;
            }

            if (tarefa.Estado != Estado.PorTerminar)
            {
                if (tarefa.DataInicio.Date == tarefa.DataFim.Date || contador == 1)
                {
                    tarefa.Estado = Estado.PorTerminar;
                    app.model.EliminarAlertasTarefa(tarefa);
                    radioBtnCheckTarefa.IsChecked = true;
                }
                else
                {
                    radioBtnCheckTarefa.IsChecked = false;
                    TarefasChecked tarefasChecked = new TarefasChecked();
                    tarefasChecked.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    tarefasChecked.DataContext = this.DataContext;
                    tarefasChecked.ShowDialog();

                    contador = 0;
                    foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
                    {
                        if (dia.ativo)
                            contador++;
                    }

                    if (contador == 0)
                    {
                        tarefa.Estado = Estado.PorTerminar;
                        app.model.EditarElementoLista(tarefa);
                        app.model.EliminarAlertasTarefa(tarefa);
                        radioBtnCheckTarefa.IsChecked = true;
                    }

                }
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = "Erro";
                mg.Content = "Não é possível terminar uma tarefa que já foi concluída.";
                mg.CloseButtonText = "OK";
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }
        }

        private void RadioBtnCheckTarefa_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Tarefa tarefa = (Tarefa)this.DataContext;
            if (tarefa.Estado == Estado.PorTerminar)
            {
                radioBtnCheckTarefa.IsChecked = true;
            }
            else
            {
                radioBtnCheckTarefa.IsChecked = false;
            }
        }

        private void BtnEstadoTarefa_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Tarefa tarefa = (Tarefa)this.DataContext;
            switch (tarefa.Estado)
            {
                case Estado.PorComecar:
                    btnEstadoTarefa.Background = System.Windows.Media.Brushes.LightGray;
                    break;
                case Estado.PorTerminar:
                    btnEstadoTarefa.Background = System.Windows.Media.Brushes.LightGreen;
                    break;
                case Estado.EmExecucao:
                    btnEstadoTarefa.Background = System.Windows.Media.Brushes.Orange;
                    break;

            }
            app.model.AlterarEstadoTarefaBD(tarefa);
        }

        //Método que muda o estado da tarefa, de por começar para em execução, de em execução para por terminar e de por terminar para por começar
        private void BtnEstadoTarefa_OnClick(object sender, RoutedEventArgs e)
        {
            Tarefa tarefa = (Tarefa)this.DataContext;
            if (tarefa.Estado == Estado.PorComecar)
            {
                tarefa.Estado = Estado.EmExecucao;
                radioBtnCheckTarefa.IsChecked = false;
                btnEstadoTarefa.Background = System.Windows.Media.Brushes.Orange;
            }
            else if (tarefa.Estado == Estado.EmExecucao)
            {
                tarefa.Estado = Estado.PorTerminar;
                app.model.EliminarAlertasTarefa(tarefa);
                radioBtnCheckTarefa.IsChecked = true;
                btnEstadoTarefa.Background = System.Windows.Media.Brushes.LightGreen;
            }
            else if (tarefa.Estado == Estado.PorTerminar)
            {
                tarefa.Estado = Estado.PorComecar;
                btnEstadoTarefa.Background = System.Windows.Media.Brushes.LightGray;
                radioBtnCheckTarefa.IsChecked = false;
            }
            app.model.AlterarEstadoTarefaBD(tarefa);

        }
    }
    public class ListCountToAlertTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as IList;
            if (list == null)
                return null;
            if(parameter.ToString() == "Antecipacao")
                return list.Count > 0 ? " <> " + Application.Current.FindResource("tarefaDesignAntecipacao") : "";
            else
                return list.Count > 0 ? " <> " + Application.Current.FindResource("tarefaDesignNaoRealizacao") : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImportanceLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string importanceLevel = value.ToString();

            switch (importanceLevel)
            {
                case "Normal":
                    return Application.Current.FindResource("btnTarefasNormalName");
                case "PoucoImportante":
                    return Application.Current.FindResource("btnTarefasPoucoImportanteName");
                case "Importante":
                    return Application.Current.FindResource("btnTarefasImportanteName");
                case "Prioritaria":
                    return Application.Current.FindResource("btnTarefasPrioritariaName");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
