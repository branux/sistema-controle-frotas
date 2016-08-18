﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.Reporting.WebForms;
using System.Drawing.Printing;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using db_transporte_sanitario;

namespace WindowsFormsApplication2
{
    public partial class SelecionaAM : Form
    {
        string Sexo, Agendamento, TipoAM, pegamotivo, statusAM, tipoSolicitacao;
        string statusAMLista, NomeAM;
        int idAmbu, idPaciente, idSolicitacaoAm;
        string pegaUnidade;     //para pegar o telefone com o nome da unidade
        string pegaUnidadeEnd;  //para pegar o endereco com o nome da unidade
        string Endereco1;

        public SelecionaAM(int IDpaciente, int AMocupada, int idSoAm)
        {
            InitializeComponent();
            idPaciente = IDpaciente;
            LabelIDPaciente.Text = idPaciente.ToString();
            idAmbu = AMocupada;
            idSolicitacaoAm = idSoAm;
            PuxarEnderecos();
            PreencherCampos();
            verificaSeAMEstaIncluida();
            VerificarPacienteJaestaInclusoNaMesma();
            pegarDadosDasAmbulancias();
        }


        private void verificaSeAMEstaIncluida()
        {
            if(idAmbu == 0)
            {
                RetirarAM.Visible = false;
                BtnCancelar.Visible = false;
                return;
            }
            using(DAHUEEntities db = new DAHUEEntities())
            {
                var query = (from am in db.ambulancia
                             where am.idAmbulancia == idAmbu
                             select new { am.NomeAmbulancia, am.StatusAmbulancia }).FirstOrDefault();
                NomeAM = query.NomeAmbulancia;
                statusAM = query.StatusAmbulancia;
            }

            if (statusAM == "OCUPADA")
            {
                PainelAM2.Visible = false;
                label23.Visible = true;
                BtnOutraAM.Visible = true;
                BtnConfimar.Visible = true;
                label22.Visible = true;
                RetirarAM.Visible = true;
                label22.Text = NomeAM;

            }
            else if (statusAM == "DISPONIVEL")
            {
                PainelAM2.Visible = false;
                label23.Visible = true;
                BtnOutraAM.Visible = true;
                BtnConfimar.Visible = true;
                label22.Visible = true;
                RetirarAM.Visible = false;
                label22.Text = NomeAM;
            }

        }

        private void VerificarPacienteJaestaInclusoNaMesma()
        {
            using(DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sa in db.solicitacoes_ambulancias
                            where sa.idSolicitacoesPacientes == idPaciente && sa.idAmbulanciaSol == idAmbu
                                select sa;
                int newProdID = query.Count();
                if (newProdID >= 1)
                {
                    PainelAM2.Visible = false;
                    label23.Visible = false;
                    BtnOutraAM.Visible = false;
                    BtnConfimar.Visible = false;
                    label22.Visible = false;
                    label22.Text = NomeAM;
                }
            }

        }

        private void PreencherCampos()
        {
            //buscar informacoes pelo id da tabela
            using(DAHUEEntities db = new DAHUEEntities())
            {
                var query = (from sp in db.solicitacoes_paciente
                            where sp.idPaciente_Solicitacoes == idPaciente
                            select sp).FirstOrDefault();
            
                    if (query.TipoSolicitacao == "Avancada")
                    {
                        BtnBasica.Visible = false;
                        BtnAvancada.BackColor = Color.Teal;
                        BtnAvancada.ForeColor = Color.PaleTurquoise;
                    }
                    else
                    {
                        BtnAvancada.Visible = false;
                        BtnBasica.BackColor = Color.Teal;
                        BtnBasica.ForeColor = Color.PaleTurquoise;
                    }

                    if (query.Agendamento == "Sim")
                    {
                        Btnagendanao.Visible = false;
                        label3.Visible = true;
                        txtAtendMarcado.Visible = true;
                        Btnagendasim.BackColor = Color.Teal;
                        Btnagendasim.ForeColor = Color.PaleTurquoise;
                    }
                    else
                    {
                        Btnagendasim.Visible = false;
                        label3.Visible = false;
                        txtAtendMarcado.Visible = false;
                        Btnagendanao.BackColor = Color.Teal;
                        Btnagendanao.ForeColor = Color.PaleTurquoise;
                    }

                        txtAtendMarcado.Text = query.DtHrAgendamento;
                        txtNomeSolicitante.Text = query.NomeSolicitante;
                        CbLocalSolicita.Text = query.LocalSolicitacao;
                        txtTelefone.Text = query.Telefone;
                        txtNomePaciente.Text = query.Paciente;

                    if (query.Genero == "F")
                    {
                        RbFemenino.Checked = true;
                    }
                    else
                    {
                        RbMasculino.Checked = true;
                    }
                        txtIdade.Text = query.Idade;
                        txtDiagnostico.Text = query.Diagnostico;
                        CbMotivoChamado.Text = query.Motivo;
                        CbTipoMotivoSelecionado.Text = query.SubMotivo;
                        PrioridadeTxt.Text = query.Prioridade;
                        CbOrigem.Text = query.Origem;
                        txtEnderecoOrigem.Text = query.EnderecoOrigem;
                        CbDestino.Text = query.Destino;
                        txtEnderecoDestino.Text = query.EnderecoDestino;
                        obsGerais.Text = query.ObsGerais;
                        tipoSolicitacao = query.TipoSolicitacao;

            }
        }

        private void BtnOutraAM_Click(object sender, EventArgs e)
        {
            PainelAM2.Visible = true;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            painelCancelar.Visible = true;
            DtHrCancelamento.Text = DateTime.Now.ToString();
            txtResponsavel.Text = System.Environment.UserName;

        }
        private void cancelar()
        {
            try
            {
            StatusBD d = new StatusBD();
            d.puxarLogisticaDaSolicitacaNaAmbulancia(idPaciente);

            InsercoesDoBanco ib = new InsercoesDoBanco();
            ib.cancelarSolicitacao(idPaciente, d.IdSolicitacoes_Ambulancias, MotivoCancelar.Text, DtHrCancelamento.Text,
                txtResponsavel.Text, txtObsCancelamento.Text);
            ib.updateNasTabelasParaCancelar(idPaciente, idAmbu, d.IdSolicitacoes_Ambulancias);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                MessageBox.Show("Solicitação cancelada com sucesso !!!");
                this.Dispose();
            }
           
        }

        private void BtnConfimar_Click(object sender, EventArgs e)
        {
            var querya = (String)null;
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from am in db.ambulancia
                            where am.idAmbulancia == idAmbu
                            select am.TipoAM;
                querya = query.FirstOrDefault();
            }

            if (tipoSolicitacao != "Avancada")
            {
                if (querya != "BASICO")
                {
                    MessageBox.Show("Selecionar ambulância do tipo basica ou a solicitação do tipo avançada!");
                    return;
                }
            }

            if (tipoSolicitacao != "Basica")
            {
                if (querya != "AVANCADO")
                {
                    MessageBox.Show("Selecionar ambulância do tipo avançada ou a solicitação do tipo básica!");
                    return;
                }
            }

            int contadorMaxdePacientes, zero = 0;
            if (statusAMLista == "BLOQUEADA")
            {
                MessageBox.Show("A ambulância selecionada esta Bloqueada, por favor selecione outra !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (statusAMLista == "OCUPADA" || statusAM == "OCUPADA")
            {
                using (DAHUEEntities db = new DAHUEEntities())
                {
                    var query = from sa in db.solicitacoes_ambulancias
                                where sa.idAmbulanciaSol == idAmbu &&
                                sa.SolicitacaoConcluida == zero
                                select sa.idSolicitacoes_Ambulancias;

                    var queryCount = query.Count();
                    contadorMaxdePacientes = queryCount;
                }
                if (contadorMaxdePacientes == 5)
                {
                    MessageBox.Show("O maximo de pacientes colocados na ambulancia ja atingiu a marca de 5 lugares, favor escolha outra ambulancia !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (contadorMaxdePacientes == 1)
                {
                    DialogResult a = MessageBox.Show("Voce esta adicionando outro paciente na ambulancia " + label22.Text + ", deseja concluir ?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                }

            }
            ConfirmaAM();
            PainelAM2.Visible = true;

        }
        public void pegarDadosDasAmbulancias()
        {
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from am in db.ambulancia
                               join sa in db.solicitacoes_ambulancias
                               on new { idAmbulanciaSol = am.idAmbulancia, SolicitacaoConcluida = 0 }
                               equals new { sa.idAmbulanciaSol, SolicitacaoConcluida = (int)sa.SolicitacaoConcluida } into sa_join
                               from sa in sa_join.DefaultIfEmpty()
                               join sp in db.solicitacoes_paciente on new { idSolicitacoesPacientes = (int)sa.idSolicitacoesPacientes } equals new { idSolicitacoesPacientes = sp.idPaciente_Solicitacoes } into sp_join
                               from sp in sp_join.DefaultIfEmpty()
                               orderby am.idAmbulancia
                               select new
                               {
                                   am.idAmbulancia,
                                   am.NomeAmbulancia,
                                   am.StatusAmbulancia,
                                   Paciente = sp.Paciente,
                                   Idade = sp.Idade,
                                   Origem = sp.Origem,
                                   Destino = sp.Destino
                               };

                var queryAmbulanciaUsb = query.ToList();

                Lista.DataSource = queryAmbulanciaUsb;
                Lista.ClearSelection();

                Lista.Columns[0].Visible = false;
                Lista.Columns[1].HeaderText = "Ambulancia";
                Lista.Columns[2].HeaderText = "Status";
                Lista.ClearSelection();
            }
        }
        private void ConfirmaAM()
        {

            try
            {
                InsercoesDoBanco ib = new InsercoesDoBanco();
                ib.confirmarAmbulanciaNaSolicitacao(idPaciente, idAmbu);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                MessageBox.Show("Solicitação salva com sucesso !!!");
                this.Dispose();
            }

        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            imprimirFicha();
        }
        public void imprimirFicha()
        {
            // ConsultarSolicitaoRelatorio();

            StatusBD d = new StatusBD();
            using (DAHUEEntities db = new DAHUEEntities())
            {
               var query = (from eq in db.equipe
                        where eq.idAM == idAmbu
                        orderby eq.idEquipe descending
                        select eq).FirstOrDefault();

               var querySa = (from sp in db.solicitacoes_paciente
                          where sp.idPaciente_Solicitacoes == idPaciente
                          select sp).FirstOrDefault();
                
            if(idSolicitacaoAm == 0)
            {
                d.puxarLogisticaDaSolicitacaNaAmbulancia(idPaciente);

            }else{

                d.puxarLogisticaDaSolicitacaNaAmbulanciaSelecionadaNaConsulta(idPaciente, idSolicitacaoAm);

                query = (from eq in db.equipe
                             where eq.idAM == d.IdAmbulanciaSol
                             orderby eq.idEquipe descending
                             select eq).FirstOrDefault();
           }

            string cancelado;
            

                if (MotivoCancelar.Text != "")
                {
                    cancelado = "Sim";
                }
                else
                {
                    cancelado = "Não";
                }

                if(NomeAM == "" || NomeAM == null)
                {
                    var nome = (from am in db.ambulancia
                                where am.idAmbulancia == d.IdAmbulanciaSol
                                select am.NomeAmbulancia).FirstOrDefault();
                    NomeAM = nome;
                }

            int n = 34;
            ReportViewer report = new ReportViewer();
            report.ProcessingMode = ProcessingMode.Local;
            report.LocalReport.ReportEmbeddedResource = "WindowsFormsApplication2.Report1.rdlc";
            ReportParameter[] listReport = new ReportParameter[n];
            listReport[0] = new ReportParameter("Nome", querySa.Paciente);
            listReport[1] = new ReportParameter("Tipo", querySa.TipoSolicitacao);
            listReport[2] = new ReportParameter("Agendado", querySa.Agendamento);
            listReport[3] = new ReportParameter("DtHrAgendado", querySa.DtHrAgendamento);
            listReport[4] = new ReportParameter("ID", Convert.ToString(querySa.idPaciente_Solicitacoes));
            listReport[5] = new ReportParameter("Sexo", querySa.Genero);
            listReport[6] = new ReportParameter("Idade", querySa.Idade);
            listReport[7] = new ReportParameter("Diagnostico", querySa.Diagnostico);
            listReport[8] = new ReportParameter("Motivo", querySa.Motivo);
            listReport[9] = new ReportParameter("Submotivo", querySa.SubMotivo);
            listReport[10] = new ReportParameter("Origem", querySa.Origem);
            listReport[11] = new ReportParameter("Destino", querySa.Destino);
            listReport[12] = new ReportParameter("EnderecoOrigem", querySa.EnderecoOrigem);
            listReport[13] = new ReportParameter("EnderecoDestino", querySa.EnderecoDestino);
            listReport[14] = new ReportParameter("Obsgerais", querySa.ObsGerais);
            listReport[15] = new ReportParameter("NomeSolicitante", querySa.NomeSolicitante);
            listReport[16] = new ReportParameter("LocalSolicitacao", querySa.LocalSolicitacao);
            listReport[17] = new ReportParameter("Telefone", querySa.Telefone);
            listReport[18] = new ReportParameter("Registrado", System.Environment.UserName);
            listReport[19] = new ReportParameter("HrRegistro", DateTime.Now.ToString("dd/MM/yyyy-HH:mm:ss"));
            listReport[20] = new ReportParameter("AM", NomeAM);
            listReport[21] = new ReportParameter("Condutor", query.Condutor);
            listReport[22] = new ReportParameter("Equipe", query.Enfermeiros);
            listReport[23] = new ReportParameter("Prioridade", querySa.Prioridade);
            listReport[24] = new ReportParameter("Cancelamento", cancelado);
            listReport[25] = new ReportParameter("HrCancelamento", DtHrCancelamento.Text);
            listReport[26] = new ReportParameter("MotivoCancelamento", MotivoCancelar.Text);
            listReport[27] = new ReportParameter("NomeCancelante", txtResponsavel.Text);
            listReport[28] = new ReportParameter("HrCiencia", d.DtHrCiencia1);
            listReport[29] = new ReportParameter("HrSaida", d.DtHrSaidaOrigem1);
            listReport[30] = new ReportParameter("HrLiberacao", d.DtHrLiberacaoEquipe1);
            listReport[31] = new ReportParameter("HrChegadaOrigem", d.DtHrChegadaOrigem1);
            listReport[32] = new ReportParameter("HrChegadaDestino", d.DtHrChegadaDestino1);
            listReport[33] = new ReportParameter("HrEquipepatio", d.DtHrEquipePatio1);
            
            report.LocalReport.SetParameters(listReport);
            report.LocalReport.Refresh();

            //reportViewer1.Visible = true;

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string enconding;
            string extension;

            byte[] bytePDF = report.LocalReport.Render("Pdf", null, out mimeType, out enconding, out extension, out streamids, out warnings);

            FileStream filestrampdf = null;
            string nomeArquivopdf = Path.GetTempPath() + "Impresso_" + txtNomePaciente.Text + DateTime.Now.ToString("_dd_MM_yyyy-HH_mm_ss") + ".pdf";

            filestrampdf = new FileStream(nomeArquivopdf, FileMode.Create);
            filestrampdf.Write(bytePDF, 0, bytePDF.Length);
            filestrampdf.Close();

            Process.Start(nomeArquivopdf);
        }
        }

        private void CancelaSolicitacao_Click(object sender, EventArgs e)
        {
            MotivoCancelar.Text = "";
            txtObsCancelamento.Text = "";
            DtHrCancelamento.Text = "";
            painelCancelar.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result1 = MessageBox.Show("Deseja cancelar a solicitação do paciente na ambulancia ?",
            "Atenção !",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result1 == DialogResult.Yes)
            {
                cancelar();
                imprimirFicha();
                this.Dispose();
                
            }
        }
        private void BtnAlterar_Click(object sender, EventArgs e)
        {
            
            if(txtAtendMarcado.Enabled == false){
            DesbloquarCampos();
            }else
            {
                if (RbFemenino.Checked)
                {
                    Sexo = "F";
                }
                else if (RbMasculino.Checked)
                {
                    Sexo = "M";
                }
                if (Agendamento == "" || TipoAM == "" || Agendamento == null || TipoAM == null)
                {
                    MessageBox.Show("Marque a opção do tipo de ambulancia ou se é agendado !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (txtNomeSolicitante.Text == "" ||
                CbLocalSolicita.Text == "" ||
                txtTelefone.Text == "" ||
                txtNomePaciente.Text == "" ||
                txtIdade.Text == "" ||
                txtDiagnostico.Text == "" ||
                CbMotivoChamado.Text == "" ||
                Sexo == "" ||
                CbTipoMotivoSelecionado.Text == "" ||
                CbOrigem.Text == "" ||
                CbDestino.Text == "" ||
                txtEnderecoOrigem.Text == "" ||
                txtEnderecoDestino.Text == ""){
                
               MessageBox.Show("Verifique se algum campo esta vazio ou desmarcado !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }
               MessageBox.Show("Deseja salvar as alterações feitas ?","Atenção !", MessageBoxButtons.OKCancel,MessageBoxIcon.Exclamation);
               AlterarCampos();
               bloquearCampos();

            }

        }

        private void DesbloquarCampos()
        {
            BtnBasica.Location = new Point(330, 12);
            BtnAvancada.Location = new Point(430, 12);
            BtnAvancada.Visible = true;
            BtnBasica.Visible = true;
            Btnagendanao.Visible = true;
            Btnagendasim.Visible = true;
            Btnagendanao.Location = new Point(470, 40);
            Btnagendasim.Location = new Point(390, 40);

            txtAtendMarcado.Enabled = true;
            txtNomeSolicitante.Enabled = true;
            CbLocalSolicita.Enabled = true;
            txtTelefone.Enabled = true;
            txtNomePaciente.Enabled = true;
            RbFemenino.Enabled = true;
            RbMasculino.Enabled = true;
            txtIdade.Enabled = true;
            txtDiagnostico.Enabled = true;
            CbMotivoChamado.Enabled = true;
            CbTipoMotivoSelecionado.Enabled = true;
            CbOrigem.Enabled = true;
            CbDestino.Enabled = true;
            txtEnderecoDestino.Enabled = true;
            txtEnderecoOrigem.Enabled = true;
            obsGerais.Enabled = true;
            PrioridadeTxt.Enabled = true;
            
        }
        private void bloquearCampos()
        {
            BtnBasica.Location = new Point(382, 12);
            BtnAvancada.Location = new Point(382, 12);
            BtnAvancada.Visible = true;
            BtnBasica.Visible = true;
            Btnagendanao.Visible = true;
            Btnagendasim.Visible = true;
            Btnagendanao.Location = new Point(405, 40);
            Btnagendasim.Location = new Point(405, 40);

            txtAtendMarcado.Enabled = false;
            txtNomeSolicitante.Enabled = false;
            CbLocalSolicita.Enabled = false;
            txtTelefone.Enabled = false;
            txtNomePaciente.Enabled = false;
            RbFemenino.Enabled = false;
            RbMasculino.Enabled = false;
            txtIdade.Enabled = false;
            txtDiagnostico.Enabled = false;
            CbMotivoChamado.Enabled = false;
            CbTipoMotivoSelecionado.Enabled = false;
            CbOrigem.Enabled = false;
            CbDestino.Enabled = false;
            txtEnderecoDestino.Enabled = false;
            txtEnderecoOrigem.Enabled = false;
            obsGerais.Enabled = false;
            PrioridadeTxt.Enabled = false;

        }

        private void AlterarCampos()
        {

          try
            {
              InsercoesDoBanco ib = new InsercoesDoBanco();
              ib.alterarCamposDaSolicitacao(idPaciente, TipoAM, Agendamento, txtAtendMarcado.Text, txtNomeSolicitante.Text, CbLocalSolicita.Text, txtTelefone.Text,
                                            txtNomePaciente.Text, Sexo, txtIdade.Text, txtDiagnostico.Text, CbMotivoChamado.Text, CbTipoMotivoSelecionado.Text, CbOrigem.Text, 
                                            txtEnderecoOrigem.Text, CbDestino.Text, txtEnderecoDestino.Text, System.Environment.UserName, DateTime.Now.ToString(), obsGerais.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MessageBox.Show("Solicitação alterada com sucesso !!!");
            }



        }

        private void BtnAvancada_Click(object sender, EventArgs e)
        {
            TipoAM = "Avancada";
            if (BtnBasica.BackColor == Color.PaleTurquoise)
            {
                BtnAvancada.BackColor = Color.PaleTurquoise;               
                BtnAvancada.ForeColor = Color.Teal;
                BtnBasica.ForeColor = Color.Teal;
                BtnBasica.BackColor = Color.PaleTurquoise;
            }
                BtnBasica.BackColor = Color.PaleTurquoise;
                BtnAvancada.BackColor = Color.Teal;
                BtnAvancada.ForeColor = Color.PaleTurquoise;
                BtnBasica.ForeColor = Color.Teal;
        
        }

        private void BtnBasica_Click(object sender, EventArgs e)
        {
            TipoAM = "Basica";  
            
            
            if (BtnAvancada.BackColor == Color.PaleTurquoise)
            {
                BtnBasica.BackColor = Color.PaleTurquoise;
                BtnBasica.ForeColor = Color.Teal;
                BtnAvancada.ForeColor = Color.Teal;
                BtnAvancada.BackColor = Color.PaleTurquoise;
            }
                BtnAvancada.BackColor = Color.PaleTurquoise;
                BtnBasica.BackColor = Color.Teal;
                BtnBasica.ForeColor = Color.PaleTurquoise;
                BtnAvancada.ForeColor = Color.Teal;
        }

        private void Btnagendasim_Click(object sender, EventArgs e)
        {
            
            txtAtendMarcado.Focus();
            txtAtendMarcado.Text = DateTime.Now.ToString();
            Agendamento = "Sim";

            if (Btnagendanao.BackColor == Color.PaleTurquoise)
            {
                Btnagendanao.BackColor = Color.PaleTurquoise;
                Btnagendanao.ForeColor = Color.Teal;
                Btnagendasim.ForeColor = Color.Teal;
                Btnagendasim.BackColor = Color.PaleTurquoise;

            }

            Btnagendanao.BackColor = Color.PaleTurquoise;
            Btnagendasim.BackColor = Color.Teal;
            Btnagendasim.ForeColor = Color.PaleTurquoise;
            Btnagendanao.ForeColor = Color.Teal;
        }

        private void Btnagendanao_Click(object sender, EventArgs e)
        {
            Agendamento = "Nao";
            if (Btnagendasim.BackColor == Color.PaleTurquoise)
            {
                Btnagendasim.BackColor = Color.PaleTurquoise;
                Btnagendasim.ForeColor = Color.Teal;
                Btnagendanao.ForeColor = Color.Teal;
                Btnagendanao.BackColor = Color.PaleTurquoise;

            }

            Btnagendasim.BackColor = Color.PaleTurquoise;
            Btnagendanao.BackColor = Color.Teal;
            Btnagendanao.ForeColor = Color.PaleTurquoise;
            Btnagendasim.ForeColor = Color.Teal;
        }

        private void CbLocalSolicita_SelectedIndexChanged(object sender, EventArgs e)
        {
            pegaUnidade = CbLocalSolicita.Text;
            unidade_telefone();
        }
        public void unidade_telefone()
        {
            try
            {
               using(DAHUEEntities db = new DAHUEEntities())
               {
                   var query = from tele in db.enderecos
                               where tele.NomeUnidade == pegaUnidade
                               select tele.Telefone;
                   txtTelefone.Text = query.FirstOrDefault();
               }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }
        private void PuxarEnderecos()
        {
            using (DAHUEEntities db = new DAHUEEntities())
            {
                CbLocalSolicita.DataSource = db.enderecos.ToList();
                CbLocalSolicita.ValueMember = "NomeUnidade";
                CbLocalSolicita.DisplayMember = "NomeUnidade";
                CbDestino.DataSource = db.enderecos.ToList();
                CbDestino.ValueMember = "NomeUnidade";
                CbDestino.DisplayMember = "NomeUnidade";
                CbOrigem.DataSource = db.enderecos.ToList();
                CbOrigem.ValueMember = "NomeUnidade";
                CbOrigem.DisplayMember = "NomeUnidade";
            }
        }

        private void CbMotivoChamado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CbTipoMotivoSelecionado.Items.Clear();
            CbTipoMotivoSelecionado.Text = "";
        }

        private void CbTipoMotivoSelecionado_SelectedIndexChanged(object sender, EventArgs e)
        {
            Motivo();
        }

        private void Motivo()
        {

            //descobrir o que foi selecionado e criar uma variavel para ela
            if (CbMotivoChamado.Text == "ALTA HOSPITALAR")
            {
                pegamotivo = "ALTA_HOSPITALAR";
            }
            else if (CbMotivoChamado.Text == "AVALIAÇÃO DE MÉDICO ESPECIALISTA")
            {
                pegamotivo = "AVALIACAO_DE_MEDICO_ESPECIALISTA";
            }
            else if (CbMotivoChamado.Text == "AVALIAÇÃO DE PROFISSIONAL NÃO MÉDICO")
            {
                pegamotivo = "AVALIACAO_DE_PROFISSIONAL_NAO_MEDICO";
            }
            else if (CbMotivoChamado.Text == "CONSULTA AGENDADA")
            {
                pegamotivo = "CONSULTA_AGENDADA";
            }
            else if (CbMotivoChamado.Text == "DEMANDAS JUDICIAIS")
            {
                pegamotivo = "DEMANDA_JUDICIAL";
            }
            else if (CbMotivoChamado.Text == "EVENTO COMEMORATIVO")
            {
                pegamotivo = "EVENTO_COMEMORATIVO_DO_MUNICIPIO";
            }
            else if (CbMotivoChamado.Text == "EVENTO DE CULTURA, LAZER OU RELIGIÃO")
            {
                pegamotivo = "EVENTO_DE_CULTURA_LAZER_OU_RELIGIAO";
            }
            else if (CbMotivoChamado.Text == "EVENTO ESPORTIVO")
            {
                pegamotivo = "EVENTO_ESPORTIVO";
            }
            else if (CbMotivoChamado.Text == "EXAME AGENDADO")
            {
                pegamotivo = "EXAME_AGENDADO";
            }
            else if (CbMotivoChamado.Text == "EXAME DE URGÊNCIA")
            {
                pegamotivo = "EXAME_DE_URGENCIA";
            }
            else if (CbMotivoChamado.Text == "INTERNAÇÃO EM ENFERMARIA")
            {
                pegamotivo = "INTERNACAO_EM_ENFERMARIA";
            }
            else if (CbMotivoChamado.Text == "INTERNAÇÃO EM UTI")
            {
                pegamotivo = "INTERNACAO_EM_UTI";
            }
            else if (CbMotivoChamado.Text == "PROCEDIMENTO")
            {
                pegamotivo = "PROCEDIMENTO";
            }
            else if (CbMotivoChamado.Text == "RETORNO")
            {
                pegamotivo = "RETORNO";
            }
            else if (CbMotivoChamado.Text == "SALA VERMELHA/EMERGÊNCIA")
            {
                pegamotivo = "SALA_VERMELHA_EMERGENCIA";
            }
            else if (CbMotivoChamado.Text == "TRANSPORTE DE INSUMOS/PRODUTOS/MATERIAIS")
            {
                pegamotivo = "TRANSPORTE_DE_INSUMOS_PRODUTOS_MATERIAIS";
            }
            else if (CbMotivoChamado.Text == "TRANSPORTE DE PROFISSIONAIS")
            {
                pegamotivo = "TRANSPORTE_DE_PROFISSIONAIS";
            }

            using (DAHUEEntities db = new DAHUEEntities())
            {
                CbTipoMotivoSelecionado.DataSource = db.referencias.ToList();
                CbTipoMotivoSelecionado.ValueMember = pegamotivo;
                CbTipoMotivoSelecionado.DisplayMember = pegamotivo;
            }

        }

        private void CbOrigem_SelectedIndexChanged(object sender, EventArgs e)
        {
            pegaUnidadeEnd = CbOrigem.Text;
            unidade_Endereco();
            txtEnderecoOrigem.Text = Endereco1;
        }

        private void CbDestino_SelectedIndexChanged(object sender, EventArgs e)
        {
            pegaUnidadeEnd = CbDestino.Text;
            unidade_Endereco();
            txtEnderecoDestino.Text = Endereco1;
        }

        private void unidade_Endereco()
        {
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var enderecoDoEnderecos = db.enderecos
                    .Where(e => e.NomeUnidade == pegaUnidadeEnd)
                    .Select(e => e.Endereco);

                Endereco1 = enderecoDoEnderecos.FirstOrDefault();
            }
        }

        private void CbTipoMotivoSelecionado_Click(object sender, EventArgs e)
        {
            Motivo();
        }

        private void Lista_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            idAmbu = Convert.ToInt32(Lista.Rows[e.RowIndex].Cells[0].Value.ToString());
            statusAMLista = Lista.Rows[e.RowIndex].Cells["StatusAmbulancia"].Value.ToString();
            NomeAM = Lista.Rows[e.RowIndex].Cells["NomeAmbulancia"].Value.ToString();
            PainelAM2.Visible = false;
            label22.Text = NomeAM;
        }

        private void Lista_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null && e.Value.Equals("BLOQUEADA"))
            {
                Lista.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0, 122, 181);
                Lista.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
            }
            else if (e.Value != null && e.Value.Equals("OCUPADA"))
            {
                Lista.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(224, 62, 54);
                Lista.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
            }
            else if (e.Value != null && e.Value.Equals("DISPONIVEL"))
            {
                Lista.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(46, 172, 109);
                Lista.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult result1 = MessageBox.Show("Deseja retirar a solicitação desta ambulância ?",
            "Atenção !",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result1 == DialogResult.Yes)
            {
                using (DAHUEEntities db = new DAHUEEntities())
                {
                    StatusBD d = new StatusBD();
                    d.puxarLogisticaDaSolicitacaNaAmbulancia(idPaciente);

                    solicitacoes_ambulancias sa = db.solicitacoes_ambulancias.First(p => p.idSolicitacoes_Ambulancias == d.IdSolicitacoes_Ambulancias);
                    sa.SolicitacaoConcluida = 1;

                    solicitacoes_paciente sp = db.solicitacoes_paciente.First(s => s.idPaciente_Solicitacoes == idPaciente);
                    sp.AmSolicitada = 0;

                    var contemPaciente = (from soa in db.solicitacoes_ambulancias
                                          where soa.idAmbulanciaSol == idAmbu && soa.SolicitacaoConcluida == 0
                                          select soa).Count();

                    if (contemPaciente == 1)
                    {
                        ambulancia am = db.ambulancia.First(a => a.idAmbulancia == idAmbu);
                        am.StatusAmbulancia = "DISPONIVEL";
                    }
                    db.SaveChanges();
                }
                this.Dispose();
            }
        }

    }
}
