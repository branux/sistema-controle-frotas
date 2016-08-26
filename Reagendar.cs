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

namespace Sistema_Controle
{
    public partial class Reagendar : Form
    {
        int idPacient;
        public Reagendar(string dataAgendamento, int idPaciente)
        {
            InitializeComponent();
            dataAgendar.Text = dataAgendamento;
            idPacient = idPaciente;
        }

        private void Enviar_Click(object sender, EventArgs e)
        {
            using(DAHUEEntities db = new DAHUEEntities())
            {
                solicitacoes_agendamentos sag = new solicitacoes_agendamentos();
                sag.idSolicitacao_paciente = idPacient;
                sag.DtHrAgendamento = Convert.ToDateTime(dataAgendar);

                db.solicitacoes_agendamentos.Add(sag);

                solicitacoes_paciente sp = db.solicitacoes_paciente.First(p => p.idPaciente_Solicitacoes == idPacient);
                sp.Registrado = "Aguardando resposta do Solicitante";

                db.SaveChanges();
            }
            MessageBox.Show("Solicitação Reagendada com sucesso !");
            this.Close();
        }
    }
}