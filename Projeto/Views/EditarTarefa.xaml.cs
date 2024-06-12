using Projeto.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for AdicionarTarefa.xaml
    /// </summary>
    public partial class EditarTarefa : Window
    {
        private App app;
        public EditarTarefa()
        {
            InitializeComponent();
            //ligar
            app = App.Current as App;
            datePInicio.Language = System.Windows.Markup.XmlLanguage.GetLanguage(app.model.linguaApp);
            datePFim.Language = System.Windows.Markup.XmlLanguage.GetLanguage(app.model.linguaApp);
        }

        private void inicializarBotoesDeRepeticao(bool estadoCheckedBotoes, DayOfWeek? diaComecoTarefa = null)
        {
            // Mapeia os dias da semana para os botões correspondentes
            var botoesDiasSemana = new Dictionary<DayOfWeek, ToggleButton>
            {
                { DayOfWeek.Monday, btnSegunda },
                { DayOfWeek.Tuesday, btnTerca },
                { DayOfWeek.Wednesday, btnQuarta },
                { DayOfWeek.Thursday, btnQuinta },
                { DayOfWeek.Friday, btnSexta },
                { DayOfWeek.Saturday, btnSabado },
                { DayOfWeek.Sunday, btnDomingo }
            };

            // Define o botão do dia de início da tarefa
            if (diaComecoTarefa != null)
            {
                botoesDiasSemana[diaComecoTarefa.Value].IsChecked = true;

                // Define o estado dos outros botões verificando se o dia de início da tarefa é igual ao dia de repetição selecionado
                foreach (var diaBotao in botoesDiasSemana)
                {
                    if (diaBotao.Key != diaComecoTarefa)
                    {
                        diaBotao.Value.IsChecked = estadoCheckedBotoes;
                    }
                }
            }
            else
            {
                // Define o estado dos outros botões para o estado passado como argumento
                foreach (var diaBotao in botoesDiasSemana)
                {
                    diaBotao.Value.IsChecked = estadoCheckedBotoes;
                }
            }
        }
        //quando da check ao botao repetir tarefa
        //se as datas forem diferentes, seleciona o dia que comeca
        private void ToggleBtnRepetir_OnChecked(object sender, RoutedEventArgs e)
        {
            if (datePFim.SelectedDate != null && datePInicio.SelectedDate != null && datePInicio.SelectedDate != datePFim.SelectedDate)
            {
                //ativar a repeticao, colocando a verde, a dizer ON e ativar os dias
                estadoToggleBtnRepetir.Foreground = new SolidColorBrush(Colors.Green);
                estadoToggleBtnRepetir.Text = "ON";

                stackPanelDias.IsEnabled = true;
                toggleBtnRepetir.IsChecked = true;

                //removemos a selecao de todos os dias da semana
                inicializarBotoesDeRepeticao(false, datePInicio.SelectedDate.Value.DayOfWeek);

                //para cada dia no intervalo de datas, selecionar o dia da semana correspondente
                if ((datePFim.SelectedDate.Value - datePInicio.SelectedDate.Value).TotalDays <= 6)
                {
                    //se forem menos que uma semana, selecionar os dias da semana correspondentes
                    for (DateTime data = datePInicio.SelectedDate.Value;
                         data <= datePFim.SelectedDate.Value;
                         data = data.AddDays(1))
                    {
                        // Verificar se o dia da semana da data atual é igual ao dia de repetição selecionado
                        switch (data.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                btnSegunda.IsChecked = true;
                                break;
                            case DayOfWeek.Tuesday:
                                btnTerca.IsChecked = true;
                                break;
                            case DayOfWeek.Wednesday:
                                btnQuarta.IsChecked = true;
                                break;
                            case DayOfWeek.Thursday:
                                btnQuinta.IsChecked = true;
                                break;
                            case DayOfWeek.Friday:
                                btnSexta.IsChecked = true;
                                break;
                            case DayOfWeek.Saturday:
                                btnSabado.IsChecked = true;
                                break;
                            case DayOfWeek.Sunday:
                                btnDomingo.IsChecked = true;
                                break;
                        }
                    }
                }
                else
                {
                    //se forem mais que uma semana, selecionar todos os dias da semana
                    inicializarBotoesDeRepeticao(true);
                }

            }
        }

        //quando da uncheck ao botao repetir tarefa, se for uma tarefa em que as datas sao iguais,
        //desativa a repeticao , mas se as datas forem diferentes, nao faz nada pois nao precisa de desativar a repeticao
        private void ToggleBtnRepetir_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (datePInicio.SelectedDate == datePFim.SelectedDate)
            {
                Brush brush = new SolidColorBrush(Colors.Red);
                estadoToggleBtnRepetir.Foreground = brush;
                estadoToggleBtnRepetir.Text = "OFF";
                stackPanelDias.IsEnabled = false;
                toggleBtnRepetir.IsChecked = false;

                inicializarBotoesDeRepeticao(false);
            }
        }
        private void ToggleBtnAlertaNaoRealizacao_OnChecked(object sender, RoutedEventArgs e)
        {
            Brush brush = new SolidColorBrush(Colors.Green);
            estadoToggleBtnAlertaNaoRealizacao.Foreground = brush;
            estadoToggleBtnAlertaNaoRealizacao.Text = "ON";
            stackPanelTiposNaoRealizacao.IsEnabled = true;
        }

        private void ToggleBtnAlertaNaoRealizacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Brush brush = new SolidColorBrush(Colors.Red);
            estadoToggleBtnAlertaNaoRealizacao.Foreground = brush;
            estadoToggleBtnAlertaNaoRealizacao.Text = "OFF";
            stackPanelTiposNaoRealizacao.IsEnabled = false;
        }

        private void ToggleBtnAlertaAntecipacao_OnChecked(object sender, RoutedEventArgs e)
        {
            Brush brush = new SolidColorBrush(Colors.Green);
            estadoToggleBtnAlertaAntecipacao.Foreground = brush;
            estadoToggleBtnAlertaAntecipacao.Text = "ON";
            stackPanelTiposAlertaAntecipacao.IsEnabled = true;
            txtbTempoParaAlertaAntecipacao.IsEnabled = true;
        }

        private void ToggleBtnAlertaAntecipacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Brush brush = new SolidColorBrush(Colors.Red);
            estadoToggleBtnAlertaAntecipacao.Foreground = brush;
            estadoToggleBtnAlertaAntecipacao.Text = "OFF";
            stackPanelTiposAlertaAntecipacao.IsEnabled = false;
            txtbTempoParaAlertaAntecipacao.IsEnabled = false;
        }

        private void BtnEmailAntecipacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnWindowsAntecipacao.IsChecked == true)
            {
                btnEmailAntecipacao.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();

                btnEmailAntecipacao.IsChecked = true;
            }
        }

        private void BtnWindowsAntecipacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnEmailAntecipacao.IsChecked == true)
            {
                btnWindowsAntecipacao.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                btnWindowsAntecipacao.IsChecked = true;
            }
        }

        private void BtnEmailNaoRealizacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnWindowsNaoRealizacao.IsChecked == true)
            {
                btnEmailNaoRealizacao.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                btnEmailNaoRealizacao.IsChecked = true;
            }
        }

        private void BtnWindowsNaoRealizacao_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (btnEmailNaoRealizacao.IsChecked == true)
            {
                btnWindowsNaoRealizacao.IsChecked = false;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("AlertasTitle") as string;
                mg.Content = Application.Current.FindResource("AlertasContent") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                btnWindowsNaoRealizacao.IsChecked = true;

            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            Tarefa tarefa = (Tarefa)this.DataContext;
            if (tarefa.Estado != Estado.PorTerminar)
            {
                app.model.EditarTarefa(((Tarefa)this.DataContext), txtbTitulo.Text, txtbDescricao.Text,
                    timePickerHoraInicio.Text, timePickerHoraFim.Text, (DateTime)datePInicio.SelectedDate,
                    (DateTime)datePFim.SelectedDate, (Importancia)comboBPrioridades.SelectedIndex, btnSegunda.IsChecked,
                    btnTerca.IsChecked, btnQuarta.IsChecked, btnQuinta.IsChecked, btnSexta.IsChecked,
                    btnSabado.IsChecked, btnDomingo.IsChecked, txtbTempoParaAlertaAntecipacao.Text,
                    toggleBtnAlertaAntecipacao.IsChecked, btnEmailAntecipacao.IsChecked,
                    btnWindowsAntecipacao.IsChecked, toggleBtnAlertaNaoRealizacao.IsChecked,
                    btnEmailNaoRealizacao.IsChecked
                    , btnWindowsNaoRealizacao.IsChecked, toggleBtnRepetir.IsChecked);
                this.DialogResult = true;
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("ErrorExistingButtonTitle") as string;
                mg.Content = Application.Current.FindResource("ErroTarefaJaConcluida") as string;
                mg.CloseButtonText = Application.Current.FindResource("ErrorExistingButtonCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }
        }
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ToggleBtnRepetir_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Tarefa)this.DataContext).Periodicidade.DiasSemana.Count > 0)
            {
                toggleBtnRepetir.IsChecked = true;
                toggleBtnRepetir.IsEnabled = false;
                estadoToggleBtnRepetir.Text = "ON";
                estadoToggleBtnRepetir.Foreground = new SolidColorBrush(Colors.Green);
                stackPanelDias.IsEnabled = true;

                foreach (DayOfWeek dia in ((Tarefa)this.DataContext).Periodicidade.DiasSemana)
                {
                    switch (dia)
                    {
                        case DayOfWeek.Monday:
                            btnSegunda.IsChecked = true;
                            break;
                        case DayOfWeek.Tuesday:
                            btnTerca.IsChecked = true;
                            break;
                        case DayOfWeek.Wednesday:
                            btnQuarta.IsChecked = true;
                            break;
                        case DayOfWeek.Thursday:
                            btnQuinta.IsChecked = true;
                            break;
                        case DayOfWeek.Friday:
                            btnSexta.IsChecked = true;
                            break;
                        case DayOfWeek.Saturday:
                            btnSabado.IsChecked = true;
                            break;
                        case DayOfWeek.Sunday:
                            btnDomingo.IsChecked = true;
                            break;
                    }
                }
            }
            else
            {
                toggleBtnRepetir.IsChecked = false;
                toggleBtnRepetir.IsEnabled = false;
                estadoToggleBtnRepetir.Text = "OFF";
                estadoToggleBtnRepetir.Foreground = new SolidColorBrush(Colors.Red);
                stackPanelDias.IsEnabled = false;
            }
        }

        private void ToggleBtnAlertaAntecipacao_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Tarefa)this.DataContext).AlertaAntecipacao.Count > 0)
            {
                toggleBtnAlertaAntecipacao.IsChecked = true;
                estadoToggleBtnAlertaAntecipacao.Text = "ON";
                estadoToggleBtnAlertaAntecipacao.Foreground = new SolidColorBrush(Colors.Green);
                stackPanelTiposAlertaAntecipacao.IsEnabled = true;
                txtbTempoParaAlertaAntecipacao.IsEnabled = true;

                foreach (TipoAlerta tipoAlerta in ((Tarefa)this.DataContext).AlertaAntecipacao[0].Tipos)
                {
                    if (tipoAlerta == TipoAlerta.Email)
                    {
                        btnEmailAntecipacao.IsChecked = true;
                    }
                    if (tipoAlerta == TipoAlerta.AlertaWindows)
                    {
                        btnWindowsAntecipacao.IsChecked = true;
                    }
                }
            }
            else
            {
                if(comboBPrioridades.SelectedIndex != 3)
                {
                    toggleBtnAlertaAntecipacao.IsChecked = false;
                    estadoToggleBtnAlertaAntecipacao.Text = "OFF";
                    estadoToggleBtnAlertaAntecipacao.Foreground = new SolidColorBrush(Colors.Red);
                    stackPanelTiposAlertaAntecipacao.IsEnabled = false;
                    txtbTempoParaAlertaAntecipacao.IsEnabled = false;
                }
                btnEmailAntecipacao.IsChecked = false;
                btnWindowsAntecipacao.IsChecked = true;
            }
        }

        private void ToggleBtnAlertaNaoRealizacao_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (((Tarefa)this.DataContext).AlertaExecucao.Count > 0)
            {
                toggleBtnAlertaNaoRealizacao.IsChecked = true;
                estadoToggleBtnAlertaNaoRealizacao.Text = "ON";
                estadoToggleBtnAlertaNaoRealizacao.Foreground = new SolidColorBrush(Colors.Green);
                stackPanelTiposNaoRealizacao.IsEnabled = true;
                foreach (TipoAlerta tipoAlerta in ((Tarefa)this.DataContext).AlertaExecucao[0].Tipos)
                {
                    if (tipoAlerta == TipoAlerta.Email)
                    {
                        btnEmailNaoRealizacao.IsChecked = true;
                    }
                    if (tipoAlerta == TipoAlerta.AlertaWindows)
                    {
                        btnWindowsNaoRealizacao.IsChecked = true;
                    }
                }
            }
            else
            {
                if (comboBPrioridades.SelectedIndex != 3)
                {
                    toggleBtnAlertaNaoRealizacao.IsChecked = false;
                    estadoToggleBtnAlertaNaoRealizacao.Text = "OFF";
                    estadoToggleBtnAlertaNaoRealizacao.Foreground = new SolidColorBrush(Colors.Red);
                    stackPanelTiposNaoRealizacao.IsEnabled = false;
                }
                btnEmailNaoRealizacao.IsChecked = false;
                btnWindowsNaoRealizacao.IsChecked = true;
            }
        }

        private void TimePickerHoraInicio_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (timePickerHoraFim.Text != null && timePickerHoraInicio.Text != null && timePickerHoraFim.Text.Length > 0 && timePickerHoraInicio.Text.Length > 0)
            {
                int minutosInicio = timePickerHoraInicio.Value.Value.Hour * 60 +
                                    timePickerHoraInicio.Value.Value.Minute;
                int minutosFim = timePickerHoraFim.Value.Value.Hour * 60 + timePickerHoraFim.Value.Value.Minute;

                if (minutosFim <= minutosInicio && datePInicio.SelectedDate == datePFim.SelectedDate)
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = Application.Current.FindResource("HorasTitle") as string;
                    mg.Content = Application.Current.FindResource("HorasContentInicioMaiorIgualFim") as string;
                    mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                    mg.CloseButtonAppearance = ControlAppearance.Info;
                    mg.ShowDialogAsync();
                    timePickerHoraInicio.Value = timePickerHoraFim.Value.Value.AddMinutes(-1);
                }
            }
        }

        private void TimePickerHoraFim_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (timePickerHoraInicio.Text != null && timePickerHoraFim.Text != null && timePickerHoraInicio.Text.Length > 0 && timePickerHoraFim.Text.Length > 0)
            {
                int minutosInicio = timePickerHoraInicio.Value.Value.Hour * 60 +
                                    timePickerHoraInicio.Value.Value.Minute;
                int minutosFim = timePickerHoraFim.Value.Value.Hour * 60 + timePickerHoraFim.Value.Value.Minute;

                if (minutosFim <= minutosInicio && datePInicio.SelectedDate == datePFim.SelectedDate)
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = Application.Current.FindResource("HorasTitle") as string;
                    mg.Content = Application.Current.FindResource("HorasContentFimMenorIgualInicio") as string;
                    mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                    mg.CloseButtonAppearance = ControlAppearance.Info;
                    mg.ShowDialogAsync();
                    timePickerHoraFim.Value = timePickerHoraInicio.Value.Value.AddMinutes(1);
                }
            }
        }

        private void DatePInicio_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (datePInicio.SelectedDate > datePFim.SelectedDate)
            {
                datePInicio.SelectedDate = datePFim.SelectedDate;
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("DatasTitle") as string;
                mg.Content = Application.Current.FindResource("DatasContentInicioMaiorFim") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }

            ToggleBtnRepetir_OnUnchecked(sender, e);
            ToggleBtnRepetir_OnChecked(sender, e);

            if (datePInicio.SelectedDate == datePFim.SelectedDate)
            {
                TimePickerHoraInicio_OnValueChanged(sender, null);
                TimePickerHoraFim_OnValueChanged(sender, null);
            }
        }

        private void DatePFim_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (datePFim.SelectedDate < datePInicio.SelectedDate)
            {
                datePFim.SelectedDate = datePInicio.SelectedDate;
                datePInicio.SelectedDate = datePFim.SelectedDate;
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("DatasTitle") as string;
                mg.Content = Application.Current.FindResource("DatasContentFimMenorInicio") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }
            ToggleBtnRepetir_OnUnchecked(sender, e);
            ToggleBtnRepetir_OnChecked(sender, e);

            if (datePInicio.SelectedDate == datePFim.SelectedDate)
            {
                TimePickerHoraFim_OnValueChanged(sender, null);
                TimePickerHoraInicio_OnValueChanged(sender, null);
            }
        }

        private void BtnOnUnChecked(object sender, RoutedEventArgs e)
        {
            if (datePInicio.SelectedDate != null && datePFim.SelectedDate != null && datePInicio.SelectedDate != datePFim.SelectedDate) //se for em dias da semana diferentes
            {
                //contamos o numero de botoes checked
                int numBotoesDiasSemanaChecked = 0;
                foreach (ToggleButton child in stackPanelDias.Children)
                {
                    if (child.IsChecked == true)
                        numBotoesDiasSemanaChecked++;
                }

                //se forem 0, nao pode acontecer, entao temos de ter pelo menos um checked que vai ser o dia da semana que comeca a tarefa
                if (numBotoesDiasSemanaChecked == 0)
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = Application.Current.FindResource("EscolheDiaTitle") as string;
                    mg.Content = Application.Current.FindResource("EscolheDiaContent") as string;
                    mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                    mg.CloseButtonAppearance = ControlAppearance.Info;
                    mg.ShowDialogAsync();

                    switch (datePInicio.SelectedDate.Value.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            btnSegunda.IsChecked = true;
                            break;
                        case DayOfWeek.Tuesday:
                            btnTerca.IsChecked = true;
                            break;
                        case DayOfWeek.Wednesday:
                            btnQuarta.IsChecked = true;
                            break;
                        case DayOfWeek.Thursday:
                            btnQuinta.IsChecked = true;
                            break;
                        case DayOfWeek.Friday:
                            btnSexta.IsChecked = true;
                            break;
                        case DayOfWeek.Saturday:
                            btnSabado.IsChecked = true;
                            break;
                        case DayOfWeek.Sunday:
                            btnDomingo.IsChecked = true;
                            break;
                    }
                }
            }
        }

        private void BtnOnChecked(object sender, RoutedEventArgs e)
        {
            bool found = false;

            if (datePInicio.SelectedDate != null && datePFim.SelectedDate != null && datePInicio.SelectedDate != datePFim.SelectedDate)
            {
                DateTime dataInicio = datePInicio.SelectedDate.Value;
                DateTime dataFim = datePFim.SelectedDate.Value;

                // Verificar se o intervalo de datas é menor que uma semana
                if ((dataFim - dataInicio).TotalDays <= 6)
                {
                    // Mapeia os nomes dos botões para os dias da semana correspondentes
                    var buttonToDayOfWeek = new Dictionary<string, DayOfWeek>
                    {
                        { "btnSegunda", DayOfWeek.Monday },
                        { "btnTerca", DayOfWeek.Tuesday },
                        { "btnQuarta", DayOfWeek.Wednesday },
                        { "btnQuinta", DayOfWeek.Thursday },
                        { "btnSexta", DayOfWeek.Friday },
                        { "btnSabado", DayOfWeek.Saturday },
                        { "btnDomingo", DayOfWeek.Sunday }
                    };

                    // Obtém o nome do botão clicado
                    string buttonName = ((ToggleButton)sender).Name;

                    // Verifica se o botão clicado está no dicionário
                    if (buttonToDayOfWeek.ContainsKey(buttonName))
                    {
                        DayOfWeek selectedDay = buttonToDayOfWeek[buttonName];

                        // Iterar por cada dia no intervalo de datas
                        for (DateTime data = dataInicio; data <= dataFim; data = data.AddDays(1))
                        {
                            // Verificar se o dia da semana da data atual é igual ao dia de repetição selecionado
                            if (data.DayOfWeek == selectedDay)
                            {
                                found = true;
                                break; // Não precisa continuar verificando uma vez que encontrou
                            }
                        }
                    }

                    if (!found)
                    {
                        Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox
                        {
                            Title = (string)FindResource("ErrorExistingButtonTitle"),
                            Content = (string)FindResource("DatasIntervaloEscolhidoErro"),
                            CloseButtonText = (string)FindResource("AlertasCloseButtonText"),
                            CloseButtonAppearance = ControlAppearance.Info
                        };
                        mg.ShowDialogAsync();

                        ((ToggleButton)sender).IsChecked = false;
                    }
                }
            }
        }



        private void EstadoToggleBtnRepetir_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleBtnRepetir_OnUnchecked(sender, e);
            ToggleBtnRepetir_OnChecked(sender, e);
        }
        private void TxtbTempoParaAlertaAntecipacao_OnLostFocus(object sender, RoutedEventArgs e)
        {

            if (txtbTempoParaAlertaAntecipacao.Text.Length == 0)
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("ErroTempoAntecipacaoTitle") as string;
                mg.Content = Application.Current.FindResource("ErroTempoAntecipacaoContentInteiro") as string;
                mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();

                txtbTempoParaAlertaAntecipacao.Text = "5";
            }
            else
            {
                // Substituir ponto por vírgula
                string textoConvertido = txtbTempoParaAlertaAntecipacao.Text.Replace('.', ',');

                if (!double.TryParse(textoConvertido, out double valor))
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = Application.Current.FindResource("ErroTempoAntecipacaoTitle") as string;
                    mg.Content = Application.Current.FindResource("ErroTempoAntecipacaoContentInteiro") as string;
                    mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                    mg.CloseButtonAppearance = ControlAppearance.Info;
                    mg.ShowDialogAsync();

                    txtbTempoParaAlertaAntecipacao.Text = "5";
                }
                else if (valor < 0)
                {
                    Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                    mg.Title = Application.Current.FindResource("ErroTempoAntecipacaoTitle") as string;
                    mg.Content = Application.Current.FindResource("ErroTempoAntecipacaoContentPositivo") as string;
                    mg.CloseButtonText = Application.Current.FindResource("AlertasCloseButtonText") as string;
                    mg.CloseButtonAppearance = ControlAppearance.Info;
                    mg.ShowDialogAsync();

                    txtbTempoParaAlertaAntecipacao.Text = "5";
                }
                else
                {
                    txtbTempoParaAlertaAntecipacao.Text = textoConvertido;
                }
            }

        }


        private void ComboBPrioridades_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toggleBtnAlertaAntecipacao != null && toggleBtnAlertaNaoRealizacao != null && comboBPrioridades.SelectedIndex == 3)
            {
                toggleBtnAlertaAntecipacao.IsChecked = true;
                toggleBtnAlertaAntecipacao.IsEnabled = false;
                toggleBtnAlertaNaoRealizacao.IsChecked = true;
                toggleBtnAlertaNaoRealizacao.IsEnabled = false;
            }
            else if (toggleBtnAlertaAntecipacao != null && toggleBtnAlertaNaoRealizacao != null) //se nao for selecionada a prioritaria
            {
                toggleBtnAlertaAntecipacao.IsEnabled = true;
                toggleBtnAlertaNaoRealizacao.IsEnabled = true;
            }

        }

        private void TxtbDescricao_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtbDescricao.Text == (string)FindResource("adicionarTarefaDescricaoText"))
                txtbDescricao.Text = "";
        }

        private void TxtbDescricao_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtbDescricao.Text == "")
                txtbDescricao.Text = (string)FindResource("adicionarTarefaDescricaoText");
        }

        private void TxtbTitulo_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtbTitulo.Text == (string)FindResource("adicionarTarefaTItuloText"))
                txtbTitulo.Text = "";
        }

        private void TxtbTitulo_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtbTitulo.Text == "")
            {
                txtbTitulo.Text = (string)FindResource("adicionarTarefaTItuloText");
            }
        }

        private void TimePickerHoraInicio_OnLoaded(object sender, RoutedEventArgs e)
        {
            var timePicker = sender as Xceed.Wpf.Toolkit.TimePicker;
            if (timePicker != null)
            {
                // Definindo a cultura como InvariantCulture para usar o formato de 24 horas
                timePicker.CultureInfo = CultureInfo.InvariantCulture;

                // Ou definindo explicitamente o formato
                timePicker.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
                timePicker.FormatString = "HH:mm";
            }
        }

        private void TimePickerHoraFim_OnLoaded(object sender, RoutedEventArgs e)
        {
            var timePicker = sender as Xceed.Wpf.Toolkit.TimePicker;
            if (timePicker != null)
            {
                // Definindo a cultura como InvariantCulture para usar o formato de 24 horas
                timePicker.CultureInfo = CultureInfo.InvariantCulture;

                // Ou definindo explicitamente o formato
                timePicker.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
                timePicker.FormatString = "HH:mm";
            }
        }
    }
}

