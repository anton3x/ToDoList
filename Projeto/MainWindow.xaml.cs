using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using Projeto.Models;
using Projeto.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Appearance;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;
using System.Windows.Forms.VisualStyles;
using Xceed.Wpf.AvalonDock.Themes;
using Xceed.Wpf.AvalonDock.Properties;
using System.Globalization;
using MenuItem = Wpf.Ui.Controls.MenuItem;
using System.Windows.Data;
using Microsoft.Data.SqlClient;
using MessageBox = System.Windows.MessageBox;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        DateTime scheduledTime; // Ano, Mês, Dia, Hora, Minuto, Segundo -> new DateTime(2024, 5, 5, 12, 49, 00);


        private App app;

        public MainWindow()
        {
            InitializeComponent();

            app = App.Current as App;
            app.model.AtualizacaoPerfilFeita += AtualizarDados;
            app.model.ListaAlertasAlterada += AtualizarScheduledTime;
            app.model.ListaNavigationViewItemsAtualizada += AtualizarListaNavigationViewItems;
            app.model.ListaNavigationViewItemRemovido += AtualizarCCAposDeleteDoBotao;
            app.model.LoadBotoesFeito += ModelOnLoadBotoesFeito;
            app.model.ModoEscuroLoad += ModelEscuroCarregar;
            app.model.ModoClaroLoad += ModelClaroCarregar;
            app.model.LinguagemAlterada += AlterarLingua;
            app.model.TarefaEliminada += AtualizarScheduledTime;
            app.model.TarefaAdicionada += AtualizarScheduledTime;
            app.model.TarefaEditada += AtualizarScheduledTime;

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Extrai os argumentos do evento
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Executa ações baseadas no argumento recebido
                if (args.TryGetValue("action", out string action))
                {
                    switch (action)
                    {
                        case "abrirApp":
                            this.Dispatcher.Invoke(AbrirApp);
                            break;

                        case "finalizarTarefa":
                            if (args.TryGetValue("tarefaId", out string tarefaId) &&
                                args.TryGetValue("dataExecucao", out string dataExecucao))
                            {
                                this.Dispatcher.Invoke(() => FinalizarTarefa(tarefaId, dataExecucao));
                            }
                            break;

                        default:
                            break;
                    }
                }
            };
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt");
        }

        private void AlterarLingua()
        {
            SetLang(app.model.linguaApp);
        }
        private void FinalizarTarefa(string tarefaId, string dataExeucao)
        {
            app.model.FinalizarTarefaDia(tarefaId, dataExeucao); // Se necessário, salvar as alterações no modelo
        }

        private void ModelClaroCarregar()
        {
            LightThemeClick(this, null);
        }
        private void ModelEscuroCarregar()
        {
            DarkThemeClick(this, null);
        }

        private void ModelOnLoadBotoesFeito()
        {

            foreach (var item in app.model.ListaItemDatasLoad)
            {
                // Criar e adicionar o novo NavigationViewItem
                var newItem = new NavigationViewItem
                {
                    Content = item.Name,
                    FontWeight = FontWeights.Bold,
                    Tag = item
                };
                var iconSource = Application.Current.Resources["IconNovaTarefa"] as BitmapImage;
                var icon = new ImageIcon()
                {
                    Source = iconSource,
                    Width = 30,
                    Height = 30
                };
                newItem.Icon = icon;

                // Configurar e adicionar tooltip
                var toolTip = new ToolTip { Content = newItem.Content.ToString() };
                ToolTipService.SetToolTip(newItem, toolTip);

                RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.Fant);

                newItem.Click += ClickBotaoComListaPersonalizado;

                RootNavigation.MenuItems.Add(newItem);
                app.model.AdicionarNavigationViewItem(newItem); // Adicionar ao modelo da aplicação
            }

            // Eventos para atualizar o painel
            if (RootNavigation.IsPaneOpen)
                RootNavigation_PaneOpened(RootNavigation, null);
            else
                RootNavigation_PaneClosed(RootNavigation, null);

        }

        //apos eliminar o botao, atualizo o conteudo para exibir as tarefas em vez do botao antigo
        private void AtualizarCCAposDeleteDoBotao()
        {
            btnTarefasTotais_Click(this, null);
        }

        private void AbrirApp()
        {
            if(!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(AbrirApp);
                return;
            }

            this.Show();
            // Restaura o estado da janela se estiver minimizada
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            // Traz a janela para o foco
            this.Activate();
        }


        // Atualiza a lista de itens do NavigationView com base na lista de itens do modelo
        private void AtualizarListaNavigationViewItems()
        {
            // Lista para armazenar itens a serem removidos
            List<NavigationViewItem> itemsToRemove = new List<NavigationViewItem>();

            // Itera sobre os itens do menu e adiciona à lista de remoção se o Tag for do tipo ItemData e o item não for um separador
            foreach (var menuItem in RootNavigation.MenuItems)
            {
                if (menuItem is NavigationViewItem item && item.Tag is ItemData)
                {
                    itemsToRemove.Add(item);
                }
            }

            // Remove os itens fora do loop de iteração original
            foreach (var item in itemsToRemove)
            {
                RootNavigation.MenuItems.Remove(item);
            }

            // Adiciona novos itens do app.model.listaItensMenu
            foreach (var item in app.model.listaItensMenu)
            {
                RootNavigation.MenuItems.Add(item);
            }
        }


        private void AtualizarScheduledTime()
        {
            if (app.model.listaAlertas.Count != 0)
                scheduledTime = app.model.listaAlertas.First().Data_Hora;
            else
            {
                scheduledTime = DateTime.MaxValue; //para o alerta anterior nao se repetir
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            MyNotifyIcon.Dispose();
            base.OnClosed(e);
        }

        private void AtualizarDados()
        {
            nomePerfil.Text = app.model.perfil.Nome;
            imagePerfil.Source = app.model.perfil.Fotografia.Source;
            emailPerfil.Text = app.model.perfil.Email;
        }

        private void NavView_PaneOpened(NavigationView sender, object args)
        {
            Storyboard expandStoryboard = (Storyboard)imagePerfil.Resources["ExpandAnimation"];
            expandStoryboard.Begin();
        }

        private void NavView_PaneClosed(NavigationView sender, object args)
        {
            Storyboard collapseStoryboard = (Storyboard)imagePerfil.Resources["CollapseAnimation"];
            collapseStoryboard.Begin();
        }

        private void btnTarefasTotais_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefasTodas();
            CC.Content = new ExibirListaTarefas((string)FindResource("buttonTarefasName"),
                Application.Current.Resources["IconTarefas"] as BitmapImage);
        }

        private void btnTarefasHoje_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefasHoje();
            CC.Content = new ExibirListaTarefas((string)FindResource("btnTarefasHojeName"),
                Application.Current.Resources["IconHoje"] as BitmapImage);
        }

        private void btnTarefasEstaSemana_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefas(DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today).AddDays(6));
            CC.Content =
                new ExibirListaTarefas((string)FindResource("btnTarefasEstaSemanaName"),
                    Application.Current.Resources["IconSemana"] as BitmapImage);
        }

        private void btnTarefasPoucoImportantes_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefas("Pouco Importante");
            CC.Content = new ExibirListaTarefas((string)FindResource("btnTarefasPoucoImportanteName"),
                Application.Current.Resources["IconPoucoImportante"] as BitmapImage);
        }

        private void btnTarefasNormais_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefas("Normal");
            CC.Content = new ExibirListaTarefas((string)FindResource("btnTarefasNormalName"),
                Application.Current.Resources["IconNormal"] as BitmapImage);
        }

        private void btnTarefasImportantes_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefas("Importante");
            CC.Content = new ExibirListaTarefas((string)FindResource("btnTarefasImportanteName"),
                Application.Current.Resources["IconImportante"] as BitmapImage);
        }

        private void btnTarefasPrioritarias_Click(object sender, RoutedEventArgs e)
        {
            app.model.FiltrarTarefas("Prioritaria");
            CC.Content = new ExibirListaTarefas((string)FindResource("btnTarefasPrioritariaName"),
                Application.Current.Resources["IconPrioritaria"] as BitmapImage);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            // Verifica se o momento atual é próximo ao horário agendado para enviar o e-mail e/ou notificação
            if (DateTime.Now >= scheduledTime)
            {
                //se existir algum alerta na lista de alertas
                if (app.model.listaAlertas.Count != 0)
                {
                    if (app.model.listaAlertas.First().Tipos.Contains(TipoAlerta.Email) && app.model.listaAlertas.First().Desligado == false) //vecrifica se e do tipo email
                        app.model.EmailSender();

                    if (app.model.listaAlertas.First().Tipos.Contains(TipoAlerta.AlertaWindows) && app.model.listaAlertas.First().Desligado == false) //verifica se e do tipo windows
                    {
                        VerificarAtivacaoNotificacoesWindows();
                        if (app.model.listaAlertas.First().Mensagem == "Alerta de Nao Realizacao")
                            app.model.Notificacao(app.model.listaAlertas.First(), true);
                        else
                            app.model.Notificacao(app.model.listaAlertas.First(), false);
                    }

                    //para eliminar o alerta ele tem que ter ocorrido, ou seja, se ele tiver as notificacoes windows ativadas é porque apareceu a notificacao e elimina-a
                    //e se for do tipo email, nao precisa de ter as notificacoes windows ativadas, entao elimina
                    if (app.model.AreToastNotificationsEnabled() == true || (app.model.listaAlertas.First().Tipos.Count == 1 && app.model.listaAlertas.First().Tipos.Contains(TipoAlerta.Email)))
                    {
                        app.model.EliminarAlerta(app.model.listaAlertas.First()); //elimina e atualiza o scheduledTime
                    }
                }
            }
        }


        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            // Oculta a janela
            this.Hide();
            //alterar pasta para guardar o ficheiro
            //app.model.SaveToXML("dados.xml"); // armazena do dados em ficheir XML quando fecha a aplicação

            //ToastNotificationManagerCompat.History.Clear(); //limpar as notificacoes relativas á nossa app
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            app.model.LoadFromBD("dados.xml"); //le ja no ficheiro
            AtualizarDados();
            app.model.FiltrarTarefasTodas();

            btnTarefasTotais_Click(this, null); //comecar a apresentar as tarefas todas
            timer = new DispatcherTimer();
            if (app.model.listaAlertas.Count != 0)
                scheduledTime = app.model.listaAlertas.First().Data_Hora;

            VerificarAtivacaoNotificacoesWindows();

            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();
        }

        //responsavel por verificar se as notificacoes windows estao ativadas
        private async void VerificarAtivacaoNotificacoesWindows()
        {
            if (app.model.AreToastNotificationsEnabled() == false) //se nao tiver as notificacoes windows ativadas
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("WindowsNotificationsTitle") as string;
                mg.Content = Application.Current.FindResource("WindowsNotificationsContent");
                mg.PrimaryButtonText = Application.Current.FindResource("WindowsNotificationsPrimaryButtonText") as string;
                mg.PrimaryButtonAppearance = ControlAppearance.Primary;
                mg.CloseButtonText = Application.Current.FindResource("WindowsNotificationsCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Secondary;

                MessageBoxResult result = await mg.ShowDialogAsync();

                if (result == MessageBoxResult.Primary) //escolheu o abrir configuracoes
                {
                    Process.Start(new ProcessStartInfo("ms-settings:notifications") { UseShellExecute = true }); //abrir notificacoes windows, nas definicoes
                }
            }
        }


        private void RootNavigation_PaneOpened(Wpf.Ui.Controls.NavigationView sender, object args)
        {
            // Quando o NavigationView é expandido, aumenta o tamanho da imagem
            imagePerfil.Width = 80;
            imagePerfil.Height = 80;

            //para nao exibir os tooltip quando esta expandida
            foreach (var item in RootNavigation.MenuItems)
            {
                if (item is NavigationViewItem navigationViewItem)
                {
                    var toolTip = ToolTipService.GetToolTip(navigationViewItem) as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void RootNavigation_PaneClosed(Wpf.Ui.Controls.NavigationView sender, object args)
        {

            // Quando o NavigationView é colapsado, diminui o tamanho da imagem
            imagePerfil.Width = 40;
            imagePerfil.Height = 40;

            //para exibir os tooltip quando esta colapsada
            foreach (var item in RootNavigation.MenuItems)
            {
                if (item is NavigationViewItem navigationViewItem)
                {
                    var toolTip = ToolTipService.GetToolTip(navigationViewItem) as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void BtnPerfilMain_OnClick(object sender, RoutedEventArgs e)
        {
            app.viewPerfil = new PerfilView();
            app.viewPerfil.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            app.viewPerfil.ShowDialog();
            switch (app.viewPerfil.languageComboBox.SelectedIndex)
            {
                case 0:
                    app.model.AtualizarLinguagem("pt-PT");
                    break;
                case 1:
                    app.model.AtualizarLinguagem("en-US");
                    break;
                case 2:
                    app.model.AtualizarLinguagem("es-ES");
                    break;
                case 3:
                    app.model.AtualizarLinguagem("fr-FR");
                    break;
                case 4:
                    app.model.AtualizarLinguagem("de-DE");
                    break;
                case 5:
                    app.model.AtualizarLinguagem("it-IT");
                    break;
            }
        }

        //responsavel pelo que o click no botao personalizado faz
        private void ClickBotaoComListaPersonalizado(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as NavigationViewItem;
            if (clickedItem != null)
            {
                var itemData = clickedItem.Tag as ItemData;
                if (itemData != null)
                {
                    CC.Content = new ExibirListaTarefasComBotaoPersonalizado( clickedItem.Content.ToString());
                }
            }
        }

        //funcao responsavel por criar o botao e adiciona-lo ao menuitems da mainwindow
        private void AdicionarBotaoComListaPersonalizado(string nomeBotao = "")
        {
            string nome = nomeBotao.Trim();

            // se o nome do botao for nulo ou espaco em branco
            if (!string.IsNullOrWhiteSpace(nomeBotao.Trim()) || nomeBotao == "")
            {
                RootNavigation.Focus(); //remove o foco da textbox

                if (nomeBotao == "")
                {
                    int indice = 1;
                    bool nomeEncontrado;

                    do
                    {
                        nomeEncontrado = false;
                        nome = "Lista" + indice;

                        foreach (var item in RootNavigation.MenuItems)
                        {
                            if (item is NavigationViewItem navigationViewItem)
                            {
                                if (navigationViewItem.Content.ToString() == nome)
                                {
                                    nomeEncontrado = true;
                                    indice++;
                                    break;
                                }
                            }
                        }
                    } while (nomeEncontrado);
                }
                else
                {
                    // Verificar se já existe um botão com esse nome
                    foreach (var item in RootNavigation.MenuItems)
                    {
                        if (item is NavigationViewItem navigationViewItem)
                        {
                            if (navigationViewItem.Content.ToString() == nome)
                            {
                                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                                mg.Title = Application.Current.FindResource("ErrorExistingButtonTitle") as string;
                                mg.Content = Application.Current.FindResource("ErrorExistingButtonContent");
                                mg.CloseButtonText = Application.Current.FindResource("ErrorExistingButtonCloseButtonText") as string;
                                mg.CloseButtonAppearance = ControlAppearance.Info;
                                mg.ShowDialogAsync();
                                return;
                            }
                        }
                    }
                }

                // Criar e adicionar o novo NavigationViewItem
                var newItem = new NavigationViewItem
                {
                    Content = nome,
                    FontWeight = FontWeights.Bold,
                    Tag = new ItemData(app.model.GetNextDiaTarefaId())
                };

                ((ItemData)newItem.Tag).Name = newItem.Content.ToString();

                // Adicionar ícone
                var iconSource = Application.Current.Resources["IconNovaTarefa"] as BitmapImage;
                var icon = new ImageIcon()
                {
                    Source = iconSource,
                    Width = 30,
                    Height = 30
                };
                // Adicione o ícone ao controle desejado, por exemplo, a um MenuItem
                // Example: myMenuItem.Icon = icon;


                newItem.Icon = icon;

                // Configurar e adicionar tooltip
                var toolTip = new ToolTip { Content = newItem.Content.ToString() };
                ToolTipService.SetToolTip(newItem, toolTip);

                RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.Fant);

                newItem.Click += ClickBotaoComListaPersonalizado;

                RootNavigation.MenuItems.Add(newItem);
                app.model.AdicionarNavigationViewItem(newItem); // Adicionar ao modelo da aplicação

                // Eventos para atualizar o painel
                if (RootNavigation.IsPaneOpen)
                    RootNavigation_PaneOpened(RootNavigation, null);
                else
                    RootNavigation_PaneClosed(RootNavigation, null);
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = Application.Current.FindResource("ErrorButtonNameNotAllowedTitle") as string;
                mg.Content = Application.Current.FindResource("ErrorButtonNameNotAllowedContent");
                mg.CloseButtonText = Application.Current.FindResource("ErrorButtonNameNotAllowedCloseButtonText") as string;
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
                RootNavigation.Focus(); //remove o foco da textbox
            }
        }

        //reponsavel por adicionar o botao sem o texto personalizado, usando o botao circular
        private void BtnAdicionarListaSemTextoPersonalizado_OnClick(object sender, RoutedEventArgs e)
        {
            AdicionarBotaoComListaPersonalizado();
        }

        //responsavel por adicionar o botao com o texto personalizado
        private void TxtbAdicionarLista_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AdicionarBotaoComListaPersonalizado(txtAdicionarLista.Text);
            }
        }

        //repoem o texto na textbox do panefooter
        private void TxtAdicionarLista_OnLostFocus(object sender, RoutedEventArgs e)
        {
            // Acessa o valor do ResourceDictionary
            string newListText = (string)FindResource("textBoxAddListaPersonalizadaText");

            // Define o texto do TextBox com o valor do ResourceDictionary
            txtAdicionarLista.Text = newListText;
        }
        //remove o texto na textbox do panefooter
        private void TxtAdicionarLista_OnGotFocus(object sender, RoutedEventArgs e)
        {
            txtAdicionarLista.Text = "";
        }

        private void MenuItem_Show_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            // Restaura o estado da janela se estiver minimizada
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            // Traz a janela para o foco
            this.Activate();

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Resources["ModoCor"] is SolidColorBrush brush && brush.Color == Colors.Black)
            {
                app.model.SaveToXML("dados.xml", false); // armazena do dados em ficheir XML quando fecha a aplicação
            }
            else
            {
                app.model.SaveToXML("dados.xml", true); // armazena do dados em ficheir XML quando fecha a aplicação
            }

            ToastNotificationManagerCompat.History.Clear(); //limpar as notificacoes relativas á nossa app
            Application.Current.Shutdown();
        }
        private void LightThemeClick(object sender, RoutedEventArgs e)
        {
            ModosClaroEscuro.AlterarModo(new Uri("Themes/Light.xaml", UriKind.Relative));
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(ApplicationTheme.Light);
            UpdateIconsForAllItems();
        }

        private void DarkThemeClick(object sender, RoutedEventArgs e)
        {
            ModosClaroEscuro.AlterarModo(new Uri("Themes/Dark.xaml", UriKind.Relative));
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            UpdateIconsForAllItems();
        }
        private void UpdateIconForItem(NavigationViewItem item, string resourceKey)
        {
            var iconSource = Application.Current.Resources[resourceKey] as BitmapImage;
            if (iconSource != null)
            {
                var icon = new ImageIcon()
                {
                    Source = iconSource,
                    Width = 30,
                    Height = 30
                };
                RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.Fant);
                item.Icon = icon;
            }
        }

        private void UpdateIconsForAllItems()
        {
            foreach (var menuItem in RootNavigation.MenuItems)
            {
                if (menuItem is NavigationViewItem item && item.Tag is ItemData)
                {
                    UpdateIconForItem(item, "IconNovaTarefa");
                }
            }

            if(CC.Content != null)
            {
                if ((CC.Content as ExibirListaTarefas) != null)
                {
                    string currentTask = ((ExibirListaTarefas)CC.Content).txtbTarefas.Text;

                    // Comparar com as strings de recursos
                    if (currentTask == (string)FindResource("buttonTarefasName")) //alterar aqui
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconTarefas"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasHojeName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconHoje"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasEstaSemanaName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconSemana"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasPoucoImportanteName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconPoucoImportante"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasNormalName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconNormal"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasImportanteName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconImportante"] as BitmapImage);
                    }
                    else if (currentTask == (string)FindResource("btnTarefasPrioritariaName"))
                    {
                        CC.Content = new ExibirListaTarefas(currentTask,
                            Application.Current.Resources["IconPrioritaria"] as BitmapImage);
                    }
                }
                else
                {
                    CC.Content =
                        new ExibirListaTarefasComBotaoPersonalizado(
                            ((ExibirListaTarefasComBotaoPersonalizado)CC.Content).txtbTarefas.Text);
                }
            }
        }

        private void SetLang(string lang)
        {
            ResourceDictionary resdir = new ResourceDictionary()
            {
                Source = new Uri($"Dictionary-{lang}.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(resdir);
            CC.Content = new ExibirListaTarefas((string)FindResource("buttonTarefasName"), Application.Current.Resources["IconTarefas"] as BitmapImage);
            MenuItemAbrir.Header = (string)FindResource("menuItemAbrir");
            MenuItemFechar.Header = (string)FindResource("menuItemSair1");
            MenuItemModoEscuro.Header = (string)FindResource("menuItemModoEscuro");
            MenuItemModoClaro.Header = (string)FindResource("menuItemModoClaro");
            txtAdicionarLista.Text = (string)FindResource("textBoxAddListaPersonalizadaText");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }
    }

}
