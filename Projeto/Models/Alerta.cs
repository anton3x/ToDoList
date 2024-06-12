using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace Projeto.Models
{
    // Enum para representar o tipo de alerta
    public enum TipoAlerta
    {
        Email,
        AlertaWindows,
    }

    public class Alerta
    {
        public int Id { get; set; }
        public string Mensagem { get; set; }
        public DateTime Data_Hora { get; set; }
        public List<TipoAlerta> Tipos { get; set; }
        public bool Desligado { get; set; }

        public Alerta(int id)
        {
            Id = id;
            Tipos = new List<TipoAlerta>();
        }

        public Alerta(string mensagem, DateTime dataHora, List<TipoAlerta> tipos, bool desligado, int id)
        {
            Id = id;
            Mensagem = mensagem;
            Data_Hora = dataHora;
            Tipos = tipos;
            Desligado = desligado;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Mensagem: {Mensagem}, Data e Hora: {Data_Hora}, Tipos: {Tipos}, Desligado: {Desligado}";
        }

        public XElement ToXML() //converter para XML
        {
            XElement alertaElement = new XElement("alerta",
                new XAttribute("id", Id),
                new XAttribute("mensagem", Mensagem),
                new XAttribute("data_Hora", Data_Hora.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement("tipos", Tipos.ConvertAll(t => new XElement("tipo", t))),
                new XAttribute("desligado", Desligado)
            );

            return alertaElement;
        }
    }

    public class AlertaComparer : IComparer<Alerta>
    {
        public int Compare(Alerta x, Alerta y)
        {
            return x.Data_Hora.CompareTo(y.Data_Hora);
        }
    }

    public class TiposAlertaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<TipoAlerta> tipos)
            {
                if (tipos.Contains(TipoAlerta.Email) && tipos.Contains(TipoAlerta.AlertaWindows))
                {
                    return " <> Email - Windows";
                }
                else if (tipos.Contains(TipoAlerta.Email))
                {
                    return " <> Email";
                }
                else if (tipos.Contains(TipoAlerta.AlertaWindows))
                {
                    return " <> Windows";
                }
            }

            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
