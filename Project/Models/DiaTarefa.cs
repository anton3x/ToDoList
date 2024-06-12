using System.Xml.Linq;

namespace Projeto.Models
{
    public class DiaTarefa
    {
        public int Id { get; private set; }
        public DateOnly Data { get; set; }
        public bool ativo { get; set; }

        public DiaTarefa(int id, DateOnly data, bool ativo)
        {
            Id = id;
            Data = data;
            this.ativo = ativo;
        }
        public XElement ToXML()
        {
            return new XElement("diaTarefa",
                new XElement("data", Data.ToString("yyyy-MM-dd")),
                new XElement("ativo", ativo)
            );
        }
    }
    public class DiaTarefaComparer : IEqualityComparer<DiaTarefa>
    {
        public bool Equals(DiaTarefa x, DiaTarefa y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Data == y.Data;
        }

        public int GetHashCode(DiaTarefa obj)
        {
            return obj.Data.GetHashCode();
        }
    }
}
