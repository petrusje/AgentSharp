using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agents_console
{
    public class ConsoleWorkflowContext
    {
        public string Topic { get; set; }
        public string InitialInput { get; set; }
        public string Summary { get; set; }
        public string Draft { get; set; }
        public string FinalText { get; set; }
    }

    public class AgentWorkFlowBook(IModel model, string name) : Agent<ConsoleWorkflowContext, string>(model, name)
    {
        public async Task<string> ExecuteAsync(string input, object context, List<AIMessage> history, CancellationToken token)
        {
            return await base.ExecuteAsync(input, (ConsoleWorkflowContext)context, history, token);
        }
    }

    public static class WorkflowExample
    {
        public static async Task RunWorkflowExample(IModel modelo)
    {
      // ---- Início do exemplo Worflow ----
      Console.WriteLine("\n=== Exemplo de Workflow (Pesquisa -> Escrita -> Revisão) ===");

      // Instancia agentes
      var pesquisador = new AgentWorkFlowBook(modelo, "researcher")
          .WithPersona("Você é um assistente de pesquisa experiente.")
          .WithInstructions("Pesquise informações relevantes sobre o tópico. Seja detalhado e preciso. Concentre-se em fatos e informações atualizadas.")
          .WithToolPacks(new SearchToolPack());
      var escritor = new AgentWorkFlowBook(modelo, "writer")
          .WithPersona("Você é um assistente de escrita profissional.")
          .WithInstructions("Escreva um rascunho estruturado e coerente baseado nas informações coletadas. Use linguagem clara e exemplos quando apropriado.");
      var revisor = new AgentWorkFlowBook(modelo, "editor")
          .WithPersona("Você é um assistente de revisão rigoroso.")
          .WithInstructions("Revise o rascunho criticamente avaliando qualidade, coerência e precisão. Corrija problemas e melhore o texto para sua versão final.");

      // Cria contexto inicial
      var context = new ConsoleWorkflowContext
      {
        Topic = "Inteligência Artificial",
        InitialInput = "Conceitos fundamentais de IA e suas aplicações modernas"
      };

      // Configura o workflow usando a API fluente com raciocínio melhorado
      var workflow = new SequentialWorkflow<ConsoleWorkflowContext, string>("Escrita de Artigos")
          .ForTask(ctx => $"Criar um texto completo sobre {ctx.Topic} com dados conteúdo: {ctx.InitialInput}")
          .RegisterStep("Pesquisa", pesquisador,
              ctx => $"Realize uma pesquisa aprofundada sobre: {ctx.Topic}. Inclua conceitos fundamentais, história, aplicações modernas e tendências futuras.",
              (ctx, res) =>
              {
                ctx.Summary = res;
              })
          .RegisterStep("Escrita", escritor,
              ctx => $"Escreva um texto bem estruturado sobre {ctx.Topic} baseado nesta pesquisa:\n\n{ctx.Summary}",
              (ctx, res) =>
              {
                ctx.Draft = res;
              })
          .RegisterStep("Revisão", revisor,
              ctx => $"Revise criticamente este texto sobre {ctx.Topic}:\n\n{ctx.Draft}\n\nCorrija problemas, melhore a clareza e coerência, e forneça a versão final aprimorada.",
              (ctx, res) =>
              {
                ctx.FinalText = res;
              });

      // Executa o workflow
      Console.WriteLine("Iniciando execução do workflow com avaliação de qualidade entre etapas...\n");
      var resultContext = await workflow.ExecuteAsync(context);

      // Exibe resultado
      Console.WriteLine($"Texto final: {resultContext.FinalText}\n");

      Console.WriteLine($"\nMetadados da tarefa: Descrição={workflow.Name}, Objetivo={workflow.TaskGoal}");

      Console.WriteLine("\n=== Detalhes do Workflow ===");
      foreach (var step in workflow.Steps)
      {
        Console.WriteLine($"- {step.Name}: {step.Agent.Name}");
        Console.WriteLine($"  Descrição: {step.Agent.description}");
        Console.WriteLine($"  Saída: {step.Result}");
        Console.WriteLine();
      }
    }
  }
}
