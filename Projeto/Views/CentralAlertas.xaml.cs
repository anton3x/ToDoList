using System.Windows;
using Projeto.Models;

namespace Projeto
{
    /// <summary>
    /// Interaction logic for CentralAlertas.xaml
    /// </summary>
    public partial class CentralAlertas : Window
    {
        private App app;
        public CentralAlertas(Tarefa tarefa)
        {
            this.DataContext = tarefa;
            InitializeComponent();
            app = App.Current as App;

        }

        private void Button_Click_Voltar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}