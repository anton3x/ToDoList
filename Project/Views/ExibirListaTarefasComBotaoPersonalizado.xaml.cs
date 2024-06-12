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
using MessageBox = Wpf.Ui.Controls.MessageBox;
using System.Windows.Markup;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for ExibirListaTarefas.xaml
    /// </summary>
    public partial class ExibirListaTarefasComBotaoPersonalizado : UserControl
    {
        private App app;
        private String textoTextBoxAntigo; // Guarda o texto antigo da TextBox para atualizar no model


        public ExibirListaTarefasComBotaoPersonalizado(string textoPagina)
        {
            InitializeComponent();

            app = App.Current as App;
            txtbTarefas.Text = textoPagina;

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
            txtbAdicionarTarefa.Text = Application.Current.FindResource("textBoxAddTarefaText") as string;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBoxFiltroListaTarefas.SelectedIndex)
            {
                case 0: // Hoje
                    stackPanelEntreDatasSelecao.IsEnabled = false;
                    stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
                    app.model.FiltrarTarefasEntreDatasBotaoPersonalizado(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today), txtbTarefas.Text);
                    break;

                case 1: // Esta semana
                    stackPanelEntreDatasSelecao.IsEnabled = false;
                    stackPanelEntreDatasSelecao.Visibility = Visibility.Collapsed;
                    app.model.FiltrarTarefasEntreDatasBotaoPersonalizado(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today).AddDays(6),txtbTarefas.Text);
                    break;

                case 2: // Entre datas
                    DateOnly dateInicio = new DateOnly(dtPickerDataInicioFiltragem.SelectedDate.Value.Year,
                        dtPickerDataInicioFiltragem.SelectedDate.Value.Month,
                        dtPickerDataInicioFiltragem.SelectedDate.Value.Day);
                    DateOnly dateFim = new DateOnly(dtPickerDataFimFiltragem.SelectedDate.Value.Year,
                        dtPickerDataFimFiltragem.SelectedDate.Value.Month, dtPickerDataFimFiltragem.SelectedDate.Value.Day);
                    stackPanelEntreDatasSelecao.IsEnabled = true;
                    stackPanelEntreDatasSelecao.Visibility = Visibility.Visible;
                    app.model.FiltrarTarefasEntreDatasBotaoPersonalizado(dateInicio, dateFim, txtbTarefas.Text);
                    break;

                default: //Quando nenhum filtro é selecionado
                    app.model.AtualizarListaFiltradaBotaoComListaPersonalizada(txtbTarefas.Text);
                    break;
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
                // Await the dialog result
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
                // Await the dialog result
                mg.ShowDialogAsync();
            }

            Selector_OnSelectionChanged(sender, e);
        }


        private void BtnAdicionar_Onclick(object sender, RoutedEventArgs e)
        {
            app.ViewAdicionarTarefa = new AdicionarTarefa();
            app.ViewAdicionarTarefa.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (!String.IsNullOrEmpty(txtbTarefas.Text))
                app.ViewAdicionarTarefa.NomeBotaoListaPersonalizada = txtbTarefas.Text;
            app.ViewAdicionarTarefa.ShowDialog();
        }

        private void TxtbAdicionarTarefa_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                app.ViewAdicionarTarefa = new AdicionarTarefa();
                app.ViewAdicionarTarefa.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (!String.IsNullOrEmpty(txtbTarefas.Text))
                    app.ViewAdicionarTarefa.NomeBotaoListaPersonalizada = txtbTarefas.Text;
                if (!String.IsNullOrEmpty(txtbAdicionarTarefa.Text))
                    app.ViewAdicionarTarefa.txtbTitulo.Text = txtbAdicionarTarefa.Text;
                listViewTarefas.Focus();
                app.ViewAdicionarTarefa.ShowDialog();
            }
        }

        private async void BtnEliminarButaoComListaPersonalizada_OnClick(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
            mg.Title = (string)Application.Current.FindResource("EliminarListaPersonalizadaTitle");
            mg.Content = (string)Application.Current.FindResource("EliminarListaPersonalizadaContent");
            mg.PrimaryButtonText = (string)Application.Current.FindResource("EliminarListaPersonalizadaPrimaryButtonText"); // (string)Application.Current.FindResource("Sim")
            mg.CloseButtonText = (string)Application.Current.FindResource("EliminarListaPersonalizadaCloseButtonText"); // (string)Application.Current.FindResource("Nao")
            mg.PrimaryButtonAppearance = ControlAppearance.Primary;
            mg.CloseButtonAppearance = ControlAppearance.Secondary;

            // Await the dialog result
            var result = await mg.ShowDialogAsync();

            // Check if the primary button was clicked
            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                app.model.ApagarBotaoComListaPersonalizada(txtbTarefas.Text);
            }
        }


        private void BtnConfiguracoesButaoComListaPersonalizada_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = txtbTarefas as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                textoTextBoxAntigo = textBox.Text; // Guarda o texto antigo para atualizar no model
                textBox.IsReadOnly = false; // Permite edição
                textBox.Focusable = true; // Torna a caixa de texto focalizável
                textBox.CaretBrush = Application.Current.Resources["TextoPrincipal"] as SolidColorBrush;  // Define a cor do cursor para preto
                textBox.Foreground = Application.Current.Resources["TextoPrincipal"] as SolidColorBrush; // Define a cor do texto para preto
                textBox.Focus(); // Coloca o foco na TextBox para iniciar a edição
            }
        }

        private void TxtbTarefas_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
                if (textBox != null)
                {
                    if (textoTextBoxAntigo != textBox.Text) //para verificar se ele alterou o nome ou mantem-se o mesmo
                    {
                        if (!app.model.ExisteBotaoComNome(textBox.Text))
                        {
                            app.model.AtualizarNomeBotaoComListaPersonalizada(textoTextBoxAntigo, textBox.Text);
                            textoTextBoxAntigo = textBox.Text;
                        }
                        else
                        {
                            textBox.Text = textoTextBoxAntigo; // Retorna o texto antigo
                            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                            mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                            mg.Content = (string)Application.Current.FindResource("ErrorButtonNameNotAllowedContent");
                            mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                            mg.CloseButtonAppearance = ControlAppearance.Info;
                            mg.ShowDialogAsync();
                        }
                    }
                    textBox.IsReadOnly = true; // Retorna para somente leitura
                    textBox.Focusable = false; // Não permite foco
                    textBox.Foreground = Application.Current.Resources["TextoPrincipal"] as SolidColorBrush; // Define a cor do texto para preto
                }
            }
        }

        private void TxtbTarefas_OnLostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                if (textoTextBoxAntigo != textBox.Text) //para verificar se ele alterou o nome ou mantem-se o mesmo
                {
                    if (!app.model.ExisteBotaoComNome(textBox.Text))
                    {
                        app.model.AtualizarNomeBotaoComListaPersonalizada(textoTextBoxAntigo, textBox.Text);
                        textoTextBoxAntigo = textBox.Text;
                    }
                    else
                    {
                        textBox.Text = textoTextBoxAntigo; // Retorna o texto antigo
                        Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                        mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle");
                        mg.Content = (string)Application.Current.FindResource("ErrorButtonNameNotAllowedContent");
                        mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText");
                        mg.CloseButtonAppearance = ControlAppearance.Info;
                        mg.ShowDialogAsync();
                    }
                }
                textBox.IsReadOnly = true; // Retorna para somente leitura
                textBox.Focusable = false; // Não permite foco
                textBox.Foreground = Application.Current.Resources["TextoPrincipal"] as SolidColorBrush; // Define a cor do texto para preto
            }
        }
    }
}
