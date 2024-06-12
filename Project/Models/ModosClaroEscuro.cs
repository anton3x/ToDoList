using System.Windows;

namespace Projeto.Models
{
    public class ModosClaroEscuro
    {
        public static void AlterarModo(Uri uriModo)
        {
            ResourceDictionary Modo = new ResourceDictionary() { Source = uriModo };

            App.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(Modo);

        }
    }
}
