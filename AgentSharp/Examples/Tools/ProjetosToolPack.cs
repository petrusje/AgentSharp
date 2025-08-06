using AgentSharp.Attributes;
using AgentSharp.Examples.Models;
using AgentSharp.Tools;
using System;
using System.Linq;

namespace AgentSharp.Examples
{
  public class ProjetosToolPack : ToolPack
  {
    [FunctionCall("Obter informações detalhadas de um projeto")]
    [FunctionCallParameter("projetoId", "ID do projeto a ser consultado")]
    public string GetProjeto(string projetoId)
    {
      var banco = GetProperty("BancoDados") as BancoDadosProjetos;

      if (banco?.Projetos.TryGetValue(projetoId, out var projeto) == true)
      {
        return $@"
📊 PROJETO: {projeto.Nome} ({projeto.Id})
Status: {projeto.Status}
Início: {projeto.DataInicio:dd/MM/yyyy}
Equipe: {projeto.TamanhoEquipe} pessoas
Tecnologias: {string.Join(", ", projeto.Tecnologias)}
Duração: {(DateTime.Now - projeto.DataInicio).Days} dias";
      }

      return $"❌ Projeto {projetoId} não encontrado";
    }

    [FunctionCall("Obter métricas de performance do projeto")]
    [FunctionCallParameter("projetoId", "ID do projeto")]
    public string GetMetricasProjeto(string projetoId)
    {
      var banco = GetProperty("BancoDados") as BancoDadosProjetos;

      if (banco?.Projetos.TryGetValue(projetoId, out var projeto) == true)
      {
        var velocidade = projeto.MetricasVelocidade;
        var qualidade = projeto.MetricasQualidade;

        return $@"
📈 MÉTRICAS DE PERFORMANCE - {projetoId}:

VELOCIDADE:
• Velocidade média: {velocidade.VelocidadeMedia:F1} pts/sprint
• Burndown rate: {velocidade.BurndownRate:P0}
• Tasks completadas: {velocidade.TasksCompletadas}
• Tasks pendentes: {velocidade.TasksPendentes}

QUALIDADE:
• Bugs abertos: {qualidade.BugsAbertos}
• Bugs fechados: {qualidade.BugsFechados}
• Cobertura testes: {qualidade.CoberturaTestes:F1}%
• Deploy frequency: {qualidade.DeployFrequency}/mês

ALERTAS:
{(velocidade.VelocidadeMedia < 6 ? "⚠️ Velocidade baixa!" : "")}
{(velocidade.BurndownRate < 0.8 ? "⚠️ Burndown rate baixo!" : "")}
{(qualidade.BugsAbertos > 10 ? "⚠️ Muitos bugs abertos!" : "")}
{(qualidade.CoberturaTestes < 70 ? "⚠️ Cobertura de testes baixa!" : "")}
{(qualidade.DeployFrequency < 4 ? "⚠️ Deploy frequency muito baixa!" : "")}";
      }

      return $"❌ Métricas não encontradas para {projetoId}";
    }

    [FunctionCall("Obter informações da equipe do projeto")]
    [FunctionCallParameter("projetoId", "ID do projeto")]
    public string GetEquipeProjeto(string projetoId)
    {
      var banco = GetProperty("BancoDados") as BancoDadosProjetos;

      if (banco?.Equipes.TryGetValue(projetoId, out var equipe) == true)
      {
        return $@"
👥 EQUIPE DO PROJETO {projetoId}:

ESTRUTURA:
• Tech Lead: {equipe.TechLead}
• Desenvolvedores: {string.Join(", ", equipe.Desenvolvedores)}
• QA: {equipe.QA}

MÉTRICAS:
• Satisfação da equipe: {equipe.SatisfacaoEquipe:F1}/10
• Rotatividade (6 meses): {equipe.RotatividadeUltimos6Meses} pessoas

ALERTAS:
{(equipe.SatisfacaoEquipe < 7 ? "⚠️ Satisfação da equipe baixa!" : "")}
{(equipe.RotatividadeUltimos6Meses > 1 ? "⚠️ Alta rotatividade de pessoal!" : "")}";
      }

      return $"❌ Equipe não encontrada para {projetoId}";
    }

    [FunctionCall("Obter histórico de deploys e defeitos")]
    [FunctionCallParameter("projetoId", "ID do projeto")]
    public string GetHistoricoDeploysDefeitos(string projetoId)
    {
      var banco = GetProperty("BancoDados") as BancoDadosProjetos;

      if (banco?.HistoricoDeploysDefeitos.TryGetValue(projetoId, out var historico) == true)
      {
        var resultado = $"📦 HISTÓRICO DE DEPLOYS - {projetoId}:\n\n";

        foreach (var deploy in historico)
        {
          resultado += $"📅 {deploy.Data:dd/MM/yyyy}: {(deploy.Sucesso ? "✅ SUCESSO" : "❌ FALHA")}\n";
          resultado += $"⏱️ Tempo inatividade: {(deploy.TempoInatividade.TotalMinutes < 60 ? $"{deploy.TempoInatividade.TotalMinutes} minutos" : $"{deploy.TempoInatividade.TotalHours} horas")}\n\n";
        }

        // Análise de falhas
        var falhas = historico.Count(d => !d.Sucesso);
        var tempoTotal = TimeSpan.FromTicks(historico.Sum(d => d.TempoInatividade.Ticks));

        resultado += $"RESUMO:\n";
        resultado += $"• Total de deploys: {historico.Count}\n";
        resultado += $"• Falhas: {falhas} ({(double)falhas / historico.Count:P0})\n";
        resultado += $"• Tempo total de inatividade: {(tempoTotal.TotalHours >= 1 ? $"{tempoTotal.TotalHours:F1} horas" : $"{tempoTotal.TotalMinutes:F0} minutos")}\n";

        if (falhas > 0 && historico.Count > 0 && (double)falhas / historico.Count > 0.3)
        {
          resultado += "⚠️ ALERTA: Alta taxa de falhas nos deploys!\n";
        }

        if (tempoTotal.TotalHours > 4)
        {
          resultado += "⚠️ ALERTA: Tempo de inatividade excessivo!\n";
        }

        return resultado;
      }

      return $"❌ Histórico não encontrado para {projetoId}";
    }
  }
}
