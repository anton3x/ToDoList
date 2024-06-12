using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Projeto.Models;
using Wpf.Ui.Controls;

namespace Projeto
{
    /// <summary>
    /// Interação lógica para DiaTarefaDesign.xam
    /// </summary>
    public partial class DiaTarefaDesign : UserControl
    {
        private App app;


        public DiaTarefaDesign()
        {
            InitializeComponent();
            app = App.Current as App;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if(((DiaTarefa)this.DataContext).Data >= DateOnly.FromDateTime(DateTime.Today) || ((DiaTarefa)this.DataContext).ativo == true)
            {
                AlteracaoEstadoDiaTarefa();
            }
            else
            {
                Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
                mg.Title = "Erro";
                mg.Content = "Não podes alterar dias anterior ao de hoje!!!";
                mg.CloseButtonText = "OK";
                mg.CloseButtonAppearance = ControlAppearance.Info;
                mg.ShowDialogAsync();
            }
        }

        public async void AlteracaoEstadoDiaTarefa()
        {
            UserControl_Loaded(this, null);
            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
            mg.Title = "Alteracoes";
            mg.Content = "Queres alterar?";
            mg.PrimaryButtonText = "Sim";
            mg.PrimaryButtonAppearance = ControlAppearance.Primary;
            mg.CloseButtonText = "Não";
            mg.CloseButtonAppearance = ControlAppearance.Secondary;

            Wpf.Ui.Controls.MessageBoxResult resposta = await mg.ShowDialogAsync();

            if (resposta == Wpf.Ui.Controls.MessageBoxResult.Primary) //escolheu o abrir configuracoes
            {
                app.model.EditarDiaTarefas((DiaTarefa)this.DataContext, !((DiaTarefa)this.DataContext).ativo);
                UserControl_Loaded(this, null);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (((DiaTarefa)this.DataContext).ativo == false)
                btnRadio.IsChecked = true;
            else
                btnRadio.IsChecked = false;
        }
    }
}