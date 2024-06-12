using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Projeto.Models
{
    // Enum para representar o estado das tarefas
    public enum Estado
    {
        PorComecar,
        EmExecucao,
        PorTerminar
    }

    // Enum para representar o nível de importância das tarefas
    public enum Importancia
    {
        PoucoImportante,
        Normal,
        Importante,
        Prioritaria
    }

    public class Tarefa
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFim { get; set; }
        public Importancia NivelImportancia { get; set; }
        public Estado Estado { get; set; }
        public Periodicidade Periodicidade { get; set; }

        public ObservableCollection<Alerta> AlertaAntecipacao { get; set; }
        public ObservableCollection<Alerta> AlertaExecucao { get; set; }
        public ObservableCollection<DiaTarefa> listaDiaTarefas { get; set; }

        private ObservableCollection<Alerta> alertasTodos;
        public ObservableCollection<Alerta> AlertasTodos
        {
            get { return alertasTodos; }
        }

        private int nivelImportanciaInt; //ver como tirar isto depois, fazendo diretamente com a var "NivelImportancia"
        public int NivelImportanciaInt
        {
            get
            {
                return (int)NivelImportancia;

            }
            set
            {
                NivelImportancia = (Importancia)value;
            }
        }

        public Tarefa(int ID)
        {
            Id = ID;
            AlertaAntecipacao = new ObservableCollection<Alerta>();
            AlertaExecucao = new ObservableCollection<Alerta>();
            alertasTodos = new ObservableCollection<Alerta>();
            listaDiaTarefas = new ObservableCollection<DiaTarefa>();

            AtualizarAlertasTodos();
        }
        public Tarefa(string titulo, string descricao, DateOnly dataInicio, DateOnly dataFim,
            Importancia nivelImportancia, Estado estado, Periodicidade periodicidade, ObservableCollection<Alerta> alertasAntecipacao,
            ObservableCollection<Alerta> alertasExecucao, TimeOnly? horaInicio = null, TimeOnly? horaFim = null, int _id = -1)
        {
            Id = _id;

            Titulo = titulo;
            Descricao = descricao;
            NivelImportancia = nivelImportancia;
            Estado = estado;
            Periodicidade = periodicidade;
            AlertaAntecipacao = alertasAntecipacao;
            AlertaExecucao = alertasExecucao;
            alertasTodos = new ObservableCollection<Alerta>();
            listaDiaTarefas = new ObservableCollection<DiaTarefa>();

            HoraInicio = horaInicio ?? TimeOnly.Parse("00:00 AM");
            HoraFim = horaFim ?? TimeOnly.Parse("00:00 AM");

            // Se a horaInicio for nula, use a meia-noite, caso contrário, use a hora especificada
            DataInicio = horaInicio != null ? dataInicio.ToDateTime(TimeOnly.Parse(horaInicio.ToString())) : dataInicio.ToDateTime(TimeOnly.Parse("00:00 AM"));

            // Se a horaFim for nula, use a meia-noite, caso contrário, use a hora especificada
            DataFim = horaFim != null ? dataFim.ToDateTime(TimeOnly.Parse(horaFim.ToString())) : dataFim.ToDateTime(TimeOnly.Parse("00:00 AM"));

            AtualizarAlertasTodos();

        }
        public void AtualizarAlertasTodos()
        {
            AlertasTodos.Clear();
            foreach (var alerta in AlertaAntecipacao.Concat(AlertaExecucao))
            {
                AlertasTodos.Add(alerta);
            }
        }
        public void CopiaTarefas(Tarefa tarefa1, Tarefa tarefa2)
        {
            tarefa1.Id = tarefa2.Id;
            tarefa1.Titulo = tarefa2.Titulo;
            tarefa1.Descricao = tarefa2.Descricao;
            tarefa1.DataInicio = tarefa2.DataInicio;
            tarefa1.DataFim = tarefa2.DataFim;
            tarefa1.NivelImportancia = tarefa2.NivelImportancia;
            tarefa1.Estado = tarefa2.Estado;
            tarefa1.Periodicidade = tarefa2.Periodicidade;
            tarefa1.AlertaAntecipacao = tarefa2.AlertaAntecipacao;
            tarefa1.AlertaExecucao = tarefa2.AlertaExecucao;
            tarefa1.HoraInicio = tarefa2.HoraInicio;
            tarefa1.HoraFim = tarefa2.HoraFim;
        }
        public override string ToString()
        {
            return Id + " " + Titulo +" " + Descricao + " " + DataInicio.ToString() + " " + DataFim.ToString() + " " +NivelImportancia+ " " +Estado+ " " +Periodicidade+ " " + AlertaAntecipacao+" " + AlertaExecucao+" ";
        }
        public XElement ToXML()
        {
            XElement tarefaElement = new XElement("tarefa",
                new XElement("id", Id),
                new XElement("titulo", Titulo),
                new XElement("descricao", Descricao),
                new XElement("dataInicio", DataInicio.ToString("yyyy-MM-dd")),
                new XElement("dataFim", DataFim.ToString("yyyy-MM-dd")),
                new XElement("horaInicio", DataInicio.ToString("HH:mm:ss")),
                new XElement("horaFim", DataFim.ToString("HH:mm:ss")),
                new XElement("nivelImportancia", NivelImportancia),
                new XElement("estado", Estado),
                Periodicidade.ToXML(), // Chama o método ToXML da classe Periodicidade
                new XElement("alertaAntecipacao", AlertaAntecipacao.Select(a => a.ToXML())), // Chama o método ToXML da classe Alerta para cada item na lista
                new XElement("alertaExecucao", AlertaExecucao.Select(a => a.ToXML())), // Chama o método ToXML da classe Alerta para cada item na lista
                new XElement("listaDiaTarefas", listaDiaTarefas.Select(d => d.ToXML()))
            );

            return tarefaElement;
        }
    }
    public static class EnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }
    }
}
