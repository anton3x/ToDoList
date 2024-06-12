using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Projeto.Models
{
    // Enum para representar o tipo de periodicidade
    public enum TipoPeriodicidade
    {
        Diaria,
        Semanal
    }
    public class Periodicidade
    {
        public int Id { get; set; }
        public TipoPeriodicidade Tipo { get; set; } // Diária ou semanal
        public List<DayOfWeek> DiasSemana { get; set; }

        public Periodicidade(int id)
        {
            Id = id;
            DiasSemana = new List<DayOfWeek>();
        }
        public Periodicidade(TipoPeriodicidade tipo, List<DayOfWeek> diasSemana, int id)
        {
            Id = id;
            Tipo = tipo;
            DiasSemana = diasSemana;
        }
        public override string ToString()
        {
            return $"Id: {Id}, Tipo: {Tipo}, Dias da Semana: {string.Join(", ", DiasSemana)}";
        }
        public XElement ToXML()
        {
            XElement periodicidadeElement = new XElement("periodicidade",
                new XElement("id", Id),
                new XElement("tipo", Tipo),
                new XElement("diasSemana", string.Join(",", DiasSemana))
            );

            return periodicidadeElement;
        }
        public static List<DayOfWeek> ParseDiasSemana(string diasSemana)
        {
            if (String.IsNullOrEmpty(diasSemana))
            {
                return new List<DayOfWeek>();
            }
            else
                return diasSemana.Split(',')
                    .Select(d => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), d))
                    .ToList();
        }
    }
}
