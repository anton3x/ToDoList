using System.Windows;

namespace Projeto
{
    /// <summary>
    /// Lógica interna para TarefasChecked.xaml
    /// </summary>
    public partial class TarefasChecked : Window
    {
        private App app;
        public TarefasChecked()
        {
            InitializeComponent();
            app = App.Current as App;
            this.DataContext = app.model;

        }

        private void Button_Click_Voltar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}