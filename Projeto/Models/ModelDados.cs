using Microsoft.Data.SqlClient;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Wpf.Ui.Controls;
using Xceed.Wpf.AvalonDock.Converters;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Projeto.Models;

public class ModelDados
{
    public event MetodosSemParametros AtualizacaoPerfilFeita;
    public event MetodosSemParametros TarefaEditada;
    public event MetodosSemParametros AlertaEditado;
    public event MetodosSemParametros AlertaRemovido;
    public event MetodosSemParametros TarefaAdicionada;
    public event MetodosSemParametros TarefaEliminada;
    public event MetodosSemParametros ListaAlertasAlterada;
    public event MetodosSemParametros ListaNavigationViewItemsAtualizada;
    public event MetodosSemParametros ListaNavigationViewItemRemovido;
    public event MetodosSemParametros LoadBotoesFeito;
    public event MetodosSemParametros ModoEscuroLoad;
    public event MetodosSemParametros ModoClaroLoad;
    public event MetodosSemParametros LinguagemAlterada;

    public Perfil perfil { get; private set; }
    public string linguaApp { get; private set; }

    public ObservableCollection<Tarefa>
        listaTarefas
    {
        get;
        private set;
    } //usei isto em vez de lista porque isto informa quanto a lista é alterada, automaticamente

    public ObservableCollection<Tarefa> listaTarefasFiltradas { get; private set; }
    public SortedSet<Alerta> listaAlertas { get; private set; }
    public string caminhoDadosArmazenados { get; private set; } //caminho para a pasta da app onde estao os dados
    public string caminhoBaseDadosArmazenados { get; private set; } //caminho para a pasta da app
    public ObservableCollection<NavigationViewItem> listaItensMenu { get; private set; }
    public List<ItemData> ListaItemDatasLoad { get; private set; }


    public ModelDados()
    {
        caminhoBaseDadosArmazenados = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (Directory.Exists(System.IO.Path.Combine(caminhoBaseDadosArmazenados, "ToDoList")) == false)
        {
            Directory.CreateDirectory(System.IO.Path.Combine(caminhoBaseDadosArmazenados, "ToDoList"));
        }

        caminhoDadosArmazenados = System.IO.Path.Combine(caminhoBaseDadosArmazenados, "ToDoList");

        if (Directory.Exists(System.IO.Path.Combine(caminhoDadosArmazenados, "Dados")) == false)
        {
            Directory.CreateDirectory(System.IO.Path.Combine(caminhoDadosArmazenados, "Dados"));
        }

        caminhoDadosArmazenados = System.IO.Path.Combine(caminhoDadosArmazenados, "Dados");

        listaTarefas = new ObservableCollection<Tarefa>();
        listaTarefasFiltradas = new ObservableCollection<Tarefa>();
        listaAlertas = new SortedSet<Alerta>(new AlertaComparer());
        listaItensMenu = new ObservableCollection<NavigationViewItem>();
        ListaItemDatasLoad = new List<ItemData>();


        ListaAlertasAlterada?.Invoke();
        perfil = new Perfil(0,Environment.UserName, "email@email.com", "pack://application:,,,/Dados/noPhoto.jpg");
        linguaApp = "pt-PT";
        FiltrarTarefasTodas();

    }

    public void AtualizarPerfil(Perfil perfil)
    {
        this.perfil = perfil;
        AtualizacaoPerfilFeita?.Invoke();
    }

    public void AtualizarLinguagem(string lingua)
    {
        linguaApp = lingua;
        LinguagemAlterada?.Invoke();
    }



    public void EliminarAlerta(Alerta alerta)
    {
        int idAlerta = -1;
        foreach (var tarefa in listaTarefas)
        {
            foreach (var alertaTarefa in tarefa.AlertaAntecipacao)
            {
                if (alertaTarefa.Id == alerta.Id)
                {
                    idAlerta = alerta.Id;
                    tarefa.AlertaAntecipacao.Remove(alertaTarefa);
                    tarefa.AtualizarAlertasTodos();
                    break;
                }
            }

            foreach (var alertaTarefa in tarefa.AlertaExecucao)
            {
                if (alertaTarefa.Id == alerta.Id)
                {
                    idAlerta = alertaTarefa.Id;
                    tarefa.AlertaExecucao.Remove(alertaTarefa);
                    tarefa.AtualizarAlertasTodos();
                    break;
                }
            }
        }

        foreach (Alerta alertaItem in listaAlertas)
        {
            if (alertaItem.Id == idAlerta)
            {
                listaAlertas.Remove(alertaItem);
                break;
            }
        }

        EliminarAlertaBD(alerta.Id);
        ListaAlertasAlterada?.Invoke();
    }

    public void EliminarAlertaBD(int alertaId)
    {
        string connectionString =
            ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Primeiro, remover da tabela Tarefa_Alerta
            string queryTarefaAlerta = "DELETE FROM Tarefa_Alerta WHERE ID_alerta = @alertaId";
            using (SqlCommand cmdTarefaAlerta = new SqlCommand(queryTarefaAlerta, conn))
            {
                cmdTarefaAlerta.Parameters.AddWithValue("@alertaId", alertaId);
                cmdTarefaAlerta.ExecuteNonQuery();
            }

            // Depois, remover da tabela alerta
            string queryAlerta = "DELETE FROM alerta WHERE ID = @alertaId";
            using (SqlCommand cmdAlerta = new SqlCommand(queryAlerta, conn))
            {
                cmdAlerta.Parameters.AddWithValue("@alertaId", alertaId);
                cmdAlerta.ExecuteNonQuery();
            }
        }
    }

    public Tarefa? GetTarefaPeloAlerta(Alerta alerta)
    {
        foreach (var tarefa in listaTarefas)
        {
            foreach (var alertaTarefa in tarefa.AlertaAntecipacao)
            {
                if (alertaTarefa.Id == alerta.Id)
                {
                    return tarefa;
                }
            }

            foreach (var alertaTarefa in tarefa.AlertaExecucao)
            {
                if (alertaTarefa.Id == alerta.Id)
                {
                    return tarefa;
                }
            }
        }

        return null;
    }

    public void AtualizarInformacoesPerfil(string nome, string email, Image ImagemPerfil, int btnSelected)
    {
        perfil.Nome = nome;
        perfil.Email = email;
        if (ImagemPerfil != null)
        {
            perfil.Fotografia = ImagemPerfil;
            perfil.PathToFotografia = ImagemPerfil.Source.ToString();
        }

        switch (btnSelected)
        {
            case 0:
                AtualizarPerfilBD("pt-PT");
                break;
            case 1:
                AtualizarPerfilBD("en-US");
                break;
            case 2:
                AtualizarPerfilBD("es-ES");
                break;
            case 3:
                AtualizarPerfilBD("fr-FR");
                break;
            case 4:
                AtualizarPerfilBD("de-DE");
                break;
            case 5:
                AtualizarPerfilBD("it-IT");
                break;
            default:
                AtualizarPerfilBD("pt-PT");
                break;
        }
        AtualizacaoPerfilFeita?.Invoke();
    }

    private void AtualizarPerfilBD(string language)
    {
        string connectionString =
    ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "UPDATE utilizador SET nome = @nome, email = @email, fotografia = @fotografia, linguagem=@linguagem WHERE ID = @ID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nome", perfil.Nome);
                cmd.Parameters.AddWithValue("@email", perfil.Email);
                cmd.Parameters.AddWithValue("@fotografia", perfil.PathToFotografia);
                cmd.Parameters.AddWithValue("@linguagem", language);
                cmd.Parameters.AddWithValue("@ID", perfil.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void EliminarTarefaBD(int tarefaId)
    {
        string connectionString =
            ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Primeiro, remover da tabela Tarefa_Alerta
            string queryTarefaAlerta = "DELETE FROM Tarefa_Alerta WHERE ID_tarefa = @tarefaId";
            using (SqlCommand cmdTarefaAlerta = new SqlCommand(queryTarefaAlerta, conn))
            {
                cmdTarefaAlerta.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdTarefaAlerta.ExecuteNonQuery();
            }

            string queryTabelaDiasTarefa = "DELETE FROM Tarefa_DiaTarefa WHERE ID_tarefa = @tarefaId";
            using (SqlCommand cmdTarefaAlerta = new SqlCommand(queryTabelaDiasTarefa, conn))
            {
                cmdTarefaAlerta.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdTarefaAlerta.ExecuteNonQuery();
            }

            // Finalmente, remover registros órfãos de DiaTarefa
            string queryDiaTarefaOrphans = @"
            DELETE FROM DiaTarefa
            WHERE ID IN (
                SELECT dt.ID
                FROM DiaTarefa dt
                LEFT JOIN Tarefa_DiaTarefa tdt ON dt.ID = tdt.ID_diaTarefa
                WHERE tdt.ID_tarefa IS NULL
            )";
            using (SqlCommand cmdDiaTarefaOrphans = new SqlCommand(queryDiaTarefaOrphans, conn))
            {

                cmdDiaTarefaOrphans.ExecuteNonQuery();
            }

            // Finalmente, remover registros órfãos de DiaTarefa
            string queryTarefaPeriodicidade = @"
            DELETE FROM Tarefa_Periodicidade
            WHERE ID_tarefa = @tarefaId";
            using (SqlCommand cmdTarefaPeriodicidade = new SqlCommand(queryTarefaPeriodicidade, conn))
            {
                cmdTarefaPeriodicidade.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdTarefaPeriodicidade.ExecuteNonQuery();
            }

            // Finalmente, remover registros órfãos de DiaTarefa
            string queryPeriodicidade = @"
            DELETE FROM periodicidade
            WHERE ID IN (
                SELECT p.ID
                FROM periodicidade p
                LEFT JOIN Tarefa_Periodicidade tp ON p.ID = tp.ID_periodicidade
                WHERE tp.ID_tarefa IS NULL
            )";
            using (SqlCommand cmdPeriodicidade = new SqlCommand(queryPeriodicidade, conn))
            {
                cmdPeriodicidade.ExecuteNonQuery();
            }

            // Depois, remover da tabela Tarefa
            string queryPossuir = "DELETE FROM Possuir WHERE ID_tarefa = @tarefaId";
            using (SqlCommand cmdPossuir = new SqlCommand(queryPossuir, conn))
            {
                cmdPossuir.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdPossuir.ExecuteNonQuery();
            }

            string queryListaPersonalizada = "DELETE FROM ListaPersonalizada_Tarefa WHERE ID_tarefa = @tarefaId";
            using (SqlCommand cmdLP = new SqlCommand(queryListaPersonalizada, conn))
            {
                cmdLP.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdLP.ExecuteNonQuery();
            }


            // Depois, remover da tabela Tarefa
            string queryTarefa = "DELETE FROM tarefa WHERE ID = @tarefaId";
            using (SqlCommand cmdTarefa = new SqlCommand(queryTarefa, conn))
            {
                cmdTarefa.Parameters.AddWithValue("@tarefaId", tarefaId);
                cmdTarefa.ExecuteNonQuery();
            }
        }
    }

    public void EliminarElementoLista(Tarefa tarefa)
    {

        foreach (var alerta in tarefa.AlertaAntecipacao) //eliminar os alertas da tarefa
        {
            listaAlertas.Remove(alerta);
            EliminarAlertaBD(alerta.Id);
        }

        foreach (var alerta in tarefa.AlertaExecucao) //eliminar os alertas da tarefa
        {
            listaAlertas.Remove(alerta);
            EliminarAlertaBD(alerta.Id);
        }

        ListaAlertasAlterada?.Invoke();
        foreach (var elementoTarefa in listaTarefas) //eliminar a tarefa da lista de tarefas
        {
            if (elementoTarefa.Id == tarefa.Id)
            {
                listaTarefas.Remove(elementoTarefa);
                EliminarTarefaBD(tarefa.Id);
                break;
            }
        }

        //remover o id da tarefa do navigationviewitem
        foreach (NavigationViewItem item in listaItensMenu)
        {
            if (((ItemData)item.Tag).IdsTarefas.Contains(tarefa.Id))
            {
                ((ItemData)item.Tag).IdsTarefas.Remove(tarefa.Id);
                EliminarTarefaIdBotaoPersonalizadoBD(tarefa.Id, ((ItemData)item.Tag).IdNavigationItem);
            }
        }
        //alterar aqui para atualizar

        ListaAlertasAlterada?.Invoke();
        ListaNavigationViewItemsAtualizada?.Invoke();
        TarefaEliminada?.Invoke();
    }

    public void EliminarAlertasTarefa(Tarefa tarefa)
    {
        foreach (var alerta in tarefa.AlertaAntecipacao)
        {
            EliminarAlertaBD(alerta.Id);
            listaAlertas.Remove(alerta);
        }

        foreach (var alerta in tarefa.AlertaExecucao)
        {
            EliminarAlertaBD(alerta.Id);
            listaAlertas.Remove(alerta);
        }

        tarefa.AlertaAntecipacao.Clear();
        tarefa.AlertaExecucao.Clear();
        tarefa.AtualizarAlertasTodos();
        ListaAlertasAlterada?.Invoke();
        TarefaEditada?.Invoke();
    }

    public void EditarElementoLista(Tarefa tarefa)
    {
        int contador = 0;
        foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
        {
            if (dia.ativo == true)
                contador++;
        }

        if (contador == 0)
        {
            tarefa.Estado = Estado.PorTerminar;
        }

        foreach (var elementoTarefa in listaTarefas)
        {
            if (elementoTarefa.Id == tarefa.Id)
            {
                int index = listaTarefas.IndexOf(elementoTarefa); //obter index
                listaTarefas[index] = tarefa; //substituir

                foreach (var alerta in tarefa.AlertaAntecipacao)
                {
                    listaAlertas.Add(alerta);
                }

                foreach (var alerta in tarefa.AlertaExecucao)
                {
                    listaAlertas.Add(alerta);
                }

                //MessageBox.Show(tarefa.Estado.ToString());

                TarefaEditada?.Invoke();
                ListaAlertasAlterada?.Invoke();
                return;
            }
        }
    }

    public void AdicionarElementoLista(Tarefa tarefa)
    {
        foreach (var elementoTarefa in listaTarefas)
        {
            if (elementoTarefa.Id == tarefa.Id)
            {
                return;
            }
        }

        listaTarefas.Add(tarefa);
        listaTarefasFiltradas.Add(tarefa);

        foreach (var alerta in tarefa.AlertaAntecipacao)
        {
            listaAlertas.Add(alerta);
        }

        foreach (var alerta in tarefa.AlertaExecucao)
        {
            listaAlertas.Add(alerta);
        }
        TarefaAdicionada?.Invoke();
        ListaAlertasAlterada?.Invoke();
    }

    public void EditarTarefa(Tarefa tarefa, string txtbTituloText, string txtbTituloDescricao,
        string timePickerHoraInicioText, string timePickerHoraFimText, DateTime datePInicioSelectedDate
        , DateTime datePFimSelectedDate, Importancia comboBPrioridadesSelectedIndex, bool? btnSegundaIsChecked,
        bool? btnTercaIsChecked, bool? btnQuartaIsChecked, bool? btnQuintaIsChecked,
        bool? btnSextaIsChecked, bool? btnSabadoIsChecked, bool? btnDomingoIsChecked,
        string txtbTempoParaAlertaAntecipacaoText, bool? toggleBtnAlertaAntecipacaoIsChecked,
        bool? btnEmailAntecipacaoIsChecked, bool? btnWindowsAntecipacaoIsChecked,
        bool? toggleBtnAlertaNaoRealizacaoIsChecked, bool? btnEmailNaoRealizacaoIsChecked,
        bool? btnWindowsNaoRealizacaoIsChecked, bool? toggleBtnRepetirTarefaIsChecked
    )
    {
        try
        {
            int numeroDiasPeriodicidade = 0;
            tarefa.Titulo = txtbTituloText;
            tarefa.Descricao = txtbTituloDescricao;
            List<DiaTarefa> diasTarefaCopia = new List<DiaTarefa>(tarefa.listaDiaTarefas);

            Dictionary<DateOnly, bool> dicDataAtivo = new Dictionary<DateOnly, bool>();

            foreach (var diaTarefa in diasTarefaCopia)
            {
                dicDataAtivo[diaTarefa.Data] = diaTarefa.ativo;
            }

            tarefa.listaDiaTarefas.Clear();
            LimparDiasTarefaBD(tarefa.Id);

            string horaInicioString = timePickerHoraInicioText;
            string[] horaInicioSeparado = horaInicioString.Split(':');

            string horaFimString = timePickerHoraFimText;
            string[] horaFimSeparado = horaFimString.Split(':');



            TimeOnly horaInicio =
                new TimeOnly(Convert.ToInt32(horaInicioSeparado[0]), Convert.ToInt32(horaInicioSeparado[1]));
            TimeOnly horaFim = new TimeOnly(Convert.ToInt32(horaFimSeparado[0]), Convert.ToInt32(horaFimSeparado[1]));
            DateOnly dataInicio = DateOnly.FromDateTime(datePInicioSelectedDate);
            DateOnly dataFim = DateOnly.FromDateTime(datePFimSelectedDate);

            // Se a horaInicio for nula, use a meia-noite, caso contrário, use a hora especificada
            tarefa.DataInicio = horaInicio != null
                ? dataInicio.ToDateTime(TimeOnly.Parse(horaInicio.ToString()))
                : dataInicio.ToDateTime(TimeOnly.Parse("00:00 AM"));

            // Se a horaFim for nula, use a meia-noite, caso contrário, use a hora especificada
            tarefa.DataFim = horaFim != null
                ? dataFim.ToDateTime(TimeOnly.Parse(horaFim.ToString()))
                : dataFim.ToDateTime(TimeOnly.Parse("00:00 AM"));

            //fazer para as datas
            tarefa.NivelImportancia = comboBPrioridadesSelectedIndex;
            //tarefa.Estado = Estado.PorComecar;
            tarefa.Periodicidade.DiasSemana.Clear();
            if (toggleBtnRepetirTarefaIsChecked == true)
            {
                if (btnSegundaIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Monday);
                    numeroDiasPeriodicidade++;
                }

                if (btnTercaIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Tuesday);
                    numeroDiasPeriodicidade++;
                }

                if (btnQuartaIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Wednesday);
                    numeroDiasPeriodicidade++;
                }

                if (btnQuintaIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Thursday);
                    numeroDiasPeriodicidade++;
                }

                if (btnSextaIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Friday);
                    numeroDiasPeriodicidade++;
                }

                if (btnSabadoIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Saturday);
                    numeroDiasPeriodicidade++;
                }

                if (btnDomingoIsChecked == true)
                {
                    tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Sunday);
                    numeroDiasPeriodicidade++;
                }
            }

            //visto que estou a editar a tarefa, ja existe necessariamente uma periodicidade
            if (numeroDiasPeriodicidade == 7 || numeroDiasPeriodicidade == 0)
            {
                tarefa.Periodicidade.Tipo = TipoPeriodicidade.Diaria;
            }
            else
            {
                tarefa.Periodicidade.Tipo = TipoPeriodicidade.Semanal;
            }

            foreach (Alerta alerta in tarefa.AlertaAntecipacao)
            {
                EliminarAlertaBD(alerta.Id);
            }

            foreach (Alerta alerta in tarefa.AlertaExecucao)
            {
                EliminarAlertaBD(alerta.Id);
            }

            tarefa.AlertaAntecipacao.Clear();
            tarefa.AlertaExecucao.Clear();


            if (horaInicio >= horaFim)
            {
                //se for semanal preciso verificar se o dia da semana esta na lista de dias da semana
                if (tarefa.Periodicidade.Tipo == TipoPeriodicidade.Semanal)
                {
                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if (toggleBtnAlertaAntecipacaoIsChecked == false &&
                            toggleBtnAlertaNaoRealizacaoIsChecked == false)
                        {
                            break;
                        }

                        if ( toggleBtnAlertaNaoRealizacaoIsChecked == true && date >= dataInicio.AddDays(1) && tarefa.Periodicidade.DiasSemana.Contains(date.AddDays(-1).DayOfWeek) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date]==true))
                        {
                            tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                                , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));
                            if (btnEmailNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                            }

                            if (btnWindowsNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1], tarefa.Id);
                        }

                        if (toggleBtnAlertaAntecipacaoIsChecked == true && date <= dataFim.AddDays(-1) && tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true) )
                        {
                            tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                                , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                                new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));

                            if (btnEmailAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.Email);
                            }

                            if (btnWindowsAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                        }
                    }

                    for (DateOnly date = dataInicio; date <= dataFim.AddDays(-1); date = date.AddDays(1)) //o ultimo dia nao ha tarefa
                    {
                        if(date.AddDays(1).ToDateTime(horaFim) <= DateTime.Now)
                            continue;
                        else if (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek))
                        {
                            tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(),date, true));
                            AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count - 1], tarefa.Id);
                        }
                    }
                }
                else
                {
                    //se for diaria
                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if (toggleBtnAlertaAntecipacaoIsChecked == false &&
                            toggleBtnAlertaNaoRealizacaoIsChecked == false)
                        {
                            break;
                        }

                        if (toggleBtnAlertaNaoRealizacaoIsChecked == true && date >= dataInicio.AddDays(1) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                                , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));
                            if (btnEmailNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                            }

                            if (btnWindowsNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1], tarefa.Id);
                        }

                        if (toggleBtnAlertaAntecipacaoIsChecked == true && date <= dataFim.AddDays(-1) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                                , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                                new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));

                            if (btnEmailAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.Email);
                            }

                            if (btnWindowsAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                        }
                    }
                    for (DateOnly date = dataInicio; date <= dataFim.AddDays(-1); date = date.AddDays(1))
                    {
                        if (date.AddDays(1).ToDateTime(horaFim) <= DateTime.Now)
                            continue;
                        else
                        {
                            tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(), date, true));
                            AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count - 1], tarefa.Id);
                        }
                    }
                }
            }
            else
            {
                //se a tarefa ocorrer 1 a 6 vezes por semana
                if (tarefa.Periodicidade.Tipo == TipoPeriodicidade.Semanal)
                {
                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if (toggleBtnAlertaAntecipacaoIsChecked == false &&
                            toggleBtnAlertaNaoRealizacaoIsChecked == false)
                        {
                            break;
                        }

                        if (date.ToDateTime(horaFim) >= DateTime.Now && toggleBtnAlertaNaoRealizacaoIsChecked == true &&
                            tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                                , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));
                            if (btnEmailNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                            }

                            if (btnWindowsNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1], tarefa.Id);
                        }

                        if (date.ToDateTime(horaInicio) >= DateTime.Now && toggleBtnAlertaAntecipacaoIsChecked == true &&
                            tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                                , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                                new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));

                            if (btnEmailAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.Email);
                            }

                            if (btnWindowsAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                        }
                    }

                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if (date.ToDateTime(horaFim) <= DateTime.Now)
                            continue;
                        else if(tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek))
                        {
                            tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(),date, true));
                            AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count - 1], tarefa.Id);
                        }
                    }
                }
                else
                {
                    //se for diaria, ou seja, ocorre naquele dia ou todos os dias da semana
                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if (toggleBtnAlertaAntecipacaoIsChecked == false &&
                            toggleBtnAlertaNaoRealizacaoIsChecked == false)
                        {
                            break;
                        }

                        if (date.ToDateTime(horaFim) >= DateTime.Now && toggleBtnAlertaNaoRealizacaoIsChecked == true && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                                , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));
                            if (btnEmailNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                            }

                            if (btnWindowsNaoRealizacaoIsChecked == true)
                            {
                                tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }


                            AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1], tarefa.Id);

                        }

                        if (date.ToDateTime(horaInicio) >= DateTime.Now && toggleBtnAlertaAntecipacaoIsChecked == true && (!dicDataAtivo.ContainsKey(date) || dicDataAtivo[date] == true))
                        {
                            tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                                , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                                new List<TipoAlerta>(),
                                desligado: false, GetNextAlertaId()));

                            if (btnEmailAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.Email);
                            }

                            if (btnWindowsAntecipacaoIsChecked == true)
                            {
                                tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                    .Add(TipoAlerta.AlertaWindows);
                            }

                            AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                        }
                    }
                    for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                    {
                        if(date.ToDateTime(horaFim) <= DateTime.Now)
                            continue;
                        else
                        {
                            tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(), date, true));
                            AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count - 1], tarefa.Id);
                        }
                    }
                }
            }
            //para comparar os dias da tarefa e da copia
            DiaTarefaComparer comparer = new DiaTarefaComparer();

            //subtituir os alertas intersetados da listaDiaTarefas e diasTarefaCopia
            //nao esta fuincional
            foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
            {
                if (diasTarefaCopia.Contains(dia, comparer))
                {
                    int index = diasTarefaCopia.FindIndex(d => comparer.Equals(d, dia));
                    dia.ativo = diasTarefaCopia[index].ativo;
                    AtualizarDiaTarefaBD(dia.Id, dia.ativo);
                }
            }

            if (tarefa.listaDiaTarefas.Count == 0)
            {
                tarefa.Estado = Estado.PorTerminar;
                foreach (var alerta in tarefa.AlertaExecucao)
                {
                    EliminarAlertaBD(alerta.Id);
                }
                foreach (var alerta in tarefa.AlertaAntecipacao)
                {
                    EliminarAlertaBD(alerta.Id);
                }

                tarefa.AlertaExecucao.Clear();
                tarefa.AlertaAntecipacao.Clear();
                tarefa.AlertasTodos.Clear();

            }
            else
            {
                tarefa.Estado = Estado.PorComecar;
            }
            EditarTarefaBD(tarefa);
            EditarPeriodicidadeBD(tarefa);
            EditarElementoLista(tarefa);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AdicionarAlertaBD(Alerta alerta, int idTarefa)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "INSERT INTO alerta (mensagem,data_hora, desligado, tipoEmail, tipoWindows) VALUES (@mensagem,@data_hora, @desligado, @tipoEmail, @tipoWindows)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@mensagem", alerta.Mensagem);
                cmd.Parameters.AddWithValue("@desligado", alerta.Desligado);
                cmd.Parameters.AddWithValue("@data_hora", alerta.Data_Hora);
                cmd.Parameters.AddWithValue("@tipoEmail", alerta.Tipos.Contains(TipoAlerta.Email));
                cmd.Parameters.AddWithValue("@tipoWindows", alerta.Tipos.Contains(TipoAlerta.AlertaWindows));
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO Tarefa_Alerta (ID_tarefa, ID_alerta) VALUES (@ID_tarefa, @ID_alerta)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID_alerta", alerta.Id);
                cmd.Parameters.AddWithValue("@ID_tarefa", idTarefa);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void EditarAlertaBD(Alerta alerta)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "UPDATE alerta SET mensagem = @mensagem, data_hora = @data_hora, desligado = @desligado, tipoEmail = @tipoEmail, tipoWindows = @tipoWindows WHERE ID = @ID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@mensagem", alerta.Mensagem);
                cmd.Parameters.AddWithValue("@desligado", alerta.Desligado);
                cmd.Parameters.AddWithValue("@data_hora", alerta.Data_Hora);
                cmd.Parameters.AddWithValue("@tipoEmail", alerta.Tipos.Contains(TipoAlerta.Email));
                cmd.Parameters.AddWithValue("@tipoWindows", alerta.Tipos.Contains(TipoAlerta.AlertaWindows));
                cmd.Parameters.AddWithValue("@ID", alerta.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void AdicionarDiaTarefa(DiaTarefa dia, int idTarefa)
    {

        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query =
                "INSERT INTO diaTarefa (dataDia,ativo) VALUES (@dataDia,@ativo)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@dataDia", dia.Data);
                cmd.Parameters.AddWithValue("@ativo", dia.ativo);
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO Tarefa_DiaTarefa (ID_tarefa, ID_diaTarefa) VALUES (@ID_tarefa, @ID_diaTarefa)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID_diaTarefa", dia.Id);
                cmd.Parameters.AddWithValue("@ID_tarefa", idTarefa);
                cmd.ExecuteNonQuery();
            }

        }
    }

    private void AdicionarTarefaBD(Tarefa tarefa)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "INSERT INTO Tarefa (titulo, descricao, dataInicio, dataFim,horaInicio, horaFim, nivelImportancia, estado) VALUES (@Titulo, @Descricao, @DataInicio, @DataFim, @HoraInicio, @HoraFim, @NivelImportancia, @Estado)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Titulo", tarefa.Titulo);
                cmd.Parameters.AddWithValue("@Descricao", tarefa.Descricao);
                cmd.Parameters.AddWithValue("@DataInicio", tarefa.DataInicio.Date);
                cmd.Parameters.AddWithValue("@DataFim", tarefa.DataFim.Date);
                cmd.Parameters.AddWithValue("@HoraInicio", TimeOnly.FromDateTime(tarefa.DataInicio));
                cmd.Parameters.AddWithValue("@HoraFim", TimeOnly.FromDateTime(tarefa.DataFim));
                cmd.Parameters.AddWithValue("@NivelImportancia", tarefa.NivelImportancia);
                cmd.Parameters.AddWithValue("@Estado", tarefa.Estado);
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO Possuir (ID_utilizador, ID_tarefa) VALUES (@ID_USER, @ID_TAREFA)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID_USER", perfil.Id);
                cmd.Parameters.AddWithValue("@ID_TAREFA", tarefa.Id);
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO Periodicidade (diasSemana, tipo) VALUES (@diasSemana, @tipo)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                string diasSemana = string.Join(",", tarefa.Periodicidade.DiasSemana.Select(d => d.ToString()));
                cmd.Parameters.AddWithValue("@DiasSemana", diasSemana);
                cmd.Parameters.AddWithValue("@tipo", tarefa.Periodicidade.Tipo);
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO Tarefa_Periodicidade (ID_tarefa, ID_periodicidade) VALUES (@ID_tarefa, @ID_periodicidade)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID_tarefa", tarefa.Id);
                cmd.Parameters.AddWithValue("@ID_periodicidade", tarefa.Periodicidade.Id);
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO diaTarefa (dataDia, ativo) VALUES (@ID_tarefa, @ID_periodicidade)";


        }
    }

    public void AlterarEstadoTarefaBD(Tarefa tarefa)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "UPDATE tarefa SET estado = @estado WHERE ID = @idTarefa";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@estado", tarefa.Estado);
                cmd.Parameters.AddWithValue("@idTarefa", tarefa.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void EditarTarefaBD(Tarefa tarefa)
    {

        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                    UPDATE tarefa
                    SET titulo = @titulo,
                        descricao = @descricao,
                        dataInicio = @dataInicio,
                        dataFim = @dataFim,
                        horaInicio = @horaInicio,
                        horaFim = @horaFim,
                        NivelImportancia = @nivelImportancia,
                        Estado = @estado
                    WHERE ID = @idTarefa";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@titulo", tarefa.Titulo);
                cmd.Parameters.AddWithValue("@descricao", tarefa.Descricao);
                cmd.Parameters.AddWithValue("@dataInicio", tarefa.DataInicio);
                cmd.Parameters.AddWithValue("@dataFim", tarefa.DataFim);
                cmd.Parameters.AddWithValue("@horaInicio", TimeOnly.FromDateTime(tarefa.DataInicio));
                cmd.Parameters.AddWithValue("@horaFim", TimeOnly.FromDateTime(tarefa.DataFim));
                cmd.Parameters.AddWithValue("@nivelImportancia", tarefa.NivelImportancia);
                cmd.Parameters.AddWithValue("@estado", tarefa.Estado);
                cmd.Parameters.AddWithValue("@idTarefa", tarefa.Id);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void EditarPeriodicidadeBD(Tarefa tarefa)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                    UPDATE periodicidade
                    SET diasSemana = @diasSemana,
                        tipo = @tipo
                    WHERE ID = (
                        SELECT ID_periodicidade
                        FROM Tarefa_Periodicidade
                        WHERE ID_tarefa = @idTarefa
                    )";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@diasSemana", string.Join(",", tarefa.Periodicidade.DiasSemana.Select(d => d.ToString())));
                cmd.Parameters.AddWithValue("@tipo", tarefa.Periodicidade.Tipo);
                cmd.Parameters.AddWithValue("@idTarefa", tarefa.Id);

                cmd.ExecuteNonQuery();
            }
        }

    }

    private int GetNextAlertaId()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT IDENT_CURRENT('alerta')";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result) + 1;
                }
            }
        }

        return -1;
    }

    private int GetNextListaPersonalizadaId()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT IDENT_CURRENT('listaPersonalizada')";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result) + 1;
                }
            }
        }

        return -1;
    }


    public int GetNextDiaTarefaId()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT IDENT_CURRENT('diaTarefa')";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result) + 1;
                }
            }
        }

        return -1;
    }
    private int GetNextTaskId()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT IDENT_CURRENT('tarefa')";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result) + 1;
                }
            }
        }

        return -1;
    }

    private int GetNextPeriodicidadeId()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT IDENT_CURRENT('periodicidade')";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result) + 1;
                }
            }
        }

        return -1;
    }

    private void LimparDiasTarefaBD(int idTarefa)
    {
        // Connection string from configuration
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        // Primeiro, remover registros na tabela Tarefa_DiaTarefa
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "DELETE FROM Tarefa_DiaTarefa WHERE ID_tarefa = @idTarefa";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idTarefa", idTarefa);
                cmd.ExecuteNonQuery();
            }
        }

        // Em seguida, remover registros órfãos na tabela DiaTarefa
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
            DELETE FROM DiaTarefa
            WHERE ID IN (
                SELECT dt.ID
                FROM DiaTarefa dt
                LEFT JOIN Tarefa_DiaTarefa tdt ON dt.ID = tdt.ID_diaTarefa
                WHERE tdt.ID_diaTarefa IS NULL
            )";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void AdicionarTarefa(string txtbTituloText, string txtbTituloDescricao,
        string timePickerHoraInicioText, string timePickerHoraFimText, DateTime datePInicioSelectedDate
        , DateTime datePFimSelectedDate, Importancia comboBPrioridadesSelectedIndex, bool? btnSegundaIsChecked,
        bool? btnTercaIsChecked, bool? btnQuartaIsChecked, bool? btnQuintaIsChecked,
        bool? btnSextaIsChecked, bool? btnSabadoIsChecked, bool? btnDomingoIsChecked,
        string txtbTempoParaAlertaAntecipacaoText, bool? toggleBtnAlertaAntecipacaoIsChecked,
        bool? btnEmailAntecipacaoIsChecked, bool? btnWindowsAntecipacaoIsChecked,
        bool? toggleBtnAlertaNaoRealizacaoIsChecked, bool? btnEmailNaoRealizacaoIsChecked,
        bool? btnWindowsNaoRealizacaoIsChecked, string nomeBotaoListaPersonalizado = ""
    )
    {
        try
        {
            Tarefa tarefa = new Tarefa(GetNextTaskId());
            if (tarefa.Id == -1)
            {
                MessageBox.Show("Erro ao adicionar tarefa", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception("Erro ao adicionar tarefa");
            }

            int numeroDiasPeriodicidade = 0;
            tarefa.Titulo = txtbTituloText;
            tarefa.Descricao = txtbTituloDescricao;

            string horaInicioString = timePickerHoraInicioText;
            string[] horaInicioSeparado = horaInicioString.Split(':');

            string horaFimString = timePickerHoraFimText;
            string[] horaFimSeparado = horaFimString.Split(':');



            TimeOnly horaInicio =
                new TimeOnly(Convert.ToInt32(horaInicioSeparado[0]), Convert.ToInt32(horaInicioSeparado[1]));
            TimeOnly horaFim = new TimeOnly(Convert.ToInt32(horaFimSeparado[0]), Convert.ToInt32(horaFimSeparado[1]));
            DateOnly dataInicio = DateOnly.FromDateTime(datePInicioSelectedDate);
            DateOnly dataFim = DateOnly.FromDateTime(datePFimSelectedDate);

            // Se a horaInicio for nula, use a meia-noite, caso contrário, use a hora especificada
            tarefa.DataInicio = horaInicio != null
                ? dataInicio.ToDateTime(TimeOnly.Parse(horaInicio.ToString()))
                : dataInicio.ToDateTime(TimeOnly.Parse("00:00 AM"));

            // Se a horaFim for nula, use a meia-noite, caso contrário, use a hora especificada
            tarefa.DataFim = horaFim != null
                ? dataFim.ToDateTime(TimeOnly.Parse(horaFim.ToString()))
                : dataFim.ToDateTime(TimeOnly.Parse("00:00 AM"));

            //fazer para as datas
            tarefa.NivelImportancia = comboBPrioridadesSelectedIndex;
            tarefa.Periodicidade = new Periodicidade(GetNextPeriodicidadeId());

            //verificar se comeca logo em por comecao ou pode comecar em execucao
            tarefa.Estado = Estado.PorComecar;

            tarefa.Periodicidade.DiasSemana = new List<DayOfWeek>();
            if (btnSegundaIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Monday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Monday);
            }

            if (btnTercaIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Tuesday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Tuesday);
            }

            if (btnQuartaIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Wednesday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Wednesday);
            }

            if (btnQuintaIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Thursday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Thursday);
            }

            if (btnSextaIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Friday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Friday);
            }

            if (btnSabadoIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Saturday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Saturday);
            }

            if (btnDomingoIsChecked == true)
            {
                tarefa.Periodicidade.DiasSemana.Add(DayOfWeek.Sunday);
                numeroDiasPeriodicidade++;
            }
            else
            {
                tarefa.Periodicidade.DiasSemana.Remove(DayOfWeek.Sunday);
            }

            //visto que estou a editar a tarefa, ja existe necessariamente uma periodicidade
            if (numeroDiasPeriodicidade == 7 || numeroDiasPeriodicidade == 0)
            {
                tarefa.Periodicidade.Tipo = TipoPeriodicidade.Diaria;
            }
            else
            {
                tarefa.Periodicidade.Tipo = TipoPeriodicidade.Semanal;
            }

            tarefa.AlertaAntecipacao = new ObservableCollection<Alerta>();
            tarefa.AlertaExecucao = new ObservableCollection<Alerta>();

            AdicionarTarefaBD(tarefa);

            if (horaInicio >= horaFim)
            {
                for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                {
                    if (toggleBtnAlertaAntecipacaoIsChecked == false && toggleBtnAlertaNaoRealizacaoIsChecked == false)
                    {
                        break;
                    }

                    //para o primeiro dia nunca se gera o alerta de nao realizacao porque comeca hoje e termina amanha
                    //entao basta verificar se a tarefa é para ser repetida naquele dia
                    if (toggleBtnAlertaNaoRealizacaoIsChecked == true &&
                        (tarefa.Periodicidade.DiasSemana.Contains(date.AddDays(-1).DayOfWeek) ||
                         tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria) && date >= dataInicio.AddDays(1)) //nao gerar o ultimo alerta de nao realizacao
                    {
                        tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                            , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                            desligado: false, GetNextAlertaId()));
                        if (btnEmailNaoRealizacaoIsChecked == true)
                        {
                            tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                        }

                        if (btnWindowsNaoRealizacaoIsChecked == true)
                        {
                            tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                .Add(TipoAlerta.AlertaWindows);
                        }
                        AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count-1], tarefa.Id);
                    }

                    if (toggleBtnAlertaAntecipacaoIsChecked == true &&
                        (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) ||
                         tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria) && date <= dataFim.AddDays(-1))
                    {
                        tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                            , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                            new List<TipoAlerta>(),
                            desligado: false, GetNextAlertaId()));

                        if (btnEmailAntecipacaoIsChecked == true)
                        {
                            tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos.Add(TipoAlerta.Email);
                        }

                        if (btnWindowsAntecipacaoIsChecked == true)
                        {
                            tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                .Add(TipoAlerta.AlertaWindows);
                        }

                        AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                    }
                }
                for (DateOnly date = dataInicio; date <= dataFim.AddDays(-1); date = date.AddDays(1)) //o ultimo dia nao ha tarefa
                {
                    if (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) || tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria)
                    {
                        tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(),date, true));
                        AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count-1], tarefa.Id);
                    }
                }
            }
            else
            {
                for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                {
                    if (toggleBtnAlertaAntecipacaoIsChecked == false && toggleBtnAlertaNaoRealizacaoIsChecked == false)
                    {
                        break;
                    }

                    if (date.ToDateTime(horaFim) >= DateTime.Now && toggleBtnAlertaNaoRealizacaoIsChecked == true &&
                        (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) ||
                         tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria)) //se é para repetir naquele dia ou se ele nao se repete, é diaria
                    {
                        tarefa.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                            , date.ToDateTime(horaFim).AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(),
                            desligado: false, GetNextAlertaId()));
                        if (btnEmailNaoRealizacaoIsChecked == true)
                        {
                            tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                        }

                        if (btnWindowsNaoRealizacaoIsChecked == true)
                        {
                            tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1].Tipos
                                .Add(TipoAlerta.AlertaWindows);
                        }

                        AdicionarAlertaBD(tarefa.AlertaExecucao[tarefa.AlertaExecucao.Count - 1], tarefa.Id);
                    }

                    //verificar se o alerta de antecipacao ainda é para ser criado ou nao, pois se a hora de inicio ja passou, nao faz sentido
                    if (date.ToDateTime(horaInicio) >= DateTime.Now && toggleBtnAlertaAntecipacaoIsChecked == true &&
                        (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) ||
                         tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria))
                    {
                        tarefa.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                            , date.ToDateTime(horaInicio).AddMinutes(-Convert.ToDouble(txtbTempoParaAlertaAntecipacaoText)),
                            new List<TipoAlerta>(),
                            desligado: false, GetNextAlertaId()));

                        if (btnEmailAntecipacaoIsChecked == true)
                        {
                            tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                .Add(TipoAlerta.Email);
                        }

                        if (btnWindowsAntecipacaoIsChecked == true)
                        {
                            tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1].Tipos
                                .Add(TipoAlerta.AlertaWindows);
                        }
                        AdicionarAlertaBD(tarefa.AlertaAntecipacao[tarefa.AlertaAntecipacao.Count - 1], tarefa.Id);
                    }

                }
                for (DateOnly date = dataInicio; date <= dataFim; date = date.AddDays(1))
                {
                    //para caso o date seja hoje e a tarefa ja tenha passado nao vou gerar alertas para hoje, mas para os outros dias posso gerar
                    if (horaFim <= TimeOnly.FromDateTime(DateTime.Now) && date == DateOnly.FromDateTime(DateTime.Today))
                        continue;
                    else
                        if (tarefa.Periodicidade.DiasSemana.Contains(date.DayOfWeek) || tarefa.Periodicidade.Tipo == TipoPeriodicidade.Diaria)
                        {
                            tarefa.listaDiaTarefas.Add(new DiaTarefa(GetNextDiaTarefaId(),date, true));
                            AdicionarDiaTarefa(tarefa.listaDiaTarefas[tarefa.listaDiaTarefas.Count - 1], tarefa.Id);
                    }
                }

            }

            if (nomeBotaoListaPersonalizado != "")
            {
                AdicionarTarefaABotaoListaPersonalizada(tarefa.Id, nomeBotaoListaPersonalizado);

            }

            //VERIFICAR SE ESTA BEM OU NAO
            if (tarefa.listaDiaTarefas.Count == 0)
            {
                tarefa.Estado = Estado.PorTerminar;
                tarefa.AlertaExecucao.Clear();
                tarefa.AlertaAntecipacao.Clear();
                tarefa.AlertasTodos.Clear();
            }
            else
            {
                tarefa.Estado = Estado.PorComecar;
            }

            AdicionarElementoLista(tarefa);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void FiltrarTarefasTodas()
    {
        listaTarefasFiltradas.Clear();
        foreach (var tarefa in listaTarefas)
        {
            listaTarefasFiltradas.Add(tarefa);
        }
    }

    public void FiltrarTarefasHoje(string importancia = "")
    {
        listaTarefasFiltradas.Clear();

        if (importancia == "") //se a importancia nao for especificada
        {
            foreach (var tarefa in listaTarefas) //apenas verifica pela data
            {
                if (TarefaVaiOcorrer(tarefa, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today))) //se estiver no mesmo dia e ela repetir nesse dia
                {
                    listaTarefasFiltradas.Add(tarefa);
                }
            }
        }
        else //se a importancia for especificada
        {
            foreach (var tarefa in listaTarefas) //temos que verificar pela data e se a tarefa é daquela importancia especificada
            {
                if (TarefaVaiOcorrer(tarefa, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today))) //se estiver no mesmo dia e ela repetir nesse dia
                {
                    if (importancia == Application.Current.FindResource("btnTarefasPoucoImportanteName") as string && tarefa.NivelImportancia == Importancia.PoucoImportante)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaImporante") as string && tarefa.NivelImportancia == Importancia.Importante)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaNormal") as string && tarefa.NivelImportancia == Importancia.Normal)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaPrioritaria") as string && tarefa.NivelImportancia == Importancia.Prioritaria)
                        listaTarefasFiltradas.Add(tarefa);
                }
            }
        }
    }


    public void FiltrarTarefas(string importancia)
    {
        Importancia aux;
        string pi = Application.Current.FindResource("btnTarefasPoucoImportanteName") as string;
        string normal = Application.Current.FindResource("adicionarTarefaComboBoxImportanciaNormal") as string;
        string importante = Application.Current.FindResource("adicionarTarefaComboBoxImportanciaImporante") as string;
        string prioritaria = Application.Current.FindResource("adicionarTarefaComboBoxImportanciaPrioritaria") as string;

        if (importancia == pi)
        {
            aux = Importancia.PoucoImportante;
        }
        else if (importancia == normal)
        {
            aux = Importancia.Normal;
        }
        else if (importancia == importante)
        {
            aux = Importancia.Importante;
        }
        else if(importancia == prioritaria)
        {
            aux = Importancia.Prioritaria;
        }
        else
        {
            aux = Importancia.Normal;
        }

        listaTarefasFiltradas.Clear();
        foreach (var tarefa in listaTarefas)
        {
            if (tarefa.NivelImportancia == aux)
            {
                listaTarefasFiltradas.Add(tarefa);
            }
        }
    }

    public void FiltrarTarefas(Estado estado)
    {
        listaTarefasFiltradas.Clear();
        foreach (var tarefa in listaTarefas)
        {
            if (tarefa.Estado == estado)
            {
                listaTarefasFiltradas.Add(tarefa);
            }
        }
    }

    public void FiltrarTarefas(DateOnly dataInicio, DateOnly dataFim, string importancia = "")
    {
        if (importancia == "")
        {
            listaTarefasFiltradas.Clear();
            foreach (var tarefa in listaTarefas)
            {
                if (TarefaVaiOcorrer(tarefa, dataInicio, dataFim))
                {
                    listaTarefasFiltradas.Add(tarefa);
                }
            }
        }
        else
        {
            listaTarefasFiltradas.Clear();
            foreach (var tarefa in listaTarefas)
            {
                if (TarefaVaiOcorrer(tarefa, dataInicio, dataFim))
                {
                    if (importancia == Application.Current.FindResource("btnTarefasPoucoImportanteName") as string && tarefa.NivelImportancia == Importancia.PoucoImportante)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaImporante") as string && tarefa.NivelImportancia == Importancia.Importante)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaNormal") as string && tarefa.NivelImportancia == Importancia.Normal)
                        listaTarefasFiltradas.Add(tarefa);
                    else if (importancia == Application.Current.FindResource("adicionarTarefaComboBoxImportanciaPrioritaria") as string && tarefa.NivelImportancia == Importancia.Prioritaria)
                        listaTarefasFiltradas.Add(tarefa);
                }
            }
        }
    }


    public List<int> GetListTarefasNavViewItemPeloNome(string nomeBotao)
    {
        foreach (NavigationViewItem botao in listaItensMenu)
        {
            if (botao.Content.ToString() == nomeBotao)
            {
                return ((ItemData)botao.Tag).IdsTarefas;
            }
        }
        return null;
    }

    public void SaveToXML(string filePath, bool modoDefault)
    {
        // guarda todos os dados do utilizador rm ficheiro XML (fica de fora apenas os tipos de alterações físicas - ficheiro à parte)
        /*
        XDocument doc = new XDocument(new XElement("dados")); // "dados" é o nó raiz do documento

        // Adiciona informações do perfil
        doc.Element("dados").Add(perfil.ToXML()); // Supondo que 'perfil' é uma instância da classe 'Perfil'
        if(modoDefault)
            doc.Element("dados").Element("perfil").Add(new XElement("modoCores", "Branco"));
        else
            doc.Element("dados").Element("perfil").Add(new XElement("modoCores", "Preto"));

        doc.Element("dados").Element("perfil").Add(new XElement("language", linguaApp));


        // Adiciona informações das tarefas
        XElement tarefasElement = new XElement("tarefas");
        foreach (Tarefa tarefa in
                 listaTarefas) // Supondo que 'listaTarefas' seja uma lista de instâncias da classe 'Tarefa'
        {
            tarefasElement.Add(tarefa.ToXML());
        }

        XElement itemDatasElement = new XElement("itemDatas");
        foreach (NavigationViewItem viewItem in listaItensMenu) // 'listaItemDatas' é uma lista de instâncias da classe 'ItemData'
        {
            ItemData itemData = (ItemData)viewItem.Tag;
            itemDatasElement.Add(itemData.ToXML());
        }

        doc.Element("dados").Add(tarefasElement);
        doc.Element("dados").Add(itemDatasElement);

        // Adiciona informações de outras classes se necessário...

        doc.Save(System.IO.Path.Combine(caminhoDadosArmazenados, "dados.xml"));*/
    }
    public ObservableCollection<Tarefa> GetTarefasByUtilizador(int utilizadorId)
    {
        var tarefas = new ObservableCollection<Tarefa>();

        try
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT t.ID, t.titulo, t.descricao, t.dataInicio, t.dataFim, t.horaInicio, t.horaFim, t.NivelImportancia, t.Estado
                    FROM tarefa t
                    INNER JOIN Possuir p ON t.ID = p.ID_tarefa
                    WHERE p.ID_utilizador = @utilizadorId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@utilizadorId", utilizadorId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dataInicio = reader.GetDateTime(3);
                            DateTime dataFim = reader.GetDateTime(4);
                            TimeOnly horaInicio = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5));
                            TimeOnly horaFim = TimeOnly.FromTimeSpan(reader.GetTimeSpan(6));

                            DateTime dataHoraInicio = dataInicio.Add(horaInicio.ToTimeSpan());
                            DateTime dataHoraFim = dataFim.Add(horaFim.ToTimeSpan());

                            var tarefa = new Tarefa(reader.GetInt32(0))
                            {
                                Titulo = reader.GetString(1),
                                Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
                                DataInicio = dataHoraInicio,
                                DataFim = dataHoraFim,
                                HoraInicio = horaInicio,
                                HoraFim = horaFim,
                                NivelImportancia = (Importancia)reader.GetInt32(7),
                                Estado = (Estado)reader.GetInt32(8),
                                listaDiaTarefas = new ObservableCollection<DiaTarefa>(),
                                Periodicidade = GetPeriodicidadeByTarefaId(reader.GetInt32(0), conn)
                            };

                            tarefa.listaDiaTarefas = GetDiaTarefasByTarefaId(tarefa.Id, conn);
                            var alertas = GetAlertasByTarefaId(tarefa.Id, conn);
                            foreach (var alerta in alertas)
                            {
                                if (alerta.Mensagem.Contains("Alerta de Antecipacao"))
                                {
                                    tarefa.AlertaAntecipacao.Add(alerta);
                                }
                                else
                                {
                                    tarefa.AlertaExecucao.Add(alerta);
                                }
                            }
                            tarefas.Add(tarefa);
                        }
                    }
                }
            }

            return tarefas;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
    private Periodicidade GetPeriodicidadeByTarefaId(int tarefaId, SqlConnection conn)
    {
        Periodicidade periodicidade = null;

        string query = @"
                SELECT p.ID, p.diasSemana, p.tipo
                FROM periodicidade p
                INNER JOIN Tarefa_Periodicidade tp ON p.ID = tp.ID_periodicidade
                WHERE tp.ID_tarefa = @tarefaId";

        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@tarefaId", tarefaId);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    periodicidade = new Periodicidade(reader.GetInt32(0))
                    {
                        DiasSemana = Periodicidade.ParseDiasSemana(reader.GetString(1)),
                        Tipo = (TipoPeriodicidade)reader.GetInt32(2)
                    };
                }
            }
        }

        return periodicidade;
    }
    private ObservableCollection<Alerta> GetAlertasByTarefaId(int tarefaId, SqlConnection conn)
    {
        var alertas = new ObservableCollection<Alerta>();

        string query = @"
                SELECT a.ID, a.mensagem, a.data_hora, a.desligado, a.tipoEmail, a.tipoWindows
                FROM alerta a
                INNER JOIN Tarefa_Alerta ta ON a.ID = ta.ID_alerta
                WHERE ta.ID_tarefa = @tarefaId";

        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@tarefaId", tarefaId);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var alerta = new Alerta(reader.GetInt32(0))
                    {
                        Mensagem = reader.GetString(1),
                        Data_Hora = reader.GetDateTime(2),
                        Desligado = reader.GetBoolean(3),
                    };
                    if (reader.GetBoolean(4) == true)
                        alerta.Tipos.Add(TipoAlerta.Email);
                    if (reader.GetBoolean(5) == true)
                        alerta.Tipos.Add(TipoAlerta.AlertaWindows);

                    alertas.Add(alerta);
                }
            }
        }

        return alertas;
    }

    private ObservableCollection<DiaTarefa> GetDiaTarefasByTarefaId(int tarefaId, SqlConnection conn)
    {
        try
        {
            var diaTarefas = new ObservableCollection<DiaTarefa>();

            string query = @"
                SELECT dt.ID, dt.dataDia, dt.ativo
                FROM diaTarefa dt
                INNER JOIN Tarefa_DiaTarefa tdt ON dt.ID = tdt.ID_diaTarefa
                WHERE tdt.ID_tarefa = @tarefaId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tarefaId", tarefaId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var diaTarefa = new DiaTarefa(reader.GetInt32(0), DateOnly.FromDateTime(reader.GetDateTime(1)),
                            reader.GetBoolean(2));
                        diaTarefas.Add(diaTarefa);
                    }
                }
            }

            return diaTarefas;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public void LoadFromBD(string nomeFicheiro)
    {
        LoadTarefasFromBD(perfil.Id);
        LoadBotoesPersonalizadosBD(perfil.Id);
    }

    public void AdicionarBotaoPersonalizadoBD(string nomeBotaoPersonalizado)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        int idListaPersonalizada = GetNextListaPersonalizadaId();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                INSERT INTO listaPersonalizada (nomeLista)
                VALUES (@nomeLista)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nomeLista", nomeBotaoPersonalizado);

                cmd.ExecuteNonQuery();
            }
        }

        //adicionar a conter o botao personalizado

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                INSERT INTO Conter (ID_utilizador, ID_listaPersonalizada)
                VALUES (@idU, @idL)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idU", perfil.Id);
                cmd.Parameters.AddWithValue("@idL", idListaPersonalizada);

                cmd.ExecuteNonQuery();
            }
        }
    }
    public void AdicionarTarefaBotaoListaPersonalizada(int idTarefa, string nomeBotao)
    {
        // Conexão com a base de dados
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Primeiro, procurar o ID do botão personalizado com base no nome do botão e no ID do usuário
            int idBotaoPersonalizado;
            string queryGetIdBotao = @"
            SELECT lp.ID
            FROM listaPersonalizada lp
            INNER JOIN Conter c ON lp.ID = c.ID_listaPersonalizada
            WHERE lp.nomeLista = @nomeBotao AND c.ID_utilizador = @userId";

            using (SqlCommand cmdGetIdBotao = new SqlCommand(queryGetIdBotao, conn))
            {
                cmdGetIdBotao.Parameters.AddWithValue("@nomeBotao", nomeBotao);
                cmdGetIdBotao.Parameters.AddWithValue("@userId", perfil.Id);
                idBotaoPersonalizado = (int?)cmdGetIdBotao.ExecuteScalar() ?? -1;
            }

            if (idBotaoPersonalizado == -1)
            {
                // Se não encontrou o botão personalizado, sair da função
                throw new Exception($"Botão personalizado com o nome '{nomeBotao}' não encontrado para o usuário com ID {perfil.Id}.");
            }

            // Em seguida, adicionar a tarefa ao botão personalizado
            string queryInsert = @"
            INSERT INTO ListaPersonalizada_Tarefa (ID_tarefa, ID_listaPersonalizada)
            VALUES (@idTarefa, @idBotaoPersonalizado)";

            using (SqlCommand cmdInsert = new SqlCommand(queryInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@idTarefa", idTarefa);
                cmdInsert.Parameters.AddWithValue("@idBotaoPersonalizado", idBotaoPersonalizado);

                cmdInsert.ExecuteNonQuery();
            }
        }
    }



    public void RemoverBotaoPersonalizadoBD(int idBotao)
    {
        // Remover botão personalizado da base de dados
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        // Remover associação entre botão personalizado e tarefas
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                DELETE FROM ListaPersonalizada_Tarefa
                WHERE ID_listaPersonalizada = @idB";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idB", idBotao);

                cmd.ExecuteNonQuery();
            }
        }

        //remover associacao entre utilizador e botao personalizado

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                DELETE FROM Conter
                WHERE ID_listaPersonalizada = @idB";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idB", idBotao);

                cmd.ExecuteNonQuery();
            }
        }

        // Remover botão personalizado
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                DELETE FROM listaPersonalizada
                WHERE ID = @idB";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idB", idBotao);

                cmd.ExecuteNonQuery();
            }
        }

    }

    public void EditarBotaoPersonalizadoNomeBD(int idBotao, string nomeBotaoPersonalizado)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                UPDATE listaPersonalizada
                SET nomeLista = @nomeLista
                WHERE ID = @idB";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nomeLista", nomeBotaoPersonalizado);
                cmd.Parameters.AddWithValue("@idB", idBotao);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void EliminarTarefaIdBotaoPersonalizadoBD(int idTarefa, int idBotao)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
                DELETE FROM ListaPersonalizada_Tarefa
                WHERE ID_tarefa = @idTarefa AND ID_listaPersonalizada = @idBotao";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idTarefa", idTarefa);
                cmd.Parameters.AddWithValue("@idBotao", idBotao);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void LoadBotoesPersonalizadosBD(int userId)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Carregar listas personalizadas associadas ao utilizador
            string queryListas = @"
            SELECT lp.ID, lp.nomeLista
            FROM listaPersonalizada lp
            INNER JOIN Conter c ON lp.ID = c.ID_listaPersonalizada
            WHERE c.ID_utilizador = @userId";

            using (SqlCommand cmdListas = new SqlCommand(queryListas, conn))
            {
                cmdListas.Parameters.AddWithValue("@userId", userId);

                using (SqlDataReader readerListas = cmdListas.ExecuteReader())
                {
                    while (readerListas.Read())
                    {
                        int listaId = readerListas.GetInt32(0);
                        string nomeLista = readerListas.GetString(1);

                        ItemData itemData = new ItemData(listaId)
                        {
                            Name = nomeLista,
                            IdsTarefas = new List<int>()
                        };

                        // Carregar IDs de tarefas associadas a esta lista personalizada
                        string queryTarefas = @"
                        SELECT ID_tarefa
                        FROM ListaPersonalizada_Tarefa
                        WHERE ID_listaPersonalizada = @listaId";

                        using (SqlCommand cmdTarefas = new SqlCommand(queryTarefas, conn))
                        {
                            cmdTarefas.Parameters.AddWithValue("@listaId", listaId);

                            using (SqlDataReader readerTarefas = cmdTarefas.ExecuteReader())
                            {
                                while (readerTarefas.Read())
                                {
                                    int tarefaId = readerTarefas.GetInt32(0);
                                    itemData.IdsTarefas.Add(tarefaId);
                                }
                            }
                        }

                        ListaItemDatasLoad.Add(itemData);
                    }
                }
            }
        }
        LoadBotoesFeito?.Invoke();
    }


    private ObservableCollection<DiaTarefa> LoadDiasFromXML(XElement diasElement)
    {
        ObservableCollection<DiaTarefa> dias = new ObservableCollection<DiaTarefa>();
        foreach (XElement diaElement in diasElement.Elements("diaTarefa"))
        {
            DiaTarefa dia = new DiaTarefa(GetNextDiaTarefaId(),
                DateOnly.Parse(diaElement.Element("data").Value),
                bool.Parse(diaElement.Element("ativo").Value)
            );
            dias.Add(dia);
        }
        return dias;
    }


    private void LoadTarefasFromBD(int idUser)
    {
        foreach (Tarefa tarefa in GetTarefasByUtilizador(idUser))
        {
            AdicionarElementoLista(tarefa);
        }
    }


    public void EditarAlerta(Alerta alerta)
    {
        foreach (Tarefa tarefa in listaTarefas)
        {
            foreach (Alerta alertaAntecipacao in tarefa.AlertaAntecipacao)
            {
                if (alertaAntecipacao.Id == alerta.Id)
                {
                    alertaAntecipacao.Desligado = alerta.Desligado;
                    alertaAntecipacao.Data_Hora = alerta.Data_Hora;
                    alertaAntecipacao.Mensagem = alerta.Mensagem;
                    alertaAntecipacao.Tipos = alerta.Tipos;
                    tarefa.AtualizarAlertasTodos();
                    break;
                }
            }

            foreach (Alerta alertaExecucao in tarefa.AlertaExecucao)
            {
                if (alertaExecucao.Id == alerta.Id)
                {
                    alertaExecucao.Desligado = alerta.Desligado;
                    alertaExecucao.Data_Hora = alerta.Data_Hora;
                    alertaExecucao.Mensagem = alerta.Mensagem;
                    alertaExecucao.Tipos = alerta.Tipos;
                    tarefa.AtualizarAlertasTodos();
                    break;
                }
            }
        }

        foreach (var elementoAlerta in listaAlertas)
        {
            if (elementoAlerta.Id == alerta.Id)
            {
                listaAlertas.Remove(elementoAlerta); //remover nesse index o elemento
                listaAlertas.Add(alerta);
                break;
            }
        }

        EditarAlertaBD(alerta);

        ListaAlertasAlterada?.Invoke();
        AlertaEditado?.Invoke();
    }


    //BOTAO LISTA PERSONALIZADA

    //adiciona um botao com o nome "nomeBotao" à lista de botoes
    public void AdicionarNavigationViewItem(NavigationViewItem botaoItem)
    {
        listaItensMenu.Add(botaoItem);
        //verificar se  o botao ja existe na base de dados para aquele utilizador

        if (!BotaoExisteParaUsuario(((ItemData)(botaoItem.Tag)).Name, perfil.Id))
        {
            AdicionarBotaoPersonalizadoBD(((ItemData)(botaoItem.Tag)).Name);
        }

        ListaNavigationViewItemsAtualizada?.Invoke();
    }
    private bool BotaoExisteParaUsuario(string nomeBotao, int userId)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = @"
            SELECT COUNT(*)
            FROM listaPersonalizada lp
            INNER JOIN Conter c ON lp.ID = c.ID_listaPersonalizada
            WHERE lp.nomeLista = @nomeBotao AND c.ID_utilizador = @userId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nomeBotao", nomeBotao);
                cmd.Parameters.AddWithValue("@userId", userId);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }

    //adiciona ao botao "nomeBotao" a tarefa com id "idTarefa"
    public void AdicionarTarefaABotaoListaPersonalizada(int idTarefa, string nomeBotao)
    {
        foreach (NavigationViewItem botao in listaItensMenu)
        {
            if (botao.Content.ToString() == nomeBotao)
            {
                ((ItemData)botao.Tag).IdsTarefas.Add(idTarefa);
                AdicionarTarefaBotaoListaPersonalizada(idTarefa, nomeBotao);
                break;
            }
        }
    }

    //atualiza a listaFiltradaTarefas com as tarefas associadas a propriedade Tag do botao com o nome "nomeBotao"
    public void AtualizarListaFiltradaBotaoComListaPersonalizada(string nomeBotao)
    {
        listaTarefasFiltradas.Clear();
        List<int> listaIdsTarefas = GetListTarefasNavViewItemPeloNome(nomeBotao);
        if(listaIdsTarefas != null)
        {
            foreach (var tarefa in listaTarefas)
            {
                if (listaIdsTarefas.Contains(tarefa.Id))
                {
                    listaTarefasFiltradas.Add(tarefa);
                }
            }
        }
    }

    //remove o botao com o nome "nomeBotao" da lista de botoes
    public void ApagarBotaoComListaPersonalizada(string nomeBotao)
    {
        foreach (NavigationViewItem botao in listaItensMenu)
        {
            if (botao.Content.ToString() == nomeBotao)
            {
                listaItensMenu.Remove(botao);
                RemoverBotaoPersonalizadoBD(((ItemData)botao.Tag).IdNavigationItem);
                break;
            }
        }
        ListaNavigationViewItemRemovido?.Invoke();
        ListaNavigationViewItemsAtualizada?.Invoke();
    }

    public bool ExisteBotaoComNome(string nomeBotao)
    {
        foreach (NavigationViewItem botao in listaItensMenu)
        {
            if (botao.Content.ToString() == nomeBotao.Trim())
            {
                return true;
            }
        }
        string nomeSemEspacos = nomeBotao.Trim();
        switch (nomeBotao.Trim())
        {
            case "Tarefas":
            case "Hoje":
            case "Esta Semana":
            case "Pouco Importante":
            case "Importante":
            case "Normal":
            case "Prioritaria":
                return true;
        }
        return false;
    }
    //atualiza o nome do botao com o nome antigo "nomeAntigo" para o nome novo "nomeNovo"
    public void AtualizarNomeBotaoComListaPersonalizada(string nomeAntigo, string nomeNovo)
    {
        foreach (NavigationViewItem botao in listaItensMenu)
        {
            if (botao.Content.ToString() == nomeAntigo.Trim())
            {
                botao.Content = nomeNovo;
                ((ItemData)botao.Tag).Name = nomeNovo;
                EditarBotaoPersonalizadoNomeBD(((ItemData)botao.Tag).IdNavigationItem, nomeNovo);
                break;
            }
        }
        ListaNavigationViewItemsAtualizada?.Invoke();
    }

    //FILTROS TAREFAS BOTAO PERSONALIZADO

    //verifica se a tarefa tem algum dia unchecked entre da dataInicio e dataFim da tarefa,  se tiver, tem que se repetir
    //mas se ja tiver terminada, nao vai aparecer
    private bool TarefaVaiOcorrer(Tarefa tarefa, DateOnly dataInicio, DateOnly dataFim)
    {
        if (tarefa.Estado != Estado.PorTerminar)
        {
            foreach (DiaTarefa diaTarefa in tarefa.listaDiaTarefas)
            {
                if (diaTarefa.Data >= dataInicio && diaTarefa.Data <= dataFim && diaTarefa.ativo == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //filtra as tarefas que ocorrem entre duas datas e estao no botao com o nome "nomeBotao"
    public void FiltrarTarefasEntreDatasBotaoPersonalizado(DateOnly dataInicio, DateOnly dataFim, string nomeBotao)
    {
        listaTarefasFiltradas.Clear();
        List<int> listaTarefasIds = GetListTarefasNavViewItemPeloNome(nomeBotao);

        foreach (var tarefa in listaTarefas)
        {
            if (listaTarefasIds.Contains(tarefa.Id) && (TarefaVaiOcorrer(tarefa, dataInicio, dataFim)))
            {
                listaTarefasFiltradas.Add(tarefa);
            }
        }

    }

    public void AtualizarDiaTarefaBD(int IDdiaTarefa, bool valor)
    {
        try
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["ToDoListConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE diaTarefa
                    SET ativo = @valor
                    WHERE ID = @IDdiaTarefa";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@valor", valor);
                    cmd.Parameters.AddWithValue("@IDdiaTarefa", IDdiaTarefa);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public void EditarDiaTarefas(DiaTarefa diaTarefa, bool valor = false)
    {
        Tarefa? tarefaAEditar = null;
        foreach (Tarefa tarefa in listaTarefas) //encontra a tarefa que tem o diaTarefa a editar
        {
            foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
            {
                if (dia.Id == diaTarefa.Id)
                {
                    tarefaAEditar = tarefa;
                    AtualizarDiaTarefaBD(diaTarefa.Id, valor);
                    break;
                }
            }
        }

        //se quiser dar check ao dia
        if (tarefaAEditar != null && valor == false) //se for para desativar, tem que se remover da lista
        {
            //se a hora de inicio < horafim
            if (tarefaAEditar.DataInicio.TimeOfDay < tarefaAEditar.DataFim.TimeOfDay)
            {
                foreach (Alerta alertaExec in tarefaAEditar.AlertaExecucao)
                {
                    //se o alerta de execucao for para o dia que foi checkado e a hora de execucao for depois da hora de fim da tarefa
                    if (DateOnly.FromDateTime(alertaExec.Data_Hora) == diaTarefa.Data && TimeOnly.FromDateTime(alertaExec.Data_Hora) >= TimeOnly.FromDateTime(tarefaAEditar.DataFim))
                    {
                        listaAlertas.Remove(alertaExec);
                        tarefaAEditar.AlertaExecucao.Remove(alertaExec);
                        EliminarAlertaBD(alertaExec.Id);
                        break;
                    }
                }

                foreach (Alerta alertaAntecipacao in tarefaAEditar.AlertaAntecipacao )
                {
                    //se o alerta de antecipacao for para o dia que foi checkado e a hora de execucao for antes da hora de inicio da tarefa
                    //temos que verificar se o alerta em questao está entre ontem desde a hora de inicio da tarefa até hoje à hora de inicio da tarefa
                    if (alertaAntecipacao.Data_Hora <= diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataInicio))
                        && alertaAntecipacao.Data_Hora > diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataInicio)).AddDays(-1))
                    {
                        listaAlertas.Remove(alertaAntecipacao);
                        tarefaAEditar.AlertaAntecipacao.Remove(alertaAntecipacao);
                        EliminarAlertaBD(alertaAntecipacao.Id);
                        break;
                    }
                }
                if (diaTarefa.Data == DateOnly.FromDateTime(DateTime.Today))
                {
                    tarefaAEditar.Estado = Estado.PorComecar;
                }
            }
            else
            {
                //se a hora de inicio > horafim
                //remove o alerta de antecipacao de hoje e o alerta de nao realizacao correspondente de amanha
                foreach (Alerta alertaExec in tarefaAEditar.AlertaExecucao)
                {
                    if (DateOnly.FromDateTime(alertaExec.Data_Hora) == diaTarefa.Data.AddDays(1))
                    {
                        listaAlertas.Remove(alertaExec);
                        tarefaAEditar.AlertaExecucao.Remove(alertaExec);
                        EliminarAlertaBD(alertaExec.Id);
                        break;
                    }
                }

                foreach (Alerta alertaAntecipacao in tarefaAEditar.AlertaAntecipacao)
                {
                    if (DateOnly.FromDateTime(alertaAntecipacao.Data_Hora) == diaTarefa.Data)
                    {
                        listaAlertas.Remove(alertaAntecipacao);
                        tarefaAEditar.AlertaAntecipacao.Remove(alertaAntecipacao);
                        EliminarAlertaBD(alertaAntecipacao.Id);
                        break;
                    }
                }
                if (diaTarefa.Data == DateOnly.FromDateTime(DateTime.Today))
                {
                    tarefaAEditar.Estado = Estado.PorComecar;
                }
            }
        }
        else if (tarefaAEditar != null && valor == true) //quer descheckar o dia, entao temos que gerar os alertas
        {
            TimeOnly time;
            DateTime dataHoraAlerta;
            bool found = false;

            if (tarefaAEditar.DataInicio.TimeOfDay < tarefaAEditar.DataFim.TimeOfDay) //se a hora de inicio < horafim
            {
                time = TimeOnly.FromDateTime(tarefaAEditar.DataFim);
                dataHoraAlerta = diaTarefa.Data.ToDateTime(time);

                foreach (Alerta alertaExecucao in tarefaAEditar.AlertaExecucao)
                {
                    if (alertaExecucao.Data_Hora.AddMinutes(Convert.ToDouble(0.1)) == dataHoraAlerta)
                    {
                        found = true;
                        break;
                    }
                }
                //geramos os dois para o dia que foi checkado, se nao existirem ja la
                if (!found && dataHoraAlerta.AddMinutes(Convert.ToDouble(0.1)) >= DateTime.Now)
                {
                    tarefaAEditar.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                        , dataHoraAlerta.AddMinutes(Convert.ToDouble(0.1)), new List<TipoAlerta>(), desligado: false, GetNextAlertaId()));

                    tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                    tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1].Tipos
                        .Add(TipoAlerta.AlertaWindows);
                    AdicionarAlertaBD(tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1], tarefaAEditar.Id);
                }

                time = TimeOnly.FromDateTime(tarefaAEditar.DataInicio);
                dataHoraAlerta = diaTarefa.Data.ToDateTime(time);

                foreach (Alerta alertaAntecipacao in tarefaAEditar.AlertaAntecipacao)
                {
                    if (alertaAntecipacao.Data_Hora.AddMinutes(-Convert.ToInt32(5)) == dataHoraAlerta)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found && dataHoraAlerta.AddMinutes(-Convert.ToInt32(5)) >= DateTime.Now)
                {
                    tarefaAEditar.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                        , dataHoraAlerta.AddMinutes(-Convert.ToInt32(5)),
                        new List<TipoAlerta>(),
                        desligado: false, GetNextAlertaId()));
                    tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaAntecipacao.Count - 1].Tipos
                        .Add(TipoAlerta.Email);
                    tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaAntecipacao.Count - 1].Tipos
                        .Add(TipoAlerta.AlertaWindows);

                    AdicionarAlertaBD(tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaExecucao.Count - 1], tarefaAEditar.Id);
                }
                if (diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataInicio)) <= DateTime.Now && diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataFim)).AddSeconds(-30) > DateTime.Now)
                {
                    tarefaAEditar.Estado = Estado.EmExecucao;
                }
                else
                {
                    tarefaAEditar.Estado = Estado.PorComecar;
                }
            }
            else
            {
                time = TimeOnly.FromDateTime(tarefaAEditar.DataFim);
                dataHoraAlerta = diaTarefa.Data.ToDateTime(time);
                bool found1 = false;

                foreach (Alerta alertaExecucao in tarefaAEditar.AlertaExecucao)
                {
                    if (alertaExecucao.Data_Hora.AddMinutes(Convert.ToDouble(0.1)) == dataHoraAlerta)
                    {
                        found1 = true;
                        break;
                    }
                }

                if (!found1 && dataHoraAlerta.AddMinutes(Convert.ToDouble(0.1)).AddDays(1) >= DateTime.Now)
                {
                    tarefaAEditar.AlertaExecucao.Add(new Alerta("Alerta de Nao Realizacao"
                        , dataHoraAlerta.AddMinutes(Convert.ToDouble(0.1)).AddDays(1), new List<TipoAlerta>(),
                        desligado: false, GetNextAlertaId()));

                    tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1].Tipos.Add(TipoAlerta.Email);
                    tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1].Tipos
                        .Add(TipoAlerta.AlertaWindows);

                    AdicionarAlertaBD(tarefaAEditar.AlertaExecucao[tarefaAEditar.AlertaExecucao.Count - 1], tarefaAEditar.Id);
                }

                time = TimeOnly.FromDateTime(tarefaAEditar.DataInicio);
                dataHoraAlerta = diaTarefa.Data.ToDateTime(time);

                foreach (Alerta alertaAntecipacao in tarefaAEditar.AlertaAntecipacao)
                {
                    if (alertaAntecipacao.Data_Hora.AddMinutes(-Convert.ToInt32(5)) == dataHoraAlerta)
                    {
                        found1 = true;
                        break;
                    }
                }

                if (!found1  && dataHoraAlerta.AddMinutes(-Convert.ToInt32(5)) >= DateTime.Now)
                {
                    tarefaAEditar.AlertaAntecipacao.Add(new Alerta("Alerta de Antecipacao"
                        , dataHoraAlerta.AddMinutes(-Convert.ToInt32(5)),
                        new List<TipoAlerta>(),
                        desligado: false, GetNextAlertaId()));
                    tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaAntecipacao.Count - 1].Tipos
                        .Add(TipoAlerta.Email);
                    tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaAntecipacao.Count - 1].Tipos
                        .Add(TipoAlerta.AlertaWindows);

                    AdicionarAlertaBD(tarefaAEditar.AlertaAntecipacao[tarefaAEditar.AlertaExecucao.Count - 1], tarefaAEditar.Id);
                }
                if (diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataInicio)) <= DateTime.Now && diaTarefa.Data.ToDateTime(TimeOnly.FromDateTime(tarefaAEditar.DataFim)).AddDays(1).AddSeconds(-30) >= DateTime.Now)
                {
                    tarefaAEditar.Estado = Estado.EmExecucao;
                }
                else
                {
                    tarefaAEditar.Estado = Estado.PorComecar;
                }
            }


        }

        if (tarefaAEditar != null) //se a tarefa foi encontrada vamos editar o diaTarefa e atualizar a tarefa
        {
            foreach (DiaTarefa dia in tarefaAEditar.listaDiaTarefas)
            {
                if (dia.Id == diaTarefa.Id)
                {
                    dia.ativo = valor;
                    EditarElementoLista(tarefaAEditar);
                    break;
                }
            }
        }

        ListaAlertasAlterada?.Invoke();
        AlertaRemovido?.Invoke();
    }

    //NOTIFICACOES
    public void EmailSender()
    {
        string MessageBody;
        try
        {
            MailMessage mailMessage = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

            DateTime dataHoraAlerta = GetTarefaPeloAlerta(listaAlertas.First()).DataInicio;
            DateTime horaInicioTarefa = GetTarefaPeloAlerta(listaAlertas.First()).DataInicio;

            DateTime novaDataHora = new DateTime(
                dataHoraAlerta.Year,
                dataHoraAlerta.Month,
                dataHoraAlerta.Day,
                horaInicioTarefa.Hour,
                horaInicioTarefa.Minute,
                0 // Segundos
            );

            mailMessage.From = new MailAddress("grupotrabalho482@gmail.com", "BotAlertas"); //enviar
            mailMessage.To.Add(perfil.Email); //receber
            mailMessage.Subject = "Nova Notificação!";
            mailMessage.IsBodyHtml = true;/*
            if (listaAlertas.First().Mensagem != "Alerta de Antecipacao")
            {
                MessageBody =
                    "<!DOCTYPE html>\r\n<html lang=\"pt-br\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>Aviso de Não Realização</title>\r\n<style>\r\n  body {{\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f5f5f5;\r\n    color: #333;\r\n    margin: 0;\r\n    padding: 0;\r\n  }}\r\n  .container {{\r\n    max-width: 600px;\r\n    margin: 20px auto;\r\n    padding: 20px;\r\n    background-color: #fff;\r\n    border-radius: 8px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n  }}\r\n  h1 {{\r\n    color: #007bff;\r\n    text-align: center;\r\n  }}\r\n  p {{\r\n    margin-bottom: 15px;\r\n    line-height: 1.6;\r\n  }}\r\n  .footer {{\r\n    margin-top: 30px;\r\n    font-size: 12px;\r\n    color: #777;\r\n    text-align: center;\r\n  }}\r\n</style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    <h1>Atenção, {0}!</h1>\r\n    <p>Este é um aviso automático enviado pelo <strong>BotAlerta</strong>, da sua aplicação <strong>TodoList</strong>.</p>\r\n    <p style=\"color: #dc3545;\"><strong>A Tarefa '{1}' excedeu o tempo programado para sua a conclusão.</strong></p>\r\n    <p>Por favor, tome as medidas necessárias para concluir esta tarefa o mais rápido possível ou ajuste o prazo, se necessário.</p>\r\n    <p>Se precisar de ajuda ou tiver alguma dúvida, não hesite em entrar em contato conosco.</p>\r\n    <p class=\"footer\">Atenciosamente,<br>TodoList</p>\r\n  </div>\r\n</body>\r\n</html>\r\n";
                mailMessage.Body = string.Format(MessageBody, perfil.Nome,
                    GetTarefaPeloAlerta(listaAlertas.First()).Titulo);
            }
            else
            {
                MessageBody =
                    "<!DOCTYPE html>\r\n<html lang=\"pt-br\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>Aviso de Antecipação</title>\r\n<style>\r\n  body {{\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f5f5f5;\r\n    color: #333;\r\n    margin: 0;\r\n    padding: 0;\r\n  }}\r\n  .container {{\r\n    max-width: 600px;\r\n    margin: 20px auto;\r\n    padding: 20px;\r\n    background-color: #fff;\r\n    border-radius: 8px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n  }}\r\n  h1 {{\r\n    color: #007bff;\r\n    text-align: center;\r\n  }}\r\n  p {{\r\n    margin-bottom: 15px;\r\n    line-height: 1.6;\r\n  }}\r\n  .footer {{\r\n    margin-top: 30px;\r\n    font-size: 12px;\r\n    color: #777;\r\n    text-align: center;\r\n  }}\r\n</style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    <h1>Atenção, {0}!</h1>\r\n    <p>Este é um aviso automático enviado pelo <strong>BotAlerta</strong>, da sua aplicação <strong>TodoList</strong>.</p>\r\n    <p style=\"color: #dc3545;\"><strong>A Tarefa '{1}' irá começar a {2}.</strong></p>\r\n    <p>Fique atento à data e hora de início da Tarefa '{1}' para garantir que esteja pronto para começar no horário especificado.</p>\r\n    <p>Se precisar de ajuda ou tiver alguma dúvida, não hesite em entrar em contato conosco.</p>\r\n    <p class=\"footer\">Atenciosamente,<br>TodoList</p>\r\n  </div>\r\n</body>\r\n</html>\r\n";
                mailMessage.Body = string.Format(MessageBody, perfil.Nome,
                    GetTarefaPeloAlerta(listaAlertas.First()).Titulo, novaDataHora.ToString());
            }*/
            if (listaAlertas.First().Mensagem != "Alerta de Antecipacao")
            {
                MessageBody =
                    "<!DOCTYPE html>\r\n<html lang=\"pt-br\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>{2}</title>\r\n<style>\r\n  body {{\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f5f5f5;\r\n    color: #333;\r\n    margin: 0;\r\n    padding: 0;\r\n  }}\r\n  .container {{\r\n    max-width: 600px;\r\n    margin: 20px auto;\r\n    padding: 20px;\r\n    background-color: #fff;\r\n    border-radius: 8px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n  }}\r\n  h1 {{\r\n    color: #007bff;\r\n    text-align: center;\r\n  }}\r\n  p {{\r\n    margin-bottom: 15px;\r\n    line-height: 1.6;\r\n  }}\r\n  .footer {{\r\n    margin-top: 30px;\r\n    font-size: 12px;\r\n    color: #777;\r\n    text-align: center;\r\n  }}\r\n</style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    <h1>{3} {0}!</h1>\r\n    <p>{4} <strong>BotAlerta</strong>{5} <strong>TodoList</strong>.</p>\r\n    <p style=\"color: #dc3545;\"><strong>{6} '{1}' {7}</strong></p>\r\n    <p>{8}</p>\r\n    <p>{9}</p>\r\n    <p class=\"footer\">{10}<br>TodoList</p>\r\n  </div>\r\n</body>\r\n</html>\r\n";
                mailMessage.Body = string.Format(MessageBody, perfil.Nome,
                    GetTarefaPeloAlerta(listaAlertas.First()).Titulo, Application.Current.FindResource("Notificacao1R") as string, Application.Current.FindResource("Notificacao2R") as string, Application.Current.FindResource("Notificacao3R") as string,
                    Application.Current.FindResource("Notificacao4R") as string, Application.Current.FindResource("Notificacao5R") as string, Application.Current.FindResource("Notificacao6R") as string, Application.Current.FindResource("Notificacao7R") as string, Application.Current.FindResource("Notificacao8R") as string,
                    Application.Current.FindResource("Notificacao9R") as string);
            }
            else
            {
                MessageBody =
                    "<!DOCTYPE html>\r\n<html lang=\"pt-br\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<title>{3}</title>\r\n<style>\r\n  body {{\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f5f5f5;\r\n    color: #333;\r\n    margin: 0;\r\n    padding: 0;\r\n  }}\r\n  .container {{\r\n    max-width: 600px;\r\n    margin: 20px auto;\r\n    padding: 20px;\r\n    background-color: #fff;\r\n    border-radius: 8px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n  }}\r\n  h1 {{\r\n    color: #007bff;\r\n    text-align: center;\r\n  }}\r\n  p {{\r\n    margin-bottom: 15px;\r\n    line-height: 1.6;\r\n  }}\r\n  .footer {{\r\n    margin-top: 30px;\r\n    font-size: 12px;\r\n    color: #777;\r\n    text-align: center;\r\n  }}\r\n</style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    <h1>{4} {0}!</h1>\r\n    <p>{5} <strong>BotAlerta</strong>{6} <strong>TodoList</strong>.</p>\r\n    <p style=\"color: #dc3545;\"><strong>{7} '{1}' {8} {2}.</strong></p>\r\n    <p>{9} '{1}' {10}</p>\r\n    <p>{11}</p>\r\n    <p class=\"footer\">{12}<br>TodoList</p>\r\n  </div>\r\n</body>\r\n</html>\r\n";
                mailMessage.Body = string.Format(MessageBody, perfil.Nome,
                    GetTarefaPeloAlerta(listaAlertas.First()).Titulo, novaDataHora.ToString(), Application.Current.FindResource("Notificacao1A") as string, Application.Current.FindResource("Notificacao2A") as string, Application.Current.FindResource("Notificacao3A") as string,
                    Application.Current.FindResource("Notificacao4A") as string, Application.Current.FindResource("Notificacao5A") as string, Application.Current.FindResource("Notificacao6A") as string, Application.Current.FindResource("Notificacao7A") as string, Application.Current.FindResource("Notificacao8A") as string,
                    Application.Current.FindResource("Notificacao9A") as string, Application.Current.FindResource("Notificacao10A") as string);
            }

            smtpClient.Port = 587;
            smtpClient.Credentials =
                new System.Net.NetworkCredential("grupotrabalho482@gmail.com", "hzdr ydtw itoq rayx");
            smtpClient.EnableSsl = true;

            smtpClient.Send(mailMessage);
        }
        catch (Exception exception)
        {
            Wpf.Ui.Controls.MessageBox mg = new Wpf.Ui.Controls.MessageBox();
            mg.Title = (string)Application.Current.FindResource("ErroTempoAntecipacaoTitle") as string;
            mg.Content = (string)Application.Current.FindResource("ErroEnvioEmail") as string;
            mg.CloseButtonText = (string)Application.Current.FindResource("AlertasCloseButtonText") as string;
            mg.CloseButtonAppearance = ControlAppearance.Info;
            mg.ShowDialogAsync();
        }
    }

    public Tarefa? GetTarefaPeloId(int idTarefa)
    {
        foreach (Tarefa tarefa in listaTarefas)
        {
            if (tarefa.Id == idTarefa)
            {
                return tarefa;
            }
        }
        return null;
    }
    public void FinalizarTarefaDia(string idTarefa, string data)
    {
        Tarefa? tarefa = GetTarefaPeloId(Convert.ToInt32(idTarefa));
        if (tarefa != null)
        {
            if (tarefa.DataInicio.TimeOfDay < tarefa.DataFim.TimeOfDay)
            {
                foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
                {
                    if (dia.Data == DateOnly.FromDateTime(DateTime.Parse(data)))
                    {
                        EditarDiaTarefas(dia, false);
                        break;
                    }
                }
            }
            else
            {
                foreach (DiaTarefa dia in tarefa.listaDiaTarefas)
                {
                    if (dia.Data == DateOnly.FromDateTime(DateTime.Parse(data)).AddDays(-1))
                    {
                        EditarDiaTarefas(dia, false);
                        break;
                    }
                }
            }
        }
    }

    public void Notificacao(Alerta alerta, bool naoRealizacao = true)
    {
        try
        {
            if (naoRealizacao && GetTarefaPeloAlerta(alerta) != null)
            {
                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 1)
                    .AddArgument("tarefaId",
                        GetTarefaPeloAlerta(alerta).Id) // Adiciona o ID da tarefa como argumento no botão
                    .AddHeader("1", Application.Current.FindResource("adicionarTarefaAlertaDeNaoRealizacao") as string, "action=openConversation&id=1")
                    .AddText(Application.Current.FindResource("buttonTarefaName") as string + ": " + GetTarefaPeloAlerta(alerta).Titulo)
                    .AddText(Application.Current.FindResource("TarefaExcedeuTempo") as string)
                    .AddText(Application.Current.FindResource("TarefaDataHora") as string +" "+ new DateTime(DateOnly.FromDateTime(alerta.Data_Hora), TimeOnly.FromDateTime(GetTarefaPeloAlerta(alerta).DataFim)).ToString())
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"))
                    .AddButton(new ToastButton()
                        .SetContent(Application.Current.FindResource("AbrirToDoList") as string)
                        .AddArgument("action", "abrirApp")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent(Application.Current.FindResource("FinalizarTarefa") as string)
                        .AddArgument("action", "finalizarTarefa")
                        .AddArgument("tarefaId", GetTarefaPeloAlerta(alerta).Id)
                        .AddArgument("dataExecucao", alerta.Data_Hora.Date.ToShortDateString())
                        .SetBackgroundActivation())
                    .Show();
            }
            else if(GetTarefaPeloAlerta(alerta) != null)
            {
                // Obtém a data de início da tarefa associada ao alerta
                Tarefa tarefa = GetTarefaPeloAlerta(alerta);

                // Converte os DateTime para TimeOnly
                TimeOnly inicioTarefa = TimeOnly.FromDateTime(tarefa.DataInicio);
                TimeOnly alertaHora = TimeOnly.FromDateTime(alerta.Data_Hora);

                // Calcula a diferença entre os horários
                TimeSpan tempoDiferenca = inicioTarefa.ToTimeSpan() - alertaHora.ToTimeSpan();
                if(alerta.Data_Hora.Add(tempoDiferenca) == DateTime.Today) //para verificar se o alerta é de antecipacao de amanha
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 2)
                        .AddHeader("2", Application.Current.FindResource("adicionarTarefaAlertaAntecipacao") as string,
                            "action=openConversation&id=2")
                        .AddText(Application.Current.FindResource("buttonTarefaName") as string + ": " +
                                 GetTarefaPeloAlerta(alerta).Titulo)
                        .AddText(Application.Current.FindResource("TarefaComecarEm") as string +" " + new DateTime(DateOnly.FromDateTime(alerta.Data_Hora).AddDays(1),
                            TimeOnly.FromDateTime(alerta.Data_Hora)).Add(tempoDiferenca).ToString())
                        .AddAudio(new Uri("ms-winsoundevent:Notification.Default"))
                        .AddButton(new ToastButton()
                            .SetContent(Application.Current.FindResource("AbrirToDoList") as string)
                            .AddArgument("action", "abrirApp")
                            .SetBackgroundActivation())
                        .Show();
                }
                else //se nao for de amanha é de hoje
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 2)
                        .AddHeader("2", Application.Current.FindResource("adicionarTarefaAlertaAntecipacao") as string, "action=openConversation&id=2")
                        .AddText(Application.Current.FindResource("buttonTarefaName") as string + ": " + GetTarefaPeloAlerta(alerta).Titulo)
                        .AddText(Application.Current.FindResource("TarefaComecarEm") as string + " "+new DateTime(DateOnly.FromDateTime(alerta.Data_Hora), TimeOnly.FromDateTime(alerta.Data_Hora)).Add(tempoDiferenca).ToString())
                        .AddAudio(new Uri("ms-winsoundevent:Notification.Default"))
                        .AddButton(new ToastButton()
                            .SetContent(Application.Current.FindResource("AbrirToDoList") as string)
                            .AddArgument("action", "abrirApp")
                            .SetBackgroundActivation())
                        .Show();
                }


            }
        }
        catch (System.IO.FileNotFoundException)
        {
            MessageBox.Show("Ficheiro de som não encontrado!");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    public bool AreToastNotificationsEnabled()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\PushNotifications";
        const string valueName = "ToastEnabled";

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath))
        {
            if (key != null)
            {
                object value = key.GetValue(valueName);
                if (value != null)
                {
                    // Convertendo o valor do registro para um inteiro
                    // 1 significa ativado, 0 significa desativado
                    return Convert.ToInt32(value) == 1;
                }
            }
        }
        return false;  // Padrão se a chave ou valor não existir

    }
}