using System;
using System.Collections.Generic;
using System.Linq;

namespace Agents.net.Core
{
    /// <summary>
    /// Memória para armazenar passos de raciocínio durante a execução
    /// </summary>
    public class ReasoningMemory
    {
        private readonly List<ReasoningStep> _steps = new List<ReasoningStep>();
        private readonly int _maxSteps;

        public ReasoningMemory(int maxSteps = 10)
        {
            _maxSteps = maxSteps;
        }

        /// <summary>
        /// Adiciona um novo passo de raciocínio
        /// </summary>
        public void AddStep(ReasoningStep step)
        {
            if (_steps.Count >= _maxSteps)
            {
                _steps.RemoveAt(0); // Remove o mais antigo
            }
            _steps.Add(step);
        }

        /// <summary>
        /// Obtém todos os passos de raciocínio
        /// </summary>
        public List<ReasoningStep> GetAllSteps()
        {
            return _steps.ToList();
        }

        /// <summary>
        /// Obtém o último passo de raciocínio
        /// </summary>
        public ReasoningStep GetLastStep()
        {
            return _steps.LastOrDefault();
        }

        /// <summary>
        /// Limpa todos os passos
        /// </summary>
        public void Clear()
        {
            _steps.Clear();
        }

        /// <summary>
        /// Obtém um resumo dos passos de raciocínio
        /// </summary>
        public string GetSummary()
        {
            if (!_steps.Any())
                return "Nenhum passo de raciocínio registrado.";

            var summary = "RESUMO DO RACIOCÍNIO:\n";
            for (int i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                summary += $"{i + 1}. {step.Title}: {step.Reasoning}\n";
            }
            return summary;
        }
    }
} 