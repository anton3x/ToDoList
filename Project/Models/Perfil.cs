using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Projeto.Models
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public Image Fotografia { get; set; }
        public string PathToFotografia { get; set; }

        public Perfil(int id, string nome, string email, string pathToFotografia)
        {
            Id = id;
            Nome = nome;
            Email = email;
            PathToFotografia = pathToFotografia;
            Fotografia = new Image();

            Uri fileUri = new Uri(PathToFotografia, UriKind.RelativeOrAbsolute);
            Fotografia.Source = new BitmapImage(fileUri);
        }

        public Perfil()
        {
            
        }
        public XElement ToXML()
        {
            XElement perfilElement = new XElement("perfil",
                new XElement("nome", Nome),
                new XElement("email", Email)
            );

            return perfilElement;
        }

    }
}