using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AgentSharp.Core.Orchestration;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Testes unitários para componentes específicos do TeamChat
    /// </summary>
    public class TeamChatComponentsTests
    {
        #region GlobalVariable Tests

        [Fact]
        public void GlobalVariable_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var variable = new GlobalVariable("test_var", "agent1", "Test description", true, "default");

            // Assert
            Assert.Equal("test_var", variable.Name);
            Assert.Equal("agent1", variable.OwnedBy);
            Assert.Equal("Test description", variable.Description);
            Assert.True(variable.IsRequired);
            Assert.Equal("default", variable.DefaultValue);
            Assert.Equal("default", variable.Value);
            Assert.False(variable.IsCollected);
            Assert.Equal(1.0, variable.Confidence);
        }

        [Fact]
        public void GlobalVariable_UpdateValue_SetsValuesAndCreatesAuditEntry()
        {
            // Arrange
            var variable = new GlobalVariable("test_var", "agent1", "Test description");
            var originalHistoryCount = variable.ChangeHistory.Count;

            // Act
            variable.UpdateValue("new_value", "agent1", 0.8);

            // Assert
            Assert.Equal("new_value", variable.Value);
            Assert.Equal("agent1", variable.CapturedBy);
            Assert.Equal(0.8, variable.Confidence);
            Assert.True(variable.IsCollected);
            Assert.NotNull(variable.CapturedAt);
            Assert.Equal(originalHistoryCount + 1, variable.ChangeHistory.Count);

            var lastChange = variable.ChangeHistory.Last();
            Assert.Equal("new_value", lastChange.NewValue);
            Assert.Equal("agent1", lastChange.UpdatedBy);
            Assert.Equal(0.8, lastChange.Confidence);
        }

        [Fact]
        public void GlobalVariable_DeepCopy_CreatesIndependentCopy()
        {
            // Arrange
            var original = new GlobalVariable("test_var", "agent1", "Test description");
            original.UpdateValue("original_value", "agent1");

            // Act
            var copy = original.DeepCopy();

            // Assert
            Assert.Equal(original.Name, copy.Name);
            Assert.Equal(original.Value, copy.Value);
            Assert.Equal(original.ChangeHistory.Count, copy.ChangeHistory.Count);

            // Modify original
            original.UpdateValue("modified_value", "agent2");

            // Copy should remain unchanged
            Assert.Equal("original_value", copy.Value);
            Assert.NotEqual(original.ChangeHistory.Count, copy.ChangeHistory.Count);
        }

        #endregion

        #region GlobalVariableCollection Tests

        [Fact]
        public void GlobalVariableCollection_ConfigureVariable_AddsVariable()
        {
            // Arrange
            var collection = new GlobalVariableCollection();

            // Act
            collection.ConfigureVariable("test_var", "agent1", "Test description", true, "default");

            // Assert
            Assert.True(collection.HasVariable("test_var"));
            var variables = collection.GetAllVariables();
            Assert.Single(variables);
            Assert.Equal("test_var", variables[0].Name);
        }

        [Fact]
        public void GlobalVariableCollection_SetVariable_WithValidOwnership_UpdatesVariable()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("test_var", "agent1", "Test description");
            collection.SetCurrentExecutingAgent("agent1");

            // Act
            collection.SetVariable("test_var", "new_value", 0.9);

            // Assert
            var value = collection.GetVariable<string>("test_var");
            Assert.Equal("new_value", value);
        }

        [Fact]
        public void GlobalVariableCollection_SetVariable_WithInvalidOwnership_ThrowsException()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("test_var", "agent1", "Test description");
            collection.SetCurrentExecutingAgent("agent2"); // Different agent

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                collection.SetVariable("test_var", "new_value"));

            Assert.Contains("agent2", exception.Message);
            Assert.Contains("test_var", exception.Message);
            Assert.Contains("agent1", exception.Message);
        }

        [Fact]
        public void GlobalVariableCollection_SetVariable_WithAnyOwnership_AllowsAnyAgent()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("shared_var", "any", "Shared variable");
            collection.SetCurrentExecutingAgent("agent1");

            // Act & Assert (should not throw)
            collection.SetVariable("shared_var", "value1");

            collection.SetCurrentExecutingAgent("agent2");
            collection.SetVariable("shared_var", "value2");

            Assert.Equal("value2", collection.GetVariable<string>("shared_var"));
        }

        [Fact]
        public void GlobalVariableCollection_GetOwnedVariables_ReturnsCorrectVariables()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("var1", "agent1", "Var 1");
            collection.ConfigureVariable("var2", "agent2", "Var 2");
            collection.ConfigureVariable("var3", "any", "Shared Var");

            // Act
            var agent1Variables = collection.GetOwnedVariables("agent1");

            // Assert
            Assert.Equal(2, agent1Variables.Count); // var1 + shared var
            Assert.Contains(agent1Variables, v => v.Name == "var1");
            Assert.Contains(agent1Variables, v => v.Name == "var3");
        }

        [Fact]
        public void GlobalVariableCollection_GetMissingVariables_ReturnsUncollectedVariables()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("var1", "agent1", "Var 1");
            collection.ConfigureVariable("var2", "agent1", "Var 2");
            collection.SetCurrentExecutingAgent("agent1");
            collection.SetVariable("var1", "collected");

            // Act
            var missingVariables = collection.GetMissingVariables("agent1");

            // Assert
            Assert.Single(missingVariables);
            Assert.Equal("var2", missingVariables[0].Name);
        }

        [Fact]
        public void GlobalVariableCollection_GetProgress_CalculatesCorrectly()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("req1", "agent1", "Required 1", required: true);
            collection.ConfigureVariable("req2", "agent1", "Required 2", required: true);
            collection.ConfigureVariable("opt1", "agent1", "Optional 1", required: false);
            
            collection.SetCurrentExecutingAgent("agent1");
            collection.SetVariable("req1", "value1"); // 1 required filled
            collection.SetVariable("opt1", "value2"); // 1 optional filled

            // Act
            var progress = collection.GetProgress();

            // Assert
            Assert.Equal(3, progress.TotalVariables);
            Assert.Equal(2, progress.FilledVariables);
            Assert.Equal(2, progress.RequiredVariables);
            Assert.Equal(1, progress.RequiredFilled);
            Assert.False(progress.IsComplete); // Not all required filled
            Assert.Equal(2.0/3.0, progress.CompletionPercentage, 2);
        }

        [Fact]
        public void GlobalVariableCollection_Clear_RemovesAllVariables()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.ConfigureVariable("var1", "agent1", "Var 1");
            collection.ConfigureVariable("var2", "agent2", "Var 2");

            // Act
            collection.Clear();

            // Assert
            Assert.Empty(collection.GetAllVariables());
            Assert.False(collection.HasVariable("var1"));
            Assert.False(collection.HasVariable("var2"));
        }

        #endregion

        #region GlobalVariableBuilder Tests

        [Fact]
        public void GlobalVariableBuilder_FluentAPI_BuildsCorrectly()
        {
            // Arrange & Act
            var variables = new GlobalVariableBuilder()
                .Add("name", "reception", "Customer name", required: true)
                .Add("email", "reception", "Customer email", required: true)
                .AddShared("notes", "General notes", required: false)
                .Build();

            // Assert
            Assert.Equal(3, variables.Count);

            var nameVar = variables.First(v => v.Name == "name");
            Assert.Equal("reception", nameVar.OwnedBy);
            Assert.True(nameVar.IsRequired);

            var notesVar = variables.First(v => v.Name == "notes");
            Assert.Equal("any", notesVar.OwnedBy);
            Assert.False(notesVar.IsRequired);
        }

        #endregion

        #region SystemMessageBuilder Tests

        [Fact]
        public void SystemMessageBuilder_BuildForAgent_GeneratesCorrectMessage()
        {
            // Arrange
            var builder = new SystemMessageBuilder();
            var variables = new GlobalVariableCollection();
            variables.ConfigureVariable("name", "agent1", "Customer name", required: true);
            variables.ConfigureVariable("email", "agent2", "Customer email", required: true);
            
            variables.SetCurrentExecutingAgent("agent2");
            variables.SetVariable("email", "test@example.com");

            // Act
            var message = builder.BuildForAgent("agent1", variables);

            // Assert
            Assert.Contains("SUA MISSÃO", message);
            Assert.Contains("agent1", message);
            Assert.Contains("Customer name", message);
            Assert.Contains("FALTANDO", message);
            Assert.Contains("CONTEXTO ATUAL", message);
            Assert.Contains("test@example.com", message);
            Assert.Contains("SaveVariable", message);
        }

        [Fact]
        public void SystemMessageBuilder_WithCustomTemplate_UsesCustomConfiguration()
        {
            // Arrange
            var builder = new SystemMessageBuilder();
            var template = new SystemMessageTemplateBuilder()
                .IncludeMission(false)
                .IncludeTools(false)
                .WithSectionTitle("context", "CUSTOM CONTEXT")
                .Build();

            builder.WithDefaultTemplate(template);

            var variables = new GlobalVariableCollection();
            variables.ConfigureVariable("var1", "agent1", "Test var");
            variables.SetCurrentExecutingAgent("agent1");
            variables.SetVariable("var1", "test_value");

            // Act
            var message = builder.BuildForAgent("agent1", variables);

            // Assert
            Assert.DoesNotContain("SUA MISSÃO", message);
            Assert.DoesNotContain("SaveVariable", message);
            Assert.Contains("CUSTOM CONTEXT", message);
        }

        [Fact]
        public void SystemMessageBuilder_WithNoVariables_GeneratesBasicMessage()
        {
            // Arrange
            var builder = new SystemMessageBuilder();
            var variables = new GlobalVariableCollection();

            // Act
            var message = builder.BuildForAgent("agent1", variables);

            // Assert
            Assert.Contains("SUA MISSÃO", message);
            Assert.Contains("agent1", message);
            Assert.Contains("SaveVariable", message);
        }

        #endregion

        #region ConversationProgress Tests

        [Fact]
        public void ConversationProgress_ToString_FormatsCorrectly()
        {
            // Arrange
            var progress = new ConversationProgress
            {
                TotalVariables = 5,
                FilledVariables = 3,
                RequiredVariables = 3,
                RequiredFilled = 2,
                CompletionPercentage = 0.6,
                IsComplete = false
            };

            // Act
            var result = progress.ToString();

            // Assert
            Assert.Contains("3/5", result);
            // Flexible percentage check - handles both "60%" and "60 %" formats across different cultures
            Assert.True(result.Contains("60%") || result.Contains("60 %"), 
                $"Expected to contain '60%' or '60 %', but got: {result}");
            Assert.Contains("2/3", result);
            // Flexible percentage check for required completion
            Assert.True(result.Contains("67%") || result.Contains("67 %"), 
                $"Expected to contain '67%' or '67 %', but got: {result}");
        }

        [Fact]
        public void ConversationProgress_RequiredCompletionPercentage_CalculatesCorrectly()
        {
            // Arrange
            var progress = new ConversationProgress
            {
                RequiredVariables = 4,
                RequiredFilled = 3
            };

            // Act
            var percentage = progress.RequiredCompletionPercentage;

            // Assert
            Assert.Equal(0.75, percentage);
        }

        [Fact]
        public void ConversationProgress_RequiredCompletionPercentage_WithNoRequired_Returns100Percent()
        {
            // Arrange
            var progress = new ConversationProgress
            {
                RequiredVariables = 0,
                RequiredFilled = 0
            };

            // Act
            var percentage = progress.RequiredCompletionPercentage;

            // Assert
            Assert.Equal(1.0, percentage);
        }

        #endregion

        #region ConversationMessage Tests

        [Fact]
        public void ConversationMessage_Constructor_SetsDefaultValues()
        {
            // Arrange & Act
            var message = new ConversationMessage();

            // Assert
            Assert.Equal("agent", message.MessageType);
            Assert.NotEqual(default(DateTime), message.Timestamp);
            Assert.NotNull(message.Metadata);
        }

        [Fact]
        public void ConversationMessage_ParameterizedConstructor_SetsValues()
        {
            // Arrange & Act
            var message = new ConversationMessage("TestAgent", "Test content", "user");

            // Assert
            Assert.Equal("TestAgent", message.AgentName);
            Assert.Equal("Test content", message.Content);
            Assert.Equal("user", message.MessageType);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void GlobalVariableCollection_SetVariable_NonExistentVariable_ThrowsException()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            collection.SetCurrentExecutingAgent("agent1");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                collection.SetVariable("nonexistent", "value"));

            Assert.Contains("nonexistent", exception.Message);
            Assert.Contains("não definida", exception.Message);
        }

        [Fact]
        public void GlobalVariableCollection_GetVariable_NonExistentVariable_ThrowsException()
        {
            // Arrange
            var collection = new GlobalVariableCollection();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                collection.GetVariable<string>("nonexistent"));

            Assert.Contains("nonexistent", exception.Message);
        }

        [Fact]
        public void SystemMessageBuilder_BuildForAgent_NullArguments_ThrowsException()
        {
            // Arrange
            var builder = new SystemMessageBuilder();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                builder.BuildForAgent("", new GlobalVariableCollection()));

            Assert.Throws<ArgumentNullException>(() =>
                builder.BuildForAgent("agent1", null));
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void GlobalVariableCollection_LargeNumberOfVariables_PerformsEfficiently()
        {
            // Arrange
            var collection = new GlobalVariableCollection();
            const int variableCount = 1000;

            // Act - Configure many variables
            var start = DateTime.UtcNow;
            for (int i = 0; i < variableCount; i++)
            {
                collection.ConfigureVariable($"var_{i}", "agent1", $"Variable {i}");
            }
            var configureTime = DateTime.UtcNow - start;

            // Act - Access variables
            start = DateTime.UtcNow;
            var ownedVariables = collection.GetOwnedVariables("agent1");
            var accessTime = DateTime.UtcNow - start;

            // Assert
            Assert.Equal(variableCount, ownedVariables.Count);
            Assert.True(configureTime.TotalMilliseconds < 1000, $"Configure took {configureTime.TotalMilliseconds}ms");
            Assert.True(accessTime.TotalMilliseconds < 100, $"Access took {accessTime.TotalMilliseconds}ms");
        }

        #endregion
    }
}