# Sistema de Ferramentas no AgentSharp

O sistema de ferramentas (Tools) do AgentSharp permite que agentes executem ações no mundo real através de function calling. Os agentes podem chamar APIs, consultar bancos de dados, executar cálculos e muito mais, tudo de forma inteligente e contextual.

## Conceitos Fundamentais

### Function Calling
O **function calling** é a capacidade do LLM de decidir quando e como chamar funções específicas durante uma conversa:

```csharp
// O LLM pode decidir chamar esta função automaticamente
[FunctionCall("calcular_preco_com_desconto")]
public decimal CalcularPrecoComDesconto(decimal preco, decimal percentualDesconto)
{
    return preco * (1 - percentualDesconto / 100);
}

// Em uma conversa:
// Usuário: "Quanto fica um produto de R$ 100 com 20% de desconto?"
// LLM: *decide chamar calcular_preco_com_desconto(100, 20)*
// Função: retorna 80
// LLM: "Com 20% de desconto, o produto de R$ 100 fica R$ 80"
```

### Arquitetura do Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                     SISTEMA DE FERRAMENTAS                 │
├─────────────────────────────────────────────────────────────┤
│  🤖 Agent Layer                                             │
│  ├─ RegisterTool()          ├─ RegisterToolPack()          │
│  ├─ RegisterAgentMethods()  ├─ Auto-discovery              │
│  └─ Function Call Context   └─ Parameter Injection         │
├─────────────────────────────────────────────────────────────┤
│  🛠️ Tool Manager                                            │
│  ├─ Function Discovery     ├─ Parameter Validation         │
│  ├─ Schema Generation      ├─ Execution Management         │
│  ├─ Error Handling         └─ Result Processing            │
├─────────────────────────────────────────────────────────────┤
│  📦 Tool Packs                                             │
│  ├─ SmartMemoryToolPack    ├─ FinanceToolPack              │
│  ├─ SearchToolPack         ├─ TeamHandoffToolPack          │
│  ├─ ReasoningToolPack      └─ Custom ToolPacks             │
├─────────────────────────────────────────────────────────────┤
│  🔧 Individual Tools                                       │
│  ├─ [FunctionCall] Methods ├─ Parameter Binding           │
│  ├─ Async Support          ├─ Return Type Handling        │
│  └─ Context Injection      └─ Error Management            │
└─────────────────────────────────────────────────────────────┘
```

## Tipos de Ferramentas

### 1. Métodos Diretos do Agente

Métodos definidos diretamente na classe do agente:

```csharp
public class AgentCalculadora : Agent<object, string>
{
    public AgentCalculadora(IModel model) : base(model, "Calculadora") { }
    
    [FunctionCall("somar")]
    [FunctionCallParameter("a", "Primeiro número")]
    [FunctionCallParameter("b", "Segundo número")]
    public double Somar(double a, double b)
    {
        return a + b;
    }
    
    [FunctionCall("calcular_porcentagem")]
    public double CalcularPorcentagem(double valor, double percentual)
    {
        return valor * (percentual / 100);
    }
}

// Uso automático
var calculadora = new AgentCalculadora(modelo);
await calculadora.ExecuteAsync("Quanto é 15% de 200?");
// LLM chamará automaticamente calcular_porcentagem(200, 15)
```

### 2. ToolPacks

Coleções organizadas de ferramentas relacionadas:

```csharp
public class FinanceToolPack : ToolPack
{
    [FunctionCall("calcular_juros_compostos")]
    public decimal CalcularJurosCompostos(
        decimal principal, 
        decimal taxa, 
        int periodos)
    {
        return principal * (decimal)Math.Pow(1 + (double)taxa, periodos);
    }
    
    [FunctionCall("converter_moeda")]
    public async Task<decimal> ConverterMoeda(
        decimal valor, 
        string moedaOrigem, 
        string moedaDestino)
    {
        // Integração com API de câmbio
        var taxa = await ObterTaxaCambio(moedaOrigem, moedaDestino);
        return valor * taxa;
    }
    
    [FunctionCall("analisar_investimento")]
    public AnaliseInvestimento AnalisarInvestimento(
        decimal valorInicial,
        decimal aportesMensais,
        decimal taxaRetorno,
        int anos)
    {
        var meses = anos * 12;
        var montante = CalcularJurosCompostos(valorInicial, taxaRetorno / 12, meses);
        
        // Adicionar aportes mensais
        for (int i = 1; i <= meses; i++)
        {
            montante += CalcularJurosCompostos(aportesMensais, taxaRetorno / 12, meses - i);
        }
        
        return new AnaliseInvestimento
        {
            ValorFinal = montante,
            TotalInvestido = valorInicial + (aportesMensais * meses),
            Rendimento = montante - valorInicial - (aportesMensais * meses),
            PercentualRetorno = ((montante / (valorInicial + aportesMensais * meses)) - 1) * 100
        };
    }
}

// Registro no agente
var conselheiro = new Agent<object, string>(modelo, "ConselheiroFinanceiro");
conselheiro.WithTools(new FinanceToolPack());

await conselheiro.ExecuteAsync(
    "Se eu investir R$ 1000 iniciais e R$ 500 por mês a 1% ao mês, quanto terei em 2 anos?"
);
```

### 3. Ferramentas Individuais

Ferramentas específicas registradas individualmente:

```csharp
// Ferramenta personalizada
public class WeatherTool : Tool
{
    [FunctionCall("obter_clima")]
    public async Task<WeatherInfo> ObterClima(string cidade)
    {
        // Integração com API de clima
        var client = new HttpClient();
        var response = await client.GetAsync($"https://api.weather.com/v1/current?city={cidade}");
        var data = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<WeatherInfo>(data);
    }
}

// Registro individual
var agente = new Agent<object, string>(modelo, "AssistenteClima");
agente.RegisterTool<WeatherTool>();
```

## ToolPacks Inclusos

### 1. SmartMemoryToolPack

Permite ao LLM gerenciar memórias diretamente:

```csharp
public class SmartMemoryToolPack : ToolPack
{
    [FunctionCall("adicionar_memoria")]
    public async Task<string> AdicionarMemoria(string conteudo);
    
    [FunctionCall("buscar_memorias")]  
    public async Task<List<UserMemory>> BuscarMemorias(string consulta, int limite = 5);
    
    [FunctionCall("atualizar_memoria")]
    public async Task<string> AtualizarMemoria(string id, string novoConteudo);
    
    [FunctionCall("remover_memoria")]
    public async Task<string> RemoverMemoria(string id);
}

// Registrado automaticamente quando memória está habilitada
var agente = new Agent<object, string>(modelo, "MemoriaInteligente")
    .WithUserMemories();            // SmartMemoryToolPack habilitado automaticamente
```

### 2. SearchToolPack

Ferramentas para busca na web e APIs:

```csharp
public class SearchToolPack : ToolPack
{
    [FunctionCall("buscar_web")]
    public async Task<List<SearchResult>> BuscarWeb(string query, int maxResults = 10);
    
    [FunctionCall("buscar_noticias")]
    public async Task<List<NewsArticle>> BuscarNoticias(string topico, string idioma = "pt-BR");
    
    [FunctionCall("buscar_wikipedia")]
    public async Task<string> BuscarWikipedia(string termo);
}
```

### 3. ReasoningToolPack

Ferramentas para raciocínio estruturado:

```csharp
public class ReasoningToolPack : ToolPack
{
    [FunctionCall("iniciar_raciocinio")]
    public ReasoningStep IniciarRaciocinio(string problema);
    
    [FunctionCall("adicionar_passo")]
    public ReasoningStep AdicionarPasso(string raciocinio, string conclusao);
    
    [FunctionCall("finalizar_raciocinio")]
    public string FinalizarRaciocinio(List<ReasoningStep> passos);
}
```

### 4. TeamHandoffToolPack

Ferramentas para transferência entre agentes:

```csharp
public class TeamHandoffToolPack : ToolPack
{
    [FunctionCall("transferir_para_agente")]
    public async Task<string> TransferirParaAgente(string nomeAgente, string contexto);
    
    [FunctionCall("solicitar_ajuda")]
    public async Task<string> SolicitarAjuda(string especialidade, string problema);
    
    [FunctionCall("listar_agentes_disponiveis")]
    public List<AgentInfo> ListarAgentesDisponiveis();
}
```

## Criando ToolPacks Personalizados

### Exemplo: Sistema de E-commerce

```csharp
public class EcommerceToolPack : ToolPack
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    
    public EcommerceToolPack(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
    }
    
    [FunctionCall("buscar_produtos")]
    [FunctionCallParameter("categoria", "Categoria do produto (ex: eletrônicos, roupas)")]
    [FunctionCallParameter("precoMaximo", "Preço máximo em reais")]
    [FunctionCallParameter("ordenarPor", "Como ordenar: preco, nome, avaliacao")]
    public async Task<List<Product>> BuscarProdutos(
        string categoria = null, 
        decimal? precoMaximo = null,
        string ordenarPor = "nome")
    {
        return await _productService.SearchAsync(categoria, precoMaximo, ordenarPor);
    }
    
    [FunctionCall("obter_detalhes_produto")]
    public async Task<ProductDetail> ObterDetalhesProduto(int produtoId)
    {
        return await _productService.GetDetailAsync(produtoId);
    }
    
    [FunctionCall("verificar_estoque")]
    public async Task<StockInfo> VerificarEstoque(int produtoId)
    {
        return await _productService.CheckStockAsync(produtoId);
    }
    
    [FunctionCall("calcular_frete")]
    public async Task<ShippingCalculation> CalcularFrete(
        int produtoId, 
        string cep,
        int quantidade = 1)
    {
        return await _productService.CalculateShippingAsync(produtoId, cep, quantidade);
    }
    
    [FunctionCall("criar_pedido")]
    public async Task<Order> CriarPedido(
        List<int> produtoIds,
        List<int> quantidades, 
        string enderecoEntrega,
        string formaPagamento)
    {
        return await _orderService.CreateOrderAsync(produtoIds, quantidades, enderecoEntrega, formaPagamento);
    }
    
    [FunctionCall("rastrear_pedido")]
    public async Task<OrderStatus> RastrearPedido(string numeroPedido)
    {
        return await _orderService.TrackOrderAsync(numeroPedido);
    }
    
    [FunctionCall("calcular_desconto")]
    public decimal CalcularDesconto(decimal valorPedido, string cupom = null)
    {
        // Lógica de desconto baseada no valor e cupom
        var desconto = 0m;
        
        if (valorPedido > 200) desconto += valorPedido * 0.1m;  // 10% acima de R$ 200
        if (valorPedido > 500) desconto += valorPedido * 0.05m; // +5% acima de R$ 500
        
        if (!string.IsNullOrEmpty(cupom))
        {
            switch (cupom.ToUpper())
            {
                case "BEMVINDO": desconto += 50m; break;
                case "FIDELIDADE": desconto += valorPedido * 0.15m; break;
            }
        }
        
        return Math.Min(desconto, valorPedido * 0.5m); // Máx. 50% de desconto
    }
}

// Uso no agente
public class AtendenteVirtualEcommerce : Agent<PedidoContext, RespostaPedido>
{
    public AtendenteVirtualEcommerce(IModel model, IProductService products, IOrderService orders) 
        : base(model, "AtendenteVirtual")
    {
        // Registrar o ToolPack personalizado
        RegisterToolPack(new EcommerceToolPack(products, orders));
        
        // Configurar persona
        this.WithPersona(@"
            Você é um atendente virtual especializado em e-commerce.
            Ajude clientes a encontrar produtos, calcular preços, verificar estoques e criar pedidos.
            Seja proativo em sugerir produtos relacionados e ofertas especiais.
            Sempre confirme detalhes importantes antes de criar pedidos.
        ");
    }
}

// Exemplo de uso
class Program
{
    static async Task Main()
    {
        var productService = new ProductService();
        var orderService = new OrderService();
        
        var atendente = new AtendenteVirtualEcommerce(modelo, productService, orderService);
        
        // Cliente interessado em comprar
        await atendente.ExecuteAsync(
            "Estou procurando um smartphone até R$ 1500, qual você recomenda?",
            userId: "cliente123",
            sessionId: "compra2024001"
        );
        
        // O agente irá:
        // 1. Chamar buscar_produtos("eletrônicos", 1500, "avaliacao")
        // 2. Analisar os resultados
        // 3. Apresentar opções ao cliente
        // 4. Oferecer verificar estoque e calcular frete
        
        // Continuação da conversa
        await atendente.ExecuteAsync(
            "Gostei do iPhone 13. Tem em estoque? Quanto fica o frete para 01310-100?",
            userId: "cliente123", 
            sessionId: "compra2024001"
        );
        
        // O agente irá:
        // 1. Chamar verificar_estoque(id_iphone_13)
        // 2. Chamar calcular_frete(id_iphone_13, "01310-100")
        // 3. Apresentar disponibilidade e opções de entrega
    }
}
```

## Ferramentas Avançadas

### 1. Ferramentas Assíncronas

```csharp
public class IntegrationToolPack : ToolPack
{
    [FunctionCall("enviar_email")]
    public async Task<bool> EnviarEmail(string destinatario, string assunto, string corpo)
    {
        var client = new SmtpClient();
        var message = new MailMessage("sistema@empresa.com", destinatario, assunto, corpo);
        await client.SendMailAsync(message);
        return true;
    }
    
    [FunctionCall("salvar_arquivo")]
    public async Task<string> SalvarArquivo(string nomeArquivo, string conteudo)
    {
        var path = Path.Combine("uploads", nomeArquivo);
        await File.WriteAllTextAsync(path, conteudo);
        return path;
    }
}
```

### 2. Ferramentas com Injeção de Contexto

O AgentSharp automaticamente injeta contexto nas ferramentas:

```csharp
public class ContextAwareToolPack : ToolPack
{
    [FunctionCall("salvar_preferencia_usuario")]
    public async Task<string> SalvarPreferenciaUsuario(
        string preferencia,
        string valor,
        // Contexto injetado automaticamente
        [FromContext] string userId,
        [FromContext] string sessionId,
        [FromContext] IMemoryManager memoryManager)
    {
        await memoryManager.AddMemoryAsync($"Preferência {preferencia}: {valor}");
        return $"Preferência salva para usuário {userId}";
    }
}
```

### 3. Validação de Parâmetros

```csharp
public class ValidatedToolPack : ToolPack
{
    [FunctionCall("criar_conta_bancaria")]
    public async Task<string> CriarContaBancaria(
        [Required] string nomeTitular,
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve ter 11 dígitos")] 
        string cpf,
        [Range(18, 100, ErrorMessage = "Idade deve estar entre 18 e 100 anos")]
        int idade,
        [EmailAddress]
        string email)
    {
        // Tool só executa se validação passar
        return $"Conta criada para {nomeTitular}";
    }
}
```

## Tratamento de Erros

```csharp
public class RobustToolPack : ToolPack
{
    [FunctionCall("operacao_com_retry")]
    public async Task<ApiResponse> OperacaoComRetry(string dados)
    {
        var maxTentativas = 3;
        var delay = TimeSpan.FromSeconds(1);
        
        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                return await ChamarApiExterna(dados);
            }
            catch (HttpRequestException ex) when (tentativa < maxTentativas)
            {
                await Task.Delay(delay);
                delay = delay.Add(delay); // Backoff exponencial
            }
            catch (Exception ex)
            {
                // Log do erro
                Logger.LogError($"Erro na tentativa {tentativa}: {ex.Message}");
                
                if (tentativa == maxTentativas)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = $"Falha após {maxTentativas} tentativas: {ex.Message}"
                    };
                }
            }
        }
        
        return new ApiResponse { Success = false, ErrorMessage = "Erro inesperado" };
    }
}
```

## Monitoramento e Telemetria

```csharp
public class MonitoredToolPack : ToolPack
{
    private readonly ITelemetryService _telemetry;
    
    public MonitoredToolPack(ITelemetryService telemetry)
    {
        _telemetry = telemetry;
    }
    
    [FunctionCall("operacao_monitorada")]
    public async Task<string> OperacaoMonitorada(string parametro)
    {
        var operationId = $"tool_operacao_monitorada_{Guid.NewGuid()}";
        
        _telemetry.StartOperation(operationId);
        
        try
        {
            // Simular operação
            await Task.Delay(100);
            var resultado = $"Processado: {parametro}";
            
            _telemetry.TrackToolExecution("operacao_monitorada", 0.1, 50);
            
            return resultado;
        }
        finally
        {
            _telemetry.EndOperation(operationId);
        }
    }
}
```

## Exemplo Completo: Sistema de Suporte Técnico

```csharp
public class SupportSystem
{
    public class TechnicalSupportToolPack : ToolPack
    {
        private readonly ITicketService _tickets;
        private readonly IKnowledgeBase _kb;
        
        public TechnicalSupportToolPack(ITicketService tickets, IKnowledgeBase kb)
        {
            _tickets = tickets;
            _kb = kb;
        }
        
        [FunctionCall("criar_ticket")]
        public async Task<Ticket> CriarTicket(
            string titulo,
            string descricao, 
            string prioridade = "media",
            [FromContext] string userId = null)
        {
            return await _tickets.CreateAsync(new Ticket
            {
                Title = titulo,
                Description = descricao,
                Priority = prioridade,
                UserId = userId,
                Status = "aberto",
                CreatedAt = DateTime.Now
            });
        }
        
        [FunctionCall("buscar_solucoes")]
        public async Task<List<KnowledgeArticle>> BuscarSolucoes(string problema)
        {
            return await _kb.SearchAsync(problema, limit: 5);
        }
        
        [FunctionCall("diagnosticar_problema")]
        public DiagnosticResult DiagnosticarProblema(
            string sintomas,
            string sistemaOperacional = null,
            string versaoSoftware = null)
        {
            // Lógica de diagnóstico baseada em sintomas
            var possiveisCausas = new List<string>();
            var solucoesRecomendadas = new List<string>();
            
            if (sintomas.Contains("lento", StringComparison.OrdinalIgnoreCase))
            {
                possiveisCausas.Add("Falta de memória RAM");
                possiveisCausas.Add("Disco cheio");
                solucoesRecomendadas.Add("Verificar uso de memória");
                solucoesRecomendadas.Add("Limpar arquivos temporários");
            }
            
            if (sintomas.Contains("erro", StringComparison.OrdinalIgnoreCase))
            {
                possiveisCausas.Add("Arquivo corrompido");
                possiveisCausas.Add("Configuração incorreta");
                solucoesRecomendadas.Add("Reinstalar aplicação");
                solucoesRecomendadas.Add("Verificar logs de erro");
            }
            
            return new DiagnosticResult
            {
                Symptoms = sintomas,
                PossibleCauses = possiveisCausas,
                RecommendedSolutions = solucoesRecomendadas,
                Severity = DeterminarSeveridade(sintomas)
            };
        }
        
        [FunctionCall("escalar_ticket")]
        public async Task<bool> EscalarTicket(
            int ticketId, 
            string nivel = "nivel2",
            string motivo = "Requer análise especializada")
        {
            return await _tickets.EscalateAsync(ticketId, nivel, motivo);
        }
        
        [FunctionCall("atualizar_status_ticket")]
        public async Task<bool> AtualizarStatusTicket(int ticketId, string novoStatus)
        {
            return await _tickets.UpdateStatusAsync(ticketId, novoStatus);
        }
        
        private string DeterminarSeveridade(string sintomas)
        {
            if (sintomas.Contains("sistema não inicia", StringComparison.OrdinalIgnoreCase) ||
                sintomas.Contains("perda de dados", StringComparison.OrdinalIgnoreCase))
                return "critica";
                
            if (sintomas.Contains("funcionalidade indisponível", StringComparison.OrdinalIgnoreCase))
                return "alta";
                
            return "media";
        }
    }
    
    // Agente de Suporte Técnico
    public class TechnicalSupportAgent : Agent<SupportContext, SupportResponse>
    {
        public TechnicalSupportAgent(
            IModel model, 
            ITicketService tickets, 
            IKnowledgeBase kb) 
            : base(model, "SuporteTecnico")
        {
            RegisterToolPack(new TechnicalSupportToolPack(tickets, kb));
            
            this.WithPersona(@"
                Você é um especialista em suporte técnico experiente e paciente.
                Sempre siga este processo:
                1. Diagnostique o problema com base nos sintomas
                2. Busque soluções na base de conhecimento
                3. Ofereça soluções passo a passo
                4. Se necessário, crie um ticket para acompanhamento
                5. Escale para nível superior apenas se extremamente complexo
                
                Seja empático, claro e técnico quando necessário.
            ");
        }
    }
}

// Uso do sistema
class Program
{
    static async Task Main()
    {
        var tickets = new TicketService();
        var kb = new KnowledgeBase();
        
        var suporte = new TechnicalSupportAgent(modelo, tickets, kb);
        
        var contexto = new SupportContext
        {
            CustomerTier = "Premium",
            ProductVersion = "2024.1.0"
        };
        
        await suporte.ExecuteAsync(
            @"Olá, estou com um problema. Meu sistema está muito lento desde ontem, 
              especialmente ao abrir arquivos grandes. Às vezes trava completamente. 
              Uso Windows 11 e tenho 8GB de RAM.",
            contexto,
            userId: "cliente_premium_001",
            sessionId: "suporte_2024_001"
        );
        
        // O agente irá:
        // 1. Chamar diagnosticar_problema() com os sintomas
        // 2. Chamar buscar_solucoes() para encontrar artigos relevantes
        // 3. Apresentar diagnóstico e soluções passo a passo
        // 4. Possivelmente criar_ticket() se necessário acompanhamento
        // 5. Oferecer suporte adicional
    }
}
```

## Boas Práticas

### 1. Design de Funções

```csharp
// ✅ BOM: Função específica e bem documentada
[FunctionCall("calcular_parcela_financiamento")]
[FunctionCallParameter("valor", "Valor total do financiamento em reais")]
[FunctionCallParameter("taxa", "Taxa de juros mensal (ex: 0.015 para 1.5%)")]  
[FunctionCallParameter("parcelas", "Número de parcelas")]
public decimal CalcularParcelaFinanciamento(decimal valor, decimal taxa, int parcelas);

// ❌ RUIM: Função genérica demais
[FunctionCall("calcular")]
public decimal Calcular(string tipo, params object[] valores);
```

### 2. Tratamento de Erros

```csharp
[FunctionCall("operacao_segura")]
public async Task<OperationResult> OperacaoSegura(string input)
{
    try
    {
        var resultado = await ProcessarInput(input);
        return new OperationResult { Success = true, Data = resultado };
    }
    catch (ValidationException ex)
    {
        return new OperationResult 
        { 
            Success = false, 
            ErrorMessage = $"Dados inválidos: {ex.Message}" 
        };
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erro em operacao_segura");
        return new OperationResult 
        { 
            Success = false, 
            ErrorMessage = "Erro interno. Contate o suporte." 
        };
    }
}
```

### 3. Performance

```csharp
// ✅ BOM: Cache para operações custosas
private readonly MemoryCache _cache = new();

[FunctionCall("buscar_dados_custosos")]
public async Task<DataResult> BuscarDadosCustosos(string query)
{
    var cacheKey = $"dados_{query.GetHashCode()}";
    
    if (_cache.TryGetValue(cacheKey, out DataResult cached))
        return cached;
    
    var resultado = await ExecutarOperacaoCustosa(query);
    
    _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(10));
    
    return resultado;
}
```

### 4. Segurança

```csharp
[FunctionCall("operacao_autenticada")]
public async Task<string> OperacaoAutenticada(
    string dados,
    [FromContext] string userId)
{
    // Verificar autorização
    if (!await _authService.CanUserAccessAsync(userId, "operacao_especial"))
    {
        throw new UnauthorizedAccessException("Usuário não autorizado");
    }
    
    // Sanitizar entrada
    dados = SecurityHelper.SanitizeInput(dados);
    
    return await ProcessarOperacao(dados);
}
```

## Próximos Passos

- **[Orquestração](./orchestration.md)** - Sistemas multi-agente e workflows
- **[API Reference - Tools](../api-reference/tools/tool-manager.md)** - Documentação completa
- **[Tutoriais - Custom Tools](../tutorials/custom-tools.md)** - Criando suas próprias ferramentas
- **[Exemplos Avançados](../examples/tool-examples.md)** - Casos de uso complexos

## Referências Técnicas

- **Function Calling**: Especificação OpenAI para chamadas de função
- **JSON Schema**: Validação automática de parâmetros
- **Dependency Injection**: Injeção de contexto e serviços
- **Error Handling**: Padrões de tratamento de erro resilientes