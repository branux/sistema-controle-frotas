﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using db_transporte_sanitario;
using System.Data.Entity.SqlServer;

namespace Sistema_Controle
{
    public partial class Solicitacoes : Form
    {
        int idPaciente, idAm;
        string contarComPrioridade, contarAgendadas;
        string tipo, statusAM;

        public Solicitacoes(int AMocup, string StatusAM)
        {
            InitializeComponent();
            puxarSolicitacoes();
            puxarAgendadas();
            idAm = AMocup;
            statusAM = StatusAM;
            txtTotal1.Text = contarComPrioridade;
            txtTotal3.Text = contarAgendadas;
            if (this.Focus())
            {
                puxarSolicitacoes();
                puxarAgendadas();
            }
        }

        public void puxarSolicitacoes()
        {
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == 0 &&
                            sp.Agendamento == "Nao" &&
                            sp.Registrado == "Sim"
                            orderby sp.DtHrdoInicio descending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };

                var queryAmbu = query.ToList();
                var contar = query.Count();
                txtTotal1.Text = contar.ToString();
                contarComPrioridade = contar.ToString();
                ListaSolicitacoes.DataSource = queryAmbu;
                ListaSolicitacoes.ClearSelection();
                ListaSolicitacoes.Columns[0].HeaderText = "ID";
            }
        }
        public void puxarAgendadas()
        {
            int zero = 0;
            DateTime Data = DateTime.Now;

            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim"
                            && sp.Registrado == "Sim"
                            && SqlFunctions.DateDiff("day", Data, sp.DtHrdoAgendamento) == 0 
                            orderby sp.Paciente ascending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.SubMotivo,
                                sp.Origem,
                                sp.EnderecoOrigem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                            };

                this.listaAgendadas.RowTemplate.MinimumHeight = 22;

                var queryAmbu = query.DefaultIfEmpty().ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();

                this.listaAgendadas.Columns.Add("VTR", "VTR");
                this.listaAgendadas.Columns.Add("Fixo", "Fixo");

                this.listaAgendadas.Columns["VTR"].DisplayIndex = 12;
                this.listaAgendadas.Columns["Fixo"].DisplayIndex = 13;

                this.listaAgendadas.Columns["VTR"].Visible = false;
                this.listaAgendadas.Columns["Fixo"].Visible = false;
            }

        }

        private void listaComPrioridade_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            ListaSolicitacoes.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            ListaSolicitacoes.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
        }
        private void listaAgendadas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
             if(e.RowIndex > -1){

                 listaAgendadas.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                 listaAgendadas.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
               //  listaAgendadas.Columns["Data_Reagendada"].DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                // listaAgendadas.Columns["Data_Reagendada"].HeaderCell.Style.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
             }
        }
        
        private void listaComPrioridade_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var querya = (String)null;
                using (DAHUEEntities db = new DAHUEEntities())
                {
                    var query = from am in db.ambulancia
                                where am.idAmbulancia == idAm
                                select am.TipoAM;
                    querya = query.FirstOrDefault();
                }
                idPaciente = Convert.ToInt32(ListaSolicitacoes.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                tipo = ListaSolicitacoes.Rows[e.RowIndex].Cells["Tipo"].Value.ToString();

                if (querya == null)
                {
                    SelecionaAM ST = new SelecionaAM(idPaciente, idAm, 0);
                    this.Dispose();
                    ST.ShowDialog();
                    return;
                }

                if (tipo == "Avancada")
                {
                    if (querya == "BASICO")
                    {
                        MessageBox.Show("Selecionar ambulância do tipo basica ou a solicitação do tipo avançada!");
                        return;
                    }
                }

                if (tipo == "Basica")
                {
                    if (querya == "AVANCADO")
                    {
                        MessageBox.Show("Selecionar ambulância do tipo avançada ou a solicitação do tipo básica!");
                        return;
                    }
                }

                SelecionaAM STi = new SelecionaAM(idPaciente, idAm, 0);
                this.Dispose();
                STi.ShowDialog();
            }
        }    
        private void listaAgendadas_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var querya = (String)null;
                using (DAHUEEntities db = new DAHUEEntities())
                {
                    var query = from am in db.ambulancia
                                where am.idAmbulancia == idAm
                                select am.TipoAM;
                    querya = query.FirstOrDefault();
                }
                idPaciente = Convert.ToInt32(listaAgendadas.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                tipo = listaAgendadas.Rows[e.RowIndex].Cells["Tipo"].Value.ToString();

                if (String.IsNullOrEmpty(querya))
                {
                    SelecionaAM ST = new SelecionaAM(idPaciente, idAm, 0);
                    this.Dispose();
                    ST.ShowDialog();
                    return;
                }
                if (tipo == "Avancada")
                {

                    if (querya == "BASICO")
                    {
                        MessageBox.Show("Selecionar ambulância do tipo avançada ou a solicitação do tipo básica!");
                        return;
                    }
                }

                if (tipo == "Basica")
                {
                    if (querya == "AVANCADO")
                    {
                        MessageBox.Show("Selecionar ambulância do tipo basica ou a solicitação do tipo avançada!");
                        return;
                    }
                }

                SelecionaAM STi = new SelecionaAM(idPaciente, idAm, 0);
                this.Dispose();
                STi.ShowDialog();
            }
        }

        private void imprimirAgendamentos_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in listaAgendadas.Rows)
            {
                row.Height = 80;
            }
            this.listaAgendadas.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8);
            this.listaAgendadas.DefaultCellStyle.Font = new Font("Arial", 8);

            this.listaAgendadas.Columns["VTR"].Visible = true;
            this.listaAgendadas.Columns["Fixo"].Visible = true;
            PrintDGV.Print_DataGridView(listaAgendadas);

        }

        #region Ordernacao
        private void OrdemPrioridade_Click(object sender, EventArgs e)
        {
            int zero = 0;
            ListaSolicitacoes.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == zero &&
                            sp.Registrado == "Sim" &&
                            sp.Agendamento == "Nao"
                            orderby sp.Prioridade ascending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };

                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarComPrioridade = contar.ToString();
                txtTotal1.Text = contar.ToString();
                ListaSolicitacoes.DataSource = queryAmbu;
                ListaSolicitacoes.ClearSelection();

            }
            OrdemPrioridade.Font = new Font(dtagenda.Font, FontStyle.Bold);
            OrdemPaciente.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemData.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
        }
        private void OrdemData_Click(object sender, EventArgs e)
        {
            int zero = 0;
            ListaSolicitacoes.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == zero &&
                            sp.Registrado == "Sim" &&
                            sp.Agendamento == "Nao"
                            orderby sp.DtHrdoInicio descending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };

                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarComPrioridade = contar.ToString();
                txtTotal1.Text = contar.ToString();
                ListaSolicitacoes.DataSource = queryAmbu;
                ListaSolicitacoes.ClearSelection();
            }
           OrdemData.Font = new Font(dtagenda.Font, FontStyle.Bold);
           OrdemPaciente.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
           OrdemPrioridade.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
        }
        private void OrdemPaciente_Click(object sender, EventArgs e)
        {
            int zero = 0;
            ListaSolicitacoes.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            where sp.AmSolicitada == zero &&
                            sp.Registrado == "Sim" &&
                            sp.Agendamento == "Nao"
                            orderby sp.Paciente ascending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino
                            };

                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarComPrioridade = contar.ToString();
                txtTotal1.Text = contar.ToString();
                ListaSolicitacoes.DataSource = queryAmbu;
                ListaSolicitacoes.ClearSelection();

                ListaSolicitacoes.Columns[0].HeaderText = "ID";
            }
            OrdemPaciente.Font = new Font(dtagenda.Font, FontStyle.Bold);
            OrdemData.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemPrioridade.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);

        }
        private void OrdemPrioridadeAgenda_Click(object sender, EventArgs e)
        {
            int zero = 0;
            listaAgendadas.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()                                                              
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim" &&
                            sp.Registrado == "Sim"
                            orderby sp.Prioridade descending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                            };
                this.listaAgendadas.RowTemplate.MinimumHeight = 22;
                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();
            }

            OrdemPrioridadeAgenda.Font = new Font(dtagenda.Font, FontStyle.Bold);
            dtagenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            dataFiltroAgenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            OrdemNomeAgenda.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemDataAgenda.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
            dtreagenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            dataReagendamento.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
        }
        private void OrdemDataAgenda_Click(object sender, EventArgs e)
        {
            int zero = 0;
            listaAgendadas.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim" &&
                            sp.Registrado == "Sim"
                            orderby sp.DtHrdoInicio descending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.Origem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                                
                            };
                this.listaAgendadas.RowTemplate.MinimumHeight = 22;
                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();

            }
            OrdemDataAgenda.Font = new Font(dtagenda.Font, FontStyle.Bold);
            dtagenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            dataFiltroAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            OrdemNomeAgenda.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemPrioridadeAgenda.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
            dtreagenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            dataReagendamento.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);

        }

        private void OrdemNomeAgenda_Click(object sender, EventArgs e)
        {
            int zero = 0;
            listaAgendadas.DataSource = "";
            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim" &&
                            sp.Registrado == "Sim"
                            orderby sp.Paciente ascending
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.SubMotivo,
                                sp.Origem,
                                sp.EnderecoOrigem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                            };
                this.listaAgendadas.RowTemplate.MinimumHeight = 22;
                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();


            }

            OrdemNomeAgenda.Font = new Font(dtagenda.Font, FontStyle.Bold);
            dtagenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            dataFiltroAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            OrdemDataAgenda.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemPrioridadeAgenda.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
            dtreagenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            dataReagendamento.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
        }
        private void dataFiltroAgenda_ValueChanged(object sender, EventArgs e)
        {
            int zero = 0;
            
            dtagenda.Font = new Font(dtagenda.Font, FontStyle.Bold);
            dataFiltroAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Bold);
            dtreagenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            dataReagendamento.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            OrdemDataAgenda.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemPrioridadeAgenda.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
            OrdemNomeAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);


            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim" &&
                            sp.Registrado == "Sim" &&
                            SqlFunctions.DateDiff("day", dataFiltroAgenda.Value, sp.DtHrdoAgendamento) == 0
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.SubMotivo,
                                sp.Origem,
                                sp.EnderecoOrigem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                            };
                this.listaAgendadas.RowTemplate.MinimumHeight = 22;
                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();

            }

        }
        private void dataReagendamento_ValueChanged(object sender, EventArgs e)
        {
            int zero = 0;
            DateTime Data = DateTime.Now;
          
            dtreagenda.Font = new Font(dtagenda.Font, FontStyle.Bold);
            dataReagendamento.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Bold);
            dtagenda.Font = new Font(dtreagenda.Font, FontStyle.Regular);
            dataFiltroAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);
            OrdemDataAgenda.Font = new Font(OrdemDataAgenda.Font, FontStyle.Regular);
            OrdemPrioridadeAgenda.Font = new Font(OrdemPrioridadeAgenda.Font, FontStyle.Regular);
            OrdemNomeAgenda.Font = new Font(OrdemNomeAgenda.Font, FontStyle.Regular);

            using (DAHUEEntities db = new DAHUEEntities())
            {
                var query = from sp in db.solicitacoes_paciente
                            join saa in db.solicitacoes_agendamentos
                            on sp.idReagendamento equals saa.idSolicitacaoAgendamento into spsaaajoin
                            from saa in spsaaajoin.DefaultIfEmpty()
                            where sp.AmSolicitada == zero &&
                            sp.Agendamento == "Sim" &&
                            sp.Registrado == "Sim" &&
                            SqlFunctions.DateDiff("day", dataReagendamento.Value, saa.DtHrAgendamento) == 0
                            select new
                            {
                                ID = sp.idPaciente_Solicitacoes,
                                sp.Paciente,
                                Tipo = sp.TipoSolicitacao,
                                sp.DtHrdoInicio,
                                sp.DtHrdoAgendamento,
                                Data_Reagendada = saa.DtHrAgendamento,
                                Prioridade = (sp.Prioridade.Contains("P0") ? "P0" : (sp.Prioridade.Contains("P1") ? "P1" : (sp.Prioridade.Contains("P2") ? "P2" : (sp.Prioridade.Contains("P3") ? "P3" : "SP")))),
                                sp.Motivo,
                                sp.SubMotivo,
                                sp.Origem,
                                sp.EnderecoOrigem,
                                sp.Destino,
                                sp.LocalSolicitacao,
                                sp.Telefone,
                                sp.ObsGerais
                            };
                this.listaAgendadas.RowTemplate.MinimumHeight = 22;
                var queryAmbu = query.ToList();
                var contar = query.Count();
                contarAgendadas = contar.ToString();
                txtTotal3.Text = contar.ToString();
                listaAgendadas.DataSource = queryAmbu;
                listaAgendadas.ClearSelection();

            }
        }

        #endregion


    }
}
