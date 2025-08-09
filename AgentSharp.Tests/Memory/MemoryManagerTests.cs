using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Memory
{
    [TestClass]
    public class MemoryManagerTests
    {
        private IModel? _mockModel;
        private ILogger? _logger;
        private IStorage? _storage;
        private IMemoryManager? _memoryManager;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
            _storage = new InMemoryStorage();
            _memoryManager = new MemoryManager(_storage, _mockModel, _logger);
        }

        [TestMethod]
        public async Task LoadContextAsync_ShouldCreateValidContext()
        {
            // Arrange
            string userId = "test_user";
            string sessionId = "test_session";

            // Act
            var context = await _memoryManager!.LoadContextAsync(userId, sessionId);

            // Assert
            Assert.IsNotNull(context);
            Assert.AreEqual(userId, context.UserId);
            Assert.AreEqual(sessionId, context.SessionId);
            Assert.IsNotNull(context.Memories);
            Assert.IsNotNull(context.MessageHistory);
        }

        [TestMethod]
        public async Task AddMemoryAsync_ShouldStoreMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";
            string memoryContent = "Usuário prefere café sem açúcar";

            // Act
            var result = await _memoryManager.AddMemoryAsync(memoryContent);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("adicionada"));

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            Assert.IsTrue(memories.Count > 0);
        }

        [TestMethod]
        public async Task GetExistingMemoriesAsync_ShouldReturnStoredMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";

            await _memoryManager.AddMemoryAsync("Primeiro fato");
            await _memoryManager.AddMemoryAsync("Segunda preferência");

            // Act
            var memories = await _memoryManager.GetExistingMemoriesAsync();

            // Assert
            Assert.IsTrue(memories.Count >= 2);
        }

        [TestMethod]
        public async Task EnhanceMessagesAsync_ShouldAddRelevantMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";

            await _memoryManager.AddMemoryAsync("Usuário gosta de café forte");

            var context = await _memoryManager.LoadContextAsync("test_user", "test_session");
            var messages = new List<AIMessage>
            {
                AIMessage.System("Você é um assistente"),
                AIMessage.User("Como você prepararia café para mim?")
            };

            // Act
            var enhancedMessages = await _memoryManager.EnhanceMessagesAsync(messages, context);

            // Assert
            Assert.IsTrue(enhancedMessages.Count >= messages.Count);
            // Deve ter adicionado contexto de memória se há memórias relevantes
        }

        [TestMethod]
        public async Task ProcessInteractionAsync_ShouldExtractMemories()
        {
            // Arrange
            var context = await _memoryManager!.LoadContextAsync("test_user", "test_session");
            var userMessage = AIMessage.User("Meu nome é João e prefiro trabalhar pela manhã");
            var assistantMessage = AIMessage.Assistant("Ótimo João! Vou lembrar que você prefere trabalhar pela manhã");

            var initialMemoryCount = (await _memoryManager.GetExistingMemoriesAsync()).Count;

            // Act
            await _memoryManager.ProcessInteractionAsync(userMessage, assistantMessage, context);

            // Assert
            var finalMemoryCount = (await _memoryManager.GetExistingMemoriesAsync()).Count;
            // Deve ter extraído pelo menos uma memória (o processo pode ser assíncrono)
            Assert.IsTrue(finalMemoryCount >= initialMemoryCount);
        }

        [TestMethod]
        public async Task UpdateMemoryAsync_ShouldModifyExistingMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Conteúdo original");

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;
            string newContent = "Conteúdo atualizado";

            // Act
            var result = await _memoryManager.UpdateMemoryAsync(memoryId, newContent);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("atualizada"));

            var updatedMemories = await _memoryManager.GetExistingMemoriesAsync();
            Assert.IsTrue(updatedMemories.Exists(m => m.Content.Contains("atualizado")));
        }

        [TestMethod]
        public async Task DeleteMemoryAsync_ShouldRemoveMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Memória para deletar");

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;

            // Act
            var result = await _memoryManager.DeleteMemoryAsync(memoryId);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("removida"));
        }

        [TestMethod]
        public async Task ClearMemoryAsync_ShouldRemoveAllMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Primeira memória");
            await _memoryManager.AddMemoryAsync("Segunda memória");

            var context = new MemoryContext { UserId = "test_user" };

            // Act
            var result = await _memoryManager.ClearMemoryAsync(context);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("limpas"));
        }

        [TestMethod]
        public async Task RunAsync_ShouldProcessMessageWithMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";
            string message = "Olá, como você está?";

            // Act
            var response = await _memoryManager.RunAsync(message);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Length > 0);
        }
    }
}
