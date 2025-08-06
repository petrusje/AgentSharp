using System;

namespace AgentSharp.Core
{
  /// <summary>
  /// Interface que define um canal para acessar propriedades do agente ou seu contexto
  /// </summary>
  public interface IAgentCtxChannel
  {
    /// <summary>
    /// Obtém o valor de uma propriedade do agente ou do contexto pelo nome
    /// </summary>
    /// <param name="propertyName">Nome da propriedade a ser obtida</param>
    /// <returns>Valor da propriedade como object</returns>
    /// <exception cref="ArgumentException">Lançada quando a propriedade não é encontrada</exception>
    object GetProperty(string propertyName);
  }
}
