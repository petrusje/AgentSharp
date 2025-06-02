using System;
using System.Collections.Generic;
using System.Threading;

namespace Agents.net.Core
{
    /// <summary>
    /// Contexto de execução que mantém estado isolado por operação
    /// Resolve o problema de contadores globais compartilhados
    /// </summary>
    public class ExecutionContext
    {
        private static readonly ThreadLocal<ExecutionContext> _current = new ThreadLocal<ExecutionContext>();
        
        /// <summary>
        /// ID único da execução
        /// </summary>
        public string ExecutionId { get; }
        
        /// <summary>
        /// Número de chamadas de modelo nesta execução
        /// </summary>
        public int CallCount { get; set; }
        
        /// <summary>
        /// Número de execuções de agente nesta execução
        /// </summary>
        public int ExecutionCount { get; set; }
        
        /// <summary>
        /// Configurações máximas para esta execução
        /// </summary>
        public int MaxCalls { get; set; } = 10;
        public int MaxExecutions { get; set; } = 3;
        
        /// <summary>
        /// Timestamp do início da execução
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        /// Profundidade de recursão atual
        /// </summary>
        public int RecursionDepth { get; set; }
        
        /// <summary>
        /// Limite máximo de recursão
        /// </summary>
        public int MaxRecursionDepth { get; set; } = 5;
        
        /// <summary>
        /// Metadados adicionais da execução
        /// </summary>
        public Dictionary<string, object> Metadata { get; }
        
        /// <summary>
        /// Contexto atual da thread
        /// </summary>
        public static ExecutionContext Current => _current.Value;
        
        /// <summary>
        /// Construtor privado - usar CreateNew()
        /// </summary>
        private ExecutionContext()
        {
            ExecutionId = Guid.NewGuid().ToString("N");
            StartTime = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Cria um novo contexto de execução e define como atual
        /// </summary>
        public static ExecutionContext CreateNew()
        {
            var context = new ExecutionContext();
            _current.Value = context;
            return context;
        }
        
        /// <summary>
        /// Define um contexto específico como atual
        /// </summary>
        public static void SetCurrent(ExecutionContext context)
        {
            _current.Value = context;
        }
        
        /// <summary>
        /// Limpa o contexto atual
        /// </summary>
        public static void ClearCurrent()
        {
            _current.Value = null;
        }
        
        /// <summary>
        /// Verifica se pode fazer mais chamadas ao modelo
        /// </summary>
        public bool CanMakeMoreCalls()
        {
            return CallCount < MaxCalls;
        }
        
        /// <summary>
        /// Verifica se pode fazer mais execuções
        /// </summary>
        public bool CanExecuteMore()
        {
            return ExecutionCount < MaxExecutions;
        }
        
        /// <summary>
        /// Verifica se pode fazer mais recursões
        /// </summary>
        public bool CanRecurse()
        {
            return RecursionDepth < MaxRecursionDepth;
        }
        
        /// <summary>
        /// Incrementa o contador de chamadas e verifica limites
        /// </summary>
        public void IncrementCallCount()
        {
            CallCount++;
            if (!CanMakeMoreCalls())
            {
                throw new InvalidOperationException($"Limite de chamadas ao modelo excedido ({MaxCalls}) na execução {ExecutionId}");
            }
        }
        
        /// <summary>
        /// Incrementa o contador de execuções e verifica limites
        /// </summary>
        public void IncrementExecutionCount()
        {
            ExecutionCount++;
            if (!CanExecuteMore())
            {
                throw new InvalidOperationException($"Limite de execuções excedido ({MaxExecutions}) na execução {ExecutionId}");
            }
        }
        
        /// <summary>
        /// Incrementa a profundidade de recursão e verifica limites
        /// </summary>
        public void IncrementRecursionDepth()
        {
            RecursionDepth++;
            if (!CanRecurse())
            {
                throw new InvalidOperationException($"Limite de recursão excedido ({MaxRecursionDepth}) na execução {ExecutionId}");
            }
        }
        
        /// <summary>
        /// Decrementa a profundidade de recursão
        /// </summary>
        public void DecrementRecursionDepth()
        {
            if (RecursionDepth > 0)
            {
                RecursionDepth--;
            }
        }
        
        /// <summary>
        /// Duração da execução até agora
        /// </summary>
        public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
        
        /// <summary>
        /// Cria um snapshot do contexto atual
        /// </summary>
        public ExecutionContextSnapshot CreateSnapshot()
        {
            return new ExecutionContextSnapshot
            {
                ExecutionId = this.ExecutionId,
                CallCount = this.CallCount,
                ExecutionCount = this.ExecutionCount,
                RecursionDepth = this.RecursionDepth,
                StartTime = this.StartTime,
                Metadata = new Dictionary<string, object>(this.Metadata)
            };
        }
        
        public override string ToString()
        {
            return $"ExecutionContext[Id={ExecutionId}, Calls={CallCount}/{MaxCalls}, Executions={ExecutionCount}/{MaxExecutions}, Depth={RecursionDepth}/{MaxRecursionDepth}, Elapsed={ElapsedTime:mm\\:ss\\.fff}]";
        }
    }
    
    /// <summary>
    /// Snapshot imutável do estado do ExecutionContext
    /// </summary>
    public class ExecutionContextSnapshot
    {
        public string ExecutionId { get; set; }
        public int CallCount { get; set; }
        public int ExecutionCount { get; set; }
        public int RecursionDepth { get; set; }
        public DateTime StartTime { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    }
} 