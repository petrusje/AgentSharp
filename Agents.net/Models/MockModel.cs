// Agents.net/Models/MockModel.cs (com correção do construtor)
using System;
using System.Threading;
using System.Threading.Tasks;
using Agents.net.Utils;

namespace Agents.net.Models
{
    /// <summary>
    /// Implementação de modelo mock para testes
    /// </summary>
    public class MockModel : Agents.net.Models.ModelBase
    {
        private readonly Func<ModelRequest, ModelConfiguration, CancellationToken, Task<ModelResponse>> _responseFactory;
        
        /// <summary>
        /// Cria uma instância de modelo mock
        /// </summary>
        /// <param name="responseFactory">Função que gera respostas mock</param>
        public MockModel(Func<ModelRequest, ModelConfiguration, CancellationToken, Task<ModelResponse>> responseFactory = null)
            : base("mock-model") // Passando o nome do modelo para o construtor base
        {
            _responseFactory = responseFactory;
        }
        
        // O resto do código permanece igual...
        public override async Task<ModelResponse> GenerateResponseAsync(
            ModelRequest request, 
            ModelConfiguration config, 
            CancellationToken cancellationToken = default)
        {
            Logger.Debug($"MockModel: Gerando resposta para {request.Messages.Count} mensagens");
            
            if (_responseFactory != null)
                return await _responseFactory(request, config, cancellationToken);
                
            // Resposta padrão se nenhuma fábrica for fornecida
            
            return new ModelResponse
            {
                Content = "Esta é uma resposta mock.",
                Usage = new UsageInfo
                {
                    PromptTokens = 10,
                    CompletionTokens = 5,
                    EstimatedCost = 0.0001m
                }
            };
        }
        
        /// <summary>
        /// Gera uma resposta em stream mock
        /// </summary>
        public override async Task<ModelResponse> GenerateStreamingResponseAsync(
            ModelRequest request,
            ModelConfiguration config,
            Action<string> handler,
            CancellationToken cancellationToken = default)
        {
            Logger.Debug($"MockModel: Gerando resposta em stream para {request.Messages.Count} mensagens");
            
            // Simula streaming enviando a resposta em partes
            var responseText = "Esta é uma resposta mock em streaming.";
            var chunks = responseText.Split(' ');
            
            foreach (var chunk in chunks)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                    
                handler?.Invoke(chunk + " ");
            }
            
            return new ModelResponse
            {
                Content = responseText,
                Usage = new UsageInfo
                {
                    PromptTokens = 10,
                    CompletionTokens = 7,
                    EstimatedCost = 0.0001m
                }
            };
        }
    }
}