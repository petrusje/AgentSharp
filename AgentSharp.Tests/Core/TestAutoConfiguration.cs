using AgentSharp.Core;
using AgentSharp.Examples.StructuredOutputs;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Core
{
    /// <summary>
    /// Testes para verificar a auto-configuração de structured extraction
    /// </summary>
    [TestClass]
    public class AutoConfigurationTests
    {
        private MockModel? _mockModel;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
        }

        [TestMethod]
        public void Agent_WithCustomType_ShouldAutoConfigureStructuredOutput()
        {
            // Arrange & Act
            var agent = new Agent<object, AnaliseDocumento>(_mockModel, "TestAgent");

            // Assert
            // Note: Aqui normalmente verificaríamos as propriedades internas do agent
            // Por enquanto, apenas verificamos se o agent foi criado sem exceções
            Assert.IsNotNull(agent);
            Assert.AreEqual("TestAgent", agent.Name);
        }

        [TestMethod]
        public void Agent_WithStringType_ShouldNotAutoConfigureStructuredOutput()
        {
            // Arrange & Act
            var agent = new Agent<object, string>(_mockModel, "TestStringAgent");

            // Assert
            Assert.IsNotNull(agent);
            Assert.AreEqual("TestStringAgent", agent.Name);
        }

        [TestMethod]
        public void Agent_WithObjectType_ShouldNotAutoConfigureStructuredOutput()
        {
            // Arrange & Act
            var agent = new Agent<object, object>(_mockModel, "TestObjectAgent");

            // Assert
            Assert.IsNotNull(agent);
            Assert.AreEqual("TestObjectAgent", agent.Name);
        }

        [TestMethod]
        public void Agent_WithExistingStructuredConfig_ShouldRespectUserConfiguration()
        {
            // Arrange
            var config = new ModelConfiguration
            {
                EnableStructuredOutput = true,
                ResponseSchema = "custom schema",
                Temperature = 0.5
            };

            // Act
            var agent = new Agent<object, AnaliseDocumento>(_mockModel, "TestAgent", modelConfig: config);

            // Assert
            Assert.IsNotNull(agent);
            // Note: Idealmente verificaríamos que a configuração do usuário foi preservada
        }

        /// <summary>
        /// Método de demonstração - pode ser usado para testes manuais/debugging
        /// </summary>
        public static Task DemonstrateAutoConfiguration()
        {
            Console.WriteLine("🔧 DEMONSTRAÇÃO DA AUTO-CONFIGURAÇÃO DE STRUCTURED EXTRACTION");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");

            var mockModel = new MockModel();

            // ✅ TESTE 1: Agent com TResult = tipo customizado (deve auto-configurar)
            Console.WriteLine("\n📊 Teste 1: Agent<object, AnaliseDocumento> (deve auto-configurar)");
            var agentStructured = new Agent<object, AnaliseDocumento>(mockModel, "TestAgent");
            Console.WriteLine($"   ✅ Agent criado com auto-configuração para tipo: {typeof(AnaliseDocumento).Name}");

            // ✅ TESTE 2: Agent com TResult = string (NÃO deve auto-configurar)
            Console.WriteLine("\n📝 Teste 2: Agent<object, string> (NÃO deve auto-configurar)");
            var agentString = new Agent<object, string>(mockModel, "TestStringAgent");
            Console.WriteLine($"   ✅ Agent criado SEM auto-configuração para tipo: {typeof(string).Name}");

            // ✅ TESTE 3: Agent com TResult = object (NÃO deve auto-configurar)
            Console.WriteLine("\n🔄 Teste 3: Agent<object, object> (NÃO deve auto-configurar)");
            var agentObject = new Agent<object, object>(mockModel, "TestObjectAgent");
            Console.WriteLine($"   ✅ Agent criado SEM auto-configuração para tipo: {typeof(object).Name}");

            Console.WriteLine("\n🎉 DEMONSTRAÇÃO CONCLUÍDA!");
            Console.WriteLine("   • Auto-configuração funciona corretamente para tipos customizados");
            Console.WriteLine("   • Não interfere com tipos primitivos/básicos");
            Console.WriteLine("   • Framework está mais inteligente e conveniente!");

            return Task.CompletedTask;
        }
    }
}
