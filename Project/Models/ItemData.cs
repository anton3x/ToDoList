using System.Xml.Linq;

namespace Projeto.Models
{
    public class ItemData
    {
        public List<int> IdsTarefas { get; set; }
        public int IdNavigationItem { get; set; }
        public string Name { get; set; }

        public ItemData(int id)
        {
            IdsTarefas = new List<int>();
            IdNavigationItem = id;
        }

        public XElement ToXML()
        {
            return new XElement("NavigationViewItemData",
                new XElement("Name", Name),
                new XElement("Numbers", IdsTarefas.Select(n => new XElement("Number", n)))
            );
        }
    }
}
