using System.Configuration;
using System.Data;
using System.Windows;
using Projeto.Models;
using Projeto.Views;
using Wpf.Ui.Markup;


namespace Projeto;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ModelDados model { get; private set; }
        public PerfilView viewPerfil { get;  set; }
        public AdicionarTarefa ViewAdicionarTarefa { get;  set; }
        public EditarTarefa ViewEditarTarefa { get; set; }
        public EditarAlerta ViewEditarAlerta { get; set; }
    public App()
        {
                //instanciar os models
                model = new ModelDados();
                //instanciar as views
                //viewPerfil = new PerfilView();
                //ViewEditarTarefa = new EditarTarefa();
                //ViewAdicionarTarefa = new AdicionarTarefa();

    }
}


