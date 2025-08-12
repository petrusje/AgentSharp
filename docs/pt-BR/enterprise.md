# 🏢 Casos Empresariais - AgentSharp

Este guia apresenta implementações práticas do AgentSharp em ambientes empresariais, com foco em produção, escalabilidade e casos de uso reais.

## 📋 Visão Geral Empresarial

O AgentSharp foi projetado para atender às necessidades específicas de ambientes corporativos, oferecendo:

- **Escalabilidade**: Suporte a milhões de interações simultâneas
- **Segurança**: Controle granular de acesso e auditoria
- **Performance**: Arquitetura otimizada para alto throughput
- **Flexibilidade**: Adaptação a diferentes domínios empresariais

## 🏥 Setor de Saúde

### Assistente Médico Inteligente

```csharp
public class AssistenteMedicoService
{
    private readonly IStorage _storage;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger _logger;

    public AssistenteMedicoService(IConfiguration config, ILogger logger)
    {
        _logger = logger;
        _embeddingService = new OpenAIEmbeddingService(
            apiKey: config["OpenAI:ApiKey"],
            model: "text-embedding-3-small",
            logger: logger
        );
        
        _storage = new VectorSqliteVecStorage(
            connectionString: config.GetConnectionString("MedicalDatabase"),
            embeddingService: _embeddingService,
            dimensions: 1536
        );
    }

    public async Task<DiagnosticoResponse> AnalysarSintomasAsync(PacienteContext paciente, string sintomas)
    {
        var agenteMedico = new Agent<PacienteContext, DiagnosticoResponse>(
                new OpenAIModel("gpt-4", apiKey), "Dr. IA Especialista")
            .WithSemanticMemory(_storage, _embeddingService)
            .WithPersona(@"
                Você é um assistente médico especializado que:
                - Analisa sintomas com base em histórico médico completo
                - Considera exames anteriores e medicações em uso
                - Fornece sugestões de diagnóstico diferencial
                - Sempre recomenda consulta médica presencial
                - Mantém confidencialidade médica absoluta
            ")
            .WithContext(paciente);

        var resultado = await agenteMedico.ExecuteAsync(sintomas);
        
        // Log auditoria (LGPD/HIPAA compliance)
        await LogInteracaoMedica(paciente.PacienteId, sintomas, resultado.Data);
        
        return resultado.Data;
    }

    private async Task LogInteracaoMedica(string pacienteId, string entrada, DiagnosticoResponse resposta)
    {
        var auditLog = new
        {
            Timestamp = DateTime.UtcNow,
            PacienteId = pacienteId,
            TipoInteracao = "AssistenteMedico",
            InputHash = ComputeHash(entrada), // Não gravar texto completo
            ResponseHash = ComputeHash(resposta.ToString()),
            Confidence = resposta.Confianca
        };
        
        await _logger.LogAsync(LogLevel.Audit, JsonSerializer.Serialize(auditLog));
    }
}

public class PacienteContext
{
    public string PacienteId { get; set; }
    public string SessionId { get; set; }
    public string MedicoResponsavel { get; set; }
    public DateTime DataConsulta { get; set; }
}

public class DiagnosticoResponse
{
    public List<string> HipotesesDiagnosticas { get; set; }
    public List<string> ExamesRecomendados { get; set; }
    public double Confianca { get; set; }
    public string RecomendacaoUrgencia { get; set; }
}
```

### Sistema de Análise de Laudos

```csharp
public class AnalisadorLaudosService
{
    public async Task<AnaliseResponse> AnalisarLaudoAsync(string laudo, string especialidade)
    {
        var analisador = new Agent<EspecialidadeContext, AnaliseResponse>(
                model, "Especialista em Laudos")
            .WithSemanticMemory(storage)
            .WithPersona($@"
                Especialista em {especialidade} que:
                - Analisa laudos com precisão técnica
                - Identifica achados relevantes e anormalidades
                - Correlaciona com padrões conhecidos
                - Sugere follow-up quando apropriado
            ")
            .WithContext(new EspecialidadeContext { Especialidade = especialidade });

        return (await analisador.ExecuteAsync($"Analisar laudo: {laudo}")).Data;
    }
}
```

## ⚖️ Setor Jurídico

### Consultor Jurídico Especializado

```csharp
public class ConsultorJuridicoService
{
    private readonly Agent<CasoJuridico, ConsultaResponse> _consultorContratual;
    private readonly Agent<CasoJuridico, ConsultaResponse> _consultorTrabalhista;
    private readonly Agent<CasoJuridico, ConsultaResponse> _consultorTributario;

    public ConsultorJuridicoService(IServiceProvider serviceProvider)
    {
        var storage = serviceProvider.GetService<IStorage>();
        var model = serviceProvider.GetService<IModel>();

        _consultorContratual = new Agent<CasoJuridico, ConsultaResponse>(model, "Especialista Contratual")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Advogado especialista em direito contratual que:
                - Analisa cláusulas e termos contratuais
                - Identifica riscos e oportunidades
                - Baseeia análises em jurisprudência atual
                - Fornece recomendações práticas
            ");

        _consultorTrabalhista = new Agent<CasoJuridico, ConsultaResponse>(model, "Especialista Trabalhista")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Advogado especialista em direito trabalhista que:
                - Interpreta CLT e convenções coletivas
                - Analisa riscos trabalhistas
                - Calcula verbas rescisórias
                - Orienta sobre compliance trabalhista
            ");

        _consultorTributario = new Agent<CasoJuridico, ConsultaResponse>(model, "Especialista Tributário")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Advogado especialista em direito tributário que:
                - Interpreta legislação fiscal complexa
                - Identifica oportunidades de economia fiscal
                - Analisa riscos tributários
                - Orienta sobre planejamento tributário
            ");
    }

    public async Task<ConsultaResponse> ConsultarAsync(CasoJuridico caso)
    {
        var especialista = caso.AreaDireito switch
        {
            AreaDireito.Contratual => _consultorContratual,
            AreaDireito.Trabalhista => _consultorTrabalhista,
            AreaDireito.Tributario => _consultorTributario,
            _ => throw new ArgumentException("Área do direito não suportada")
        };

        return (await especialista.ExecuteAsync(caso.Descricao, caso)).Data;
    }
}

public class CasoJuridico
{
    public string CasoId { get; set; }
    public AreaDireito AreaDireito { get; set; }
    public string Descricao { get; set; }
    public string ClienteId { get; set; }
    public DateTime DataAbertura { get; set; }
}

public enum AreaDireito
{
    Contratual,
    Trabalhista,
    Tributario,
    Empresarial,
    Civil
}
```

## 🏢 Setor Financeiro

### Sistema de Análise de Crédito

```csharp
public class AnaliseCreditoService
{
    private readonly TeamOrchestrator<AnaliseContext, DecisaoCredito> _teamAnalise;

    public AnaliseCreditoService(IServiceProvider serviceProvider)
    {
        var model = serviceProvider.GetService<IModel>();
        var storage = serviceProvider.GetService<IStorage>();

        var analistaFinanceiro = new Agent<AnaliseContext, string>(model, "Analista Financeiro")
            .WithSemanticMemory(storage)
            .WithPersona("Especialista em análise de demonstrações financeiras e fluxo de caixa");

        var analistaRisco = new Agent<AnaliseContext, string>(model, "Analista de Risco")
            .WithSemanticMemory(storage)
            .WithPersona("Especialista em avaliação de riscos de crédito e mercado");

        var gerente = new Agent<AnaliseContext, DecisaoCredito>(model, "Gerente de Crédito")
            .WithSemanticMemory(storage)
            .WithPersona("Gerente que toma decisões finais baseadas em análises especializadas");

        _teamAnalise = new TeamOrchestrator<AnaliseContext, DecisaoCredito>()
            .AddAgent("financeiro", analistaFinanceiro)
            .AddAgent("risco", analistaRisco)
            .AddAgent("gerente", gerente)
            .WithMode(TeamMode.Coordinate)
            .WithCoordinator("gerente");
    }

    public async Task<DecisaoCredito> AnalisarCreditoAsync(SolicitacaoCredito solicitacao)
    {
        var contexto = new AnaliseContext
        {
            ClienteId = solicitacao.ClienteId,
            ValorSolicitado = solicitacao.Valor,
            Finalidade = solicitacao.Finalidade,
            DadosFinanceiros = solicitacao.DadosFinanceiros
        };

        var resultado = await _teamAnalise.ExecuteAsync(
            $"Analisar solicitação de crédito: {JsonSerializer.Serialize(solicitacao)}", 
            contexto
        );

        return resultado.Data;
    }
}
```

## 🔧 Setor de Tecnologia

### Sistema de Code Review Automatizado

```csharp
public class CodeReviewService
{
    private readonly Agent<CodeContext, ReviewResponse> _revisorSeguranca;
    private readonly Agent<CodeContext, ReviewResponse> _revisorPerformance;
    private readonly Agent<CodeContext, ReviewResponse> _revisorQualidade;

    public CodeReviewService(IServiceProvider serviceProvider)
    {
        var model = serviceProvider.GetService<IModel>();
        var storage = serviceProvider.GetService<IStorage>();

        _revisorSeguranca = new Agent<CodeContext, ReviewResponse>(model, "Security Reviewer")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Expert security reviewer focused on:
                - SQL injection and XSS vulnerabilities
                - Authentication and authorization flaws
                - Data encryption and privacy compliance
                - Secure coding best practices
            ");

        _revisorPerformance = new Agent<CodeContext, ReviewResponse>(model, "Performance Reviewer")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Performance optimization expert focused on:
                - Algorithm complexity and efficiency
                - Memory usage and garbage collection
                - Database query optimization
                - Async/await patterns and threading
            ");

        _revisorQualidade = new Agent<CodeContext, ReviewResponse>(model, "Quality Reviewer")
            .WithSemanticMemory(storage)
            .WithPersona(@"
                Code quality expert focused on:
                - Clean code principles and SOLID
                - Design patterns and architecture
                - Unit testing and code coverage
                - Documentation and maintainability
            ");
    }

    public async Task<CodeReviewResult> ReviewCodeAsync(string codigo, string linguagem, string contexto)
    {
        var codeContext = new CodeContext
        {
            Code = codigo,
            Language = linguagem,
            Context = contexto,
            Timestamp = DateTime.UtcNow
        };

        var tasks = new[]
        {
            _revisorSeguranca.ExecuteAsync("Revisar código para vulnerabilidades de segurança", codeContext),
            _revisorPerformance.ExecuteAsync("Revisar código para otimizações de performance", codeContext),
            _revisorQualidade.ExecuteAsync("Revisar código para qualidade e manutenibilidade", codeContext)
        };

        var results = await Task.WhenAll(tasks);

        return new CodeReviewResult
        {
            SecurityReview = results[0].Data,
            PerformanceReview = results[1].Data,
            QualityReview = results[2].Data,
            OverallScore = CalculateOverallScore(results.Select(r => r.Data))
        };
    }
}
```

## 📊 Configuração para Produção

### Configuração de Alta Disponibilidade

```csharp
public class ProductionAgentConfiguration
{
    public static IServiceCollection ConfigureAgentSharp(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Storage de produção com replicação
        services.AddSingleton<IStorage>(provider =>
        {
            var logger = provider.GetService<ILogger<VectorSqliteVecStorage>>();
            var embeddingService = provider.GetService<IEmbeddingService>();
            
            return new VectorSqliteVecStorage(
                connectionString: configuration.GetConnectionString("AgentSharpProduction"),
                embeddingService: embeddingService,
                dimensions: 1536,
                distanceMetric: "cosine"
            );
        });

        // Embedding service com cache distribuído
        services.AddSingleton<IEmbeddingService>(provider =>
        {
            var logger = provider.GetService<ILogger<OpenAIEmbeddingService>>();
            var cache = provider.GetService<IDistributedCache>();
            
            var baseService = new OpenAIEmbeddingService(
                apiKey: configuration["OpenAI:ApiKey"],
                model: "text-embedding-3-small",
                logger: logger
            );
            
            return new CachedEmbeddingService(baseService, cache);
        });

        // Pool de agentes para alta concorrência
        services.AddSingleton<IAgentPool<string, string>>(provider =>
        {
            return new AgentPool<string, string>(
                factory: () => CreateStandardAgent(provider),
                minSize: 10,
                maxSize: 100,
                idleTimeout: TimeSpan.FromMinutes(5)
            );
        });

        return services;
    }
}
```

### Monitoramento e Observabilidade

```csharp
public class AgentMetricsCollector
{
    private readonly IMetricsLogger _metricsLogger;
    private readonly Counter _requestCounter;
    private readonly Histogram _responseTimeHistogram;
    private readonly Gauge _activeAgentsGauge;

    public AgentMetricsCollector(IMetricsLogger metricsLogger)
    {
        _metricsLogger = metricsLogger;
        _requestCounter = metricsLogger.CreateCounter("agent_requests_total");
        _responseTimeHistogram = metricsLogger.CreateHistogram("agent_response_time_seconds");
        _activeAgentsGauge = metricsLogger.CreateGauge("active_agents");
    }

    public async Task<T> MeasureAgentExecution<T>(
        string agentName, 
        Func<Task<T>> execution)
    {
        var stopwatch = Stopwatch.StartNew();
        _requestCounter.WithTag("agent", agentName).Increment();
        _activeAgentsGauge.Increment();

        try
        {
            var result = await execution();
            _requestCounter.WithTag("agent", agentName).WithTag("status", "success").Increment();
            return result;
        }
        catch (Exception ex)
        {
            _requestCounter.WithTag("agent", agentName).WithTag("status", "error").Increment();
            _metricsLogger.LogException(ex, agentName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _responseTimeHistogram.WithTag("agent", agentName).Observe(stopwatch.Elapsed.TotalSeconds);
            _activeAgentsGauge.Decrement();
        }
    }
}
```

## 🔒 Segurança Empresarial

### Controle de Acesso e Auditoria

```csharp
public class SecureAgentWrapper<TContext, TResult>
{
    private readonly Agent<TContext, TResult> _agent;
    private readonly IAuthorizationService _authorization;
    private readonly IAuditLogger _auditLogger;
    private readonly IDataProtectionProvider _dataProtection;

    public SecureAgentWrapper(
        Agent<TContext, TResult> agent,
        IAuthorizationService authorization,
        IAuditLogger auditLogger,
        IDataProtectionProvider dataProtection)
    {
        _agent = agent;
        _authorization = authorization;
        _auditLogger = auditLogger;
        _dataProtection = dataProtection;
    }

    public async Task<AgentResult<TResult>> ExecuteSecureAsync(
        string prompt, 
        TContext context, 
        ClaimsPrincipal user)
    {
        // Verificar autorização
        var authResult = await _authorization.AuthorizeAsync(user, context, "AgentAccess");
        if (!authResult.Succeeded)
        {
            throw new UnauthorizedAccessException("Acesso negado ao agente");
        }

        // Sanitizar entrada
        var sanitizedPrompt = SanitizeInput(prompt);

        // Proteger dados sensíveis no contexto
        var protectedContext = await ProtectSensitiveData(context);

        // Log de auditoria (início)
        await _auditLogger.LogAgentAccessAsync(user.Identity.Name, _agent.Name, "START");

        try
        {
            var result = await _agent.ExecuteAsync(sanitizedPrompt, protectedContext);
            
            // Log de auditoria (sucesso)
            await _auditLogger.LogAgentAccessAsync(user.Identity.Name, _agent.Name, "SUCCESS");
            
            return result;
        }
        catch (Exception ex)
        {
            // Log de auditoria (erro)
            await _auditLogger.LogAgentAccessAsync(user.Identity.Name, _agent.Name, "ERROR", ex.Message);
            throw;
        }
    }

    private string SanitizeInput(string input)
    {
        // Remover potenciais injeções
        return input
            .Replace("<script>", "")
            .Replace("</script>", "")
            .Replace("DROP TABLE", "")
            .Trim();
    }

    private async Task<TContext> ProtectSensitiveData(TContext context)
    {
        // Implementar criptografia de dados sensíveis
        var protector = _dataProtection.CreateProtector("AgentContext");
        
        // Serializar, proteger e deserializar contexto
        var json = JsonSerializer.Serialize(context);
        var protectedJson = protector.Protect(json);
        
        // Retornar contexto com dados protegidos
        return JsonSerializer.Deserialize<TContext>(protectedJson);
    }
}
```

## 📈 Dimensionamento e Performance

### Configuração para Alto Volume

```csharp
public class HighVolumeAgentConfiguration
{
    public static void ConfigureForHighVolume(IServiceCollection services, IConfiguration config)
    {
        // Connection pooling para storage
        services.AddDbContextPool<AgentDbContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("AgentSharp"), sqlite =>
            {
                sqlite.CommandTimeout(30);
            });
        }, poolSize: 100);

        // Cache distribuído para embeddings
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("Redis");
            options.InstanceName = "AgentSharp";
        });

        // Rate limiting
        services.AddRateLimiting(options =>
        {
            options.AddFixedWindowLimiter("AgentAPI", configure =>
            {
                configure.PermitLimit = 1000;
                configure.Window = TimeSpan.FromMinutes(1);
                configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                configure.QueueLimit = 100;
            });
        });

        // Background processing para tarefas assíncronas
        services.AddHangfire(configuration =>
        {
            configuration.UseSqlServerStorage(config.GetConnectionString("HangfireDb"));
        });
    }
}
```

## 🎯 Próximos Passos

1. **[Sistema de Memória](memory-system.md)** - Configuração otimizada de storage
2. **[Workflows Avançados](workflows.md)** - Orquestração multi-agente
3. **[API Reference](api/core/agent.md)** - Documentação técnica completa
4. **[Exemplos Práticos](examples.md)** - Mais casos de uso implementados

---

> **💡 Recomendação**: Sempre implemente logging de auditoria, controle de acesso e monitoramento em ambientes de produção. O AgentSharp oferece todas as hooks necessárias para integração com sistemas empresariais existentes.