﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using db_transporte_sanitario;
using System.Data.Entity.SqlServer;

namespace Sistema_Controle
{
    public partial class RespostaDeAmbulancias : Form
    {
        int idPaciente;
        public RespostaDeAmbulancias()
        {
            InitializeComponent();
            puxarAgendadasNegadas();
            puxarAgendadasPendentes();
            id.Text = "";
            dtHrReagendamento.Text = "";
        }

        public void puxarAgendadasNegadas()
        {
            var final = Calendario.SelectionRange.End.AddHours(23).AddMinutes(59).AddSeconds(59);
            var comeco = Calendario.SelectionRange.Start;
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == 0 &&
                            sp.Agendamento == "Sim" &&
                            (sp.DtHrdoAgendamento >= comeco && sp.DtHrdoAgendamento <= final) &&
                            sp.Registrado == "Aguardando resposta do solicitante"
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                sp.LocalSolicitacao,
                                sp.Agendamento,
                                sp.DtHrdoAgendamento,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.Prioridade,
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };
                var quertCont = query.Count();
                var queryAmbu = query.ToList();
                Respondidos.Text = "Respostas negadas (" + quertCont + ")";
                ListaAgendados.DataSource = queryAmbu;
                ListaAgendados.ClearSelection();

            }
        }
        public void puxarAgendadasPendentes()
        {

            var final = Calendario.SelectionRange.End.AddHours(23).AddMinutes(59).AddSeconds(59);
            var comeco = Calendario.SelectionRange.Start;
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == 0 &&
                            sp.Agendamento == "Sim" &&
                            (sp.DtHrdoAgendamento >= comeco && sp.DtHrdoAgendamento <= final) &&
                            sp.Registrado == "Aguardando resposta do controle"
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                sp.LocalSolicitacao,
                                sp.Agendamento,
                                sp.DtHrdoAgendamento,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.Prioridade,
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };

                var queryAmbu = query.ToList();
                var querycont = query.Count();
                Encaminhados.Text = "Solicitações agendadas (" + querycont + ")";
                ListaAgendados.DataSource = queryAmbu;
                ListaAgendados.ClearSelection();

            }
        }

        private void ListaAgendados_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                idPaciente = Convert.ToInt32(ListaAgendados.Rows[e.RowIndex].Cells[0].Value.ToString());
                using (DAHUEEntities db = new DAHUEEntities())
                {
                    var query = (from sp in db.solicitacoes_paciente
                                 join saa in db.solicitacoes_agendamentos
                                 on sp.idReagendamento equals saa.idSolicitacaoAgendamento into saa_sp_join
                                 from saa in saa_sp_join.DefaultIfEmpty()
                                 where sp.idPaciente_Solicitacoes == idPaciente
                                 select new { sp, saa }).FirstOrDefault();

                    id.Text = query.sp.idPaciente_Solicitacoes.ToString();
                    Tipo.Text = query.sp.TipoSolicitacao;
                    DataInicio.Text = query.sp.DtHrdoInicio.ToString();
                    DataHrAgendamento.Text = query.sp.DtHrdoAgendamento.ToString();
                    NomeSolicitante.Text = query.sp.NomeSolicitante;
                    LocalSolicitacao.Text = query.sp.LocalSolicitacao;
                    Telefone.Text = query.sp.Telefone;
                    NomePaciente.Text = query.sp.Paciente;
                    if (query.sp.Genero == "F")
                    {
                        RbFemenino.Checked = true;
                    }
                    else
                    {
                        RbMasculino.Checked = true;
                    }
                    Idade.Text = query.sp.Idade;
                    Diagnostico.Text = query.sp.Diagnostico;
                    MotivoChamado.Text = query.sp.Motivo;
                    TipoMotivoSelecionado.Text = query.sp.SubMotivo;
                    Prioridade.Text = query.sp.Prioridade;
                    Origem.Text = query.sp.Origem;
                    EnderecoOrigem.Text = query.sp.EnderecoOrigem;
                    Destino.Text = query.sp.Destino;
                    EnderecoDestino.Text = query.sp.EnderecoDestino;
                    Obs.Text = query.sp.ObsGerais;
                    if (query.saa != null)
                    {
                        dtHrReagendamento.Text = query.saa.DtHrAgendamento.ToString();
                    }

                }

            }
        }
        private void Calendario_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (Respondidos.Checked == true)
            {
                puxarAgendadasNegadas();
            }
            else if (Encaminhados.Checked == true)
            {
                puxarAgendadasPendentes();
            }
        }
        private void RespostaDeAmbulancias_Activated(object sender, EventArgs e)
        {
            if (Respondidos.Checked == true)
            {
                puxarAgendadasNegadas();
                Respondidos.Checked = true;
            }
            else
            {
                puxarAgendadasPendentes();
                Encaminhados.Checked = true;
            }
        }

        private void Encaminhados_Click(object sender, EventArgs e)
        {
            puxarAgendadasPendentes();
            negativa.Visible = true;
        }
        private void Respondidos_Click(object sender, EventArgs e)
        {
            puxarAgendadasNegadas();
            negativa.Visible = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(id.Text).Equals(false))
            {
                Reagendar re = new Reagendar(DataHrAgendamento.Text, idPaciente);
                re.ShowDialog();

                this.ClearTextBoxes();
                this.ClearComboBox();
                this.id.Text = "";
                this.dtHrReagendamento.Text = "";
                this.RbFemenino.Checked = false;
                this.RbMasculino.Checked = false;
                this.Obs.Text = "";
            }
            else
            {
                MessageBox.Show("Selecione a solicitação que deseja reagendar !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void negativa_Click(object sender, EventArgs e)
        {
            if (idPaciente != 0) { 
                NegarReagendamento nr = new NegarReagendamento(idPaciente);
                nr.ShowDialog();
            }
            else
            {
                MessageBox.Show("Selecione a solicitação que deseja cancelar !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Reagendamentos_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(id.Text).Equals(false))
            {
                Reagedamentos re = new Reagedamentos(idPaciente);
                re.ShowDialog();
            }
            else
            {
                MessageBox.Show("Selecione a solicitação que deseja ver o histórico de reagendamentos !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Aceitar_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(id.Text).Equals(false))
            {
                DialogResult result1 = MessageBox.Show("Deseja aceitar o agendamento ?",
                "Atenção !",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result1 == DialogResult.Yes)
                {

                    using (DAHUEEntities db = new DAHUEEntities())
                    {
                        solicitacoes_paciente sp = db.solicitacoes_paciente.First(p => p.idPaciente_Solicitacoes == idPaciente);
                        sp.Registrado = "Sim";
                        db.SaveChanges();
                    }
                    MessageBox.Show("Solicitação aceita com sucesso !");

                    ClearTextBoxes();
                    ClearComboBox();
                    id.Text = "";
                    dtHrReagendamento.Text = "";
                    RbFemenino.Checked = false;
                    RbMasculino.Checked = false;
                    Obs.Text = "";

                }
            }
            else
            {
                MessageBox.Show("Selecione a solicitação que deseja aceitar !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cancelarReagendamento_Click(object sender, EventArgs e)
        {
            if (idPaciente != 0)
            {
                SelecionaAM ST = new SelecionaAM(idPaciente, 0, 0);
                ST.ShowDialog();
            }
            else
            {
                MessageBox.Show("Selecione a solicitação que deseja cancelar !", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void ClearTextBoxes()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };

            func(Controls);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void ClearComboBox()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is ComboBox)
                        (control as ComboBox).SelectedIndex = -1;
                    else
                        func(control.Controls);
            };

            func(Controls);
        }

    }
}
