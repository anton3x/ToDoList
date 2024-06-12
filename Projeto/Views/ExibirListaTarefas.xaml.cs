using System.IO;
using Projeto.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
using Wpf.Ui.Controls;
using System.Windows.Markup;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for ExibirListaTarefas.xaml
    /// </summary>
    public partial class ExibirListaTarefas : UserControl
    {
        private App app;

        public ExibirListaTarefas(string textoPagina, BitmapImage imageIcon)
        {
            InitializeComponent();

            app = App.Current as App;
            iconTarefas.Source = imageIcon;
            txtbTarefas.Text = textoPagina;

            if (textoPagina == (string)FindResource("btnTarefasHojeName") || textoPagina == (string)FindResource("btnTarefasEstaSemanaName")) //remover o filtro de tarefas pois nao e necessario
            {
                comboBoxFiltroListaTarefas.IsEnabled = false;
                comboBoxFiltroListaTarefas.Visibility = Visibility.Collapsed;
                stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
            }

            app.model.TarefaEditada += ModelOnTarefaEditada;
            app.model.TarefaAdicionada += ModelOnTarefaEditada;
            app.model.TarefaEliminada += ModelOnTarefaEditada;
            app.model.ListaAlertasAlterada += ModelOnTarefaEditada;

            this.DataContext = app.model;

            dtPickerDataFimFiltragem.Language = XmlLanguage.GetLanguage(app.model.linguaApp);
            dtPickerDataInicioFiltragem.Language = XmlLanguage.GetLanguage(app.model.linguaApp);
        }
        void ModelOnTarefaEditada() // quando a tarefa for editada faz isto para atualizar a lista
        {
            Selector_OnSelectionChanged(this, null);
        }
        private void TxtbAdicionarTarefa_OnGotFocus(object sender, RoutedEventArgs e)
        {
            txtbAdicionarTarefa.Text = "";
        }

        private void TxtbAdicionarTarefa_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string newListText = (string)FindResource("textBoxAddTarefaText");

            // Define o texto do TextBox com o valor do ResourceDictionary
            txtbAdicionarTarefa.Text = newListText;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (comboBoxFiltroListaTarefas.SelectedIndex)
                {
                    case 0: // Hoje
                        stackPanelEntreDatasSelecao.IsEnabled = false;
                        stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
                        if (txtbTarefas.Text == (string)FindResource("btnTarefasPoucoImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasNormalName") ||
                            txtbTarefas.Text == (string)FindResource("btnTarefasImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasPrioritariaName"))
                        {
                            app.model.FiltrarTarefasHoje(txtbTarefas.Text);
                        }
                        else
                        {
                            app.model.FiltrarTarefasHoje();
                        }

                        break;

                    case 1: // Esta semana
                        stackPanelEntreDatasSelecao.IsEnabled = false;
                        stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
                        if (txtbTarefas.Text == (string)FindResource("btnTarefasPoucoImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasNormalName") ||
                            txtbTarefas.Text == (string)FindResource("btnTarefasImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasPrioritariaName"))
                        {
                            app.model.FiltrarTarefas(DateOnly.FromDateTime(DateTime.Today),
                                DateOnly.FromDateTime(DateTime.Today).AddDays(6), txtbTarefas.Text);
                        }
                        else
                        {
                            app.model.FiltrarTarefas(DateOnly.FromDateTime(DateTime.Today),
                                DateOnly.FromDateTime(DateTime.Today).AddDays(6));
                        }

                        break;

                    case 2: // Entre datas
                        DateOnly dateInicio = new DateOnly(dtPickerDataInicioFiltragem.SelectedDate.Value.Year,
                            dtPickerDataInicioFiltragem.SelectedDate.Value.Month,
                            dtPickerDataInicioFiltragem.SelectedDate.Value.Day);
                        DateOnly dateFim = new DateOnly(dtPickerDataFimFiltragem.SelectedDate.Value.Year,
                            dtPickerDataFimFiltragem.SelectedDate.Value.Month,
                            dtPickerDataFimFiltragem.SelectedDate.Value.Day);
                        stackPanelEntreDatasSelecao.IsEnabled = true;
                        stackPanelEntreDatasSelecao.Visibility = Visibility.Visible;
                        if (txtbTarefas.Text == (string)FindResource("btnTarefasPoucoImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasNormalName") ||
                            txtbTarefas.Text == (string)FindResource("btnTarefasImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasPrioritariaName"))
                        {
                            app.model.FiltrarTarefas(dateInicio, dateFim, txtbTarefas.Text);
                        }
                        else
                        {
                            app.model.FiltrarTarefas(dateInicio, dateFim);
                        }

                        break;

                    default: //Quando nenhum filtro é selecionado
                        if (txtbTarefas.Text == (string)FindResource("btnTarefasPoucoImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasNormalName") ||
                            txtbTarefas.Text == (string)FindResource("btnTarefasImportanteName") || txtbTarefas.Text == (string)FindResource("btnTarefasPrioritariaName"))
                        {
                            app.model.FiltrarTarefas(txtbTarefas.Text);
                        }
                        else
                        {
                            if (txtbTarefas.Text == (string)FindResource("btnTarefasHojeName"))
                                app.model.FiltrarTarefasHoje();
                            else if (txtbTarefas.Text == (string)FindResource("btnTarefasEstaSemanaName"))
                            {
                                app.model.FiltrarTarefas(DateOnly.FromDateTime(DateTime.Today),
                                    DateOnly.FromDateTime(DateTime.Today.AddDays(6)));
                            }
                            else if (txtbTarefas.Text == (string)FindResource("buttonTarefasName"))
                            {
                                app.model.FiltrarTarefasTodas();
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = "Erro";
                mg.Content = "Erro ao filtrar as tarefas!!!";
                mg.CloseButtonText = "OK";
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }
        }

        private void DtPickerDataInicioFiltragem_OnLoaded(object sender, RoutedEventArgs e)
        {
            dtPickerDataInicioFiltragem.SelectedDate = DateTime.Today;
            dtPickerDataFimFiltragem.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void StackPanelEntreDatasSelecao_OnLoaded(object sender, RoutedEventArgs e)
        {
            stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
        }

        private void DtPickerDataInicioFiltragem_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dtPickerDataInicioFiltragem.SelectedDate > dtPickerDataFimFiltragem.SelectedDate)
            {
                dtPickerDataInicioFiltragem.SelectedDate = dtPickerDataFimFiltragem.SelectedDate;
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("DatasTitle") as string;
                mg.Content = Application.Current.FindResource("DatasContentInicioMaiorFim") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }


            Selector_OnSelectionChanged(sender, e);
        }

        private void DtPickerDataFimFiltragem_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dtPickerDataFimFiltragem.SelectedDate < dtPickerDataInicioFiltragem.SelectedDate)
            {
                dtPickerDataFimFiltragem.SelectedDate = dtPickerDataInicioFiltragem.SelectedDate;
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("DatasTitle") as string;
                mg.Content = Application.Current.FindResource("DatasContentFimMenorInicio") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }

            Selector_OnSelectionChanged(sender, e);
        }


        private void BtnAdicionar_Onclick(object sender, RoutedEventArgs e)
        {
            app.ViewAdicionarTarefa = new AdicionarTarefa();
            if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasPoucoImportanteName") as string)
                app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 0;
            else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasNormalName") as string)
                app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 1;
            else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasImportanteName") as string)
                app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 2;
            else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasPrioritariaName") as string)
            {
                app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 3;
                app.ViewAdicionarTarefa.toggleBtnAlertaAntecipacao.IsChecked = true;
                app.ViewAdicionarTarefa.toggleBtnAlertaAntecipacao.IsEnabled = false;
                app.ViewAdicionarTarefa.toggleBtnAlertaNaoRealizacao.IsChecked = true;
                app.ViewAdicionarTarefa.toggleBtnAlertaNaoRealizacao.IsEnabled = false;
            }
            app.ViewAdicionarTarefa.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            app.ViewAdicionarTarefa.ShowDialog();
        }

        private void TxtbAdicionarTarefa_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                app.ViewAdicionarTarefa = new AdicionarTarefa();

                if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasPoucoImportanteName") as string)
                    app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 0;
                else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasNormalName") as string)
                    app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 1;
                else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasImportanteName") as string)
                    app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 2;
                else if (txtbTarefas.Text == Application.Current.FindResource("btnTarefasPrioritariaName") as string)
                {
                    app.ViewAdicionarTarefa.comboBPrioridades.SelectedIndex = 3;
                    app.ViewAdicionarTarefa.toggleBtnAlertaAntecipacao.IsChecked = true;
                    app.ViewAdicionarTarefa.toggleBtnAlertaAntecipacao.IsEnabled = false;
                    app.ViewAdicionarTarefa.toggleBtnAlertaNaoRealizacao.IsChecked = true;
                    app.ViewAdicionarTarefa.toggleBtnAlertaNaoRealizacao.IsEnabled = false;
                }

                app.ViewAdicionarTarefa.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if(!String.IsNullOrEmpty(txtbAdicionarTarefa.Text))
                    app.ViewAdicionarTarefa.txtbTitulo.Text = txtbAdicionarTarefa.Text;
                listViewTarefas.Focus();
                app.ViewAdicionarTarefa.ShowDialog();

            }
        }
    }
}
