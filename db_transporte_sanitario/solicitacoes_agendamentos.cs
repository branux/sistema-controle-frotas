//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace db_transporte_sanitario
{
    using System;
    using System.Collections.Generic;
    
    public partial class solicitacoes_agendamentos
    {
        public int? idSolicitacaoAgendamento { get; set; }
        public DateTime? DtHrAgendamento { get; set; }
        public int idSolicitacao_paciente { get; set; }
    
        public virtual solicitacoes_paciente solicitacoes_paciente { get; set; }
    }
}