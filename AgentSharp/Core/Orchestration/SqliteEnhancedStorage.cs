using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Storage;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Implementação SQLite avançada para IEnhancedStorage com suporte completo a TeamChat
    /// </summary>
    public class SqliteEnhancedStorage : IEnhancedStorage
    {
        private readonly string _connectionString;
        private readonly IStorage _baseStorage;
        private bool _isInitialized = false;

        // Implementação das propriedades IStorage
        public ISessionStorage Sessions => _baseStorage?.Sessions;
        public IMemoryStorage Memories => _baseStorage?.Memories;
        public IEmbeddingStorage Embeddings => _baseStorage?.Embeddings;
        public bool IsConnected => _baseStorage?.IsConnected ?? false;

        public SqliteEnhancedStorage(string connectionString, IStorage baseStorage = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _baseStorage = baseStorage != null ? baseStorage : new InMemoryStorage();
        }

        #region Inicialização e Configuração

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await _baseStorage.InitializeAsync();
            await CreateTablesAsync();
            _isInitialized = true;
        }

        private async Task CreateTablesAsync()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Tabela de estados de conversa
                var createConversationStatesTable = @"
                    CREATE TABLE IF NOT EXISTS conversation_states (
                        session_id TEXT PRIMARY KEY,
                        user_id TEXT NOT NULL,
                        current_agent TEXT,
                        status INTEGER NOT NULL DEFAULT 0,
                        created_at TEXT NOT NULL,
                        updated_at TEXT NOT NULL,
                        last_activity TEXT NOT NULL,
                        shared_memory TEXT,
                        metadata TEXT,
                        FOREIGN KEY (session_id) REFERENCES sessions(id)
                    )";

                // Tabela de variáveis globais
                var createGlobalVariablesTable = @"
                    CREATE TABLE IF NOT EXISTS global_variables (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        name TEXT NOT NULL,
                        owned_by TEXT NOT NULL,
                        description TEXT,
                        is_required INTEGER NOT NULL DEFAULT 0,
                        default_value TEXT,
                        current_value TEXT,
                        is_collected INTEGER NOT NULL DEFAULT 0,
                        confidence REAL NOT NULL DEFAULT 1.0,
                        captured_by TEXT,
                        captured_at TEXT,
                        created_at TEXT NOT NULL,
                        updated_at TEXT NOT NULL,
                        UNIQUE(session_id, name),
                        FOREIGN KEY (session_id) REFERENCES conversation_states(session_id)
                    )";

                // Tabela de auditoria de variáveis
                var createVariableAuditTable = @"
                    CREATE TABLE IF NOT EXISTS variable_audit (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        entry_id TEXT UNIQUE NOT NULL,
                        session_id TEXT NOT NULL,
                        variable_name TEXT NOT NULL,
                        old_value TEXT,
                        new_value TEXT,
                        modified_by TEXT NOT NULL,
                        modified_at TEXT NOT NULL,
                        confidence REAL NOT NULL DEFAULT 1.0,
                        source TEXT NOT NULL DEFAULT 'agent',
                        context TEXT,
                        FOREIGN KEY (session_id) REFERENCES conversation_states(session_id)
                    )";

                // Tabela de mensagens de conversa
                var createConversationMessagesTable = @"
                    CREATE TABLE IF NOT EXISTS conversation_messages (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        message_id TEXT UNIQUE NOT NULL,
                        session_id TEXT NOT NULL,
                        agent_name TEXT NOT NULL,
                        content TEXT NOT NULL,
                        message_type TEXT NOT NULL DEFAULT 'agent',
                        timestamp TEXT NOT NULL,
                        metadata TEXT,
                        FOREIGN KEY (session_id) REFERENCES conversation_states(session_id)
                    )";

                // Tabela de agentes disponíveis
                var createConversationAgentsTable = @"
                    CREATE TABLE IF NOT EXISTS conversation_agents (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        agent_name TEXT NOT NULL,
                        description TEXT,
                        triggers TEXT,
                        owned_variables TEXT,
                        metadata TEXT,
                        UNIQUE(session_id, agent_name),
                        FOREIGN KEY (session_id) REFERENCES conversation_states(session_id)
                    )";

                // Tabela de backups
                var createBackupsTable = @"
                    CREATE TABLE IF NOT EXISTS conversation_backups (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        backup_id TEXT UNIQUE NOT NULL,
                        created_at TEXT NOT NULL,
                        version TEXT NOT NULL,
                        backup_data TEXT NOT NULL,
                        metadata TEXT,
                        FOREIGN KEY (session_id) REFERENCES conversation_states(session_id)
                    )";

                // Índices para performance
                var createIndexes = @"
                    CREATE INDEX IF NOT EXISTS idx_conversation_states_user_id ON conversation_states(user_id);
                    CREATE INDEX IF NOT EXISTS idx_conversation_states_status ON conversation_states(status);
                    CREATE INDEX IF NOT EXISTS idx_conversation_states_last_activity ON conversation_states(last_activity);
                    CREATE INDEX IF NOT EXISTS idx_global_variables_session_id ON global_variables(session_id);
                    CREATE INDEX IF NOT EXISTS idx_global_variables_owned_by ON global_variables(owned_by);
                    CREATE INDEX IF NOT EXISTS idx_variable_audit_session_id ON variable_audit(session_id);
                    CREATE INDEX IF NOT EXISTS idx_variable_audit_variable_name ON variable_audit(variable_name);
                    CREATE INDEX IF NOT EXISTS idx_variable_audit_modified_at ON variable_audit(modified_at);
                    CREATE INDEX IF NOT EXISTS idx_conversation_messages_session_id ON conversation_messages(session_id);
                    CREATE INDEX IF NOT EXISTS idx_conversation_messages_timestamp ON conversation_messages(timestamp);
                ";

                var command = connection.CreateCommand();
                command.CommandText = createConversationStatesTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createGlobalVariablesTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createVariableAuditTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createConversationMessagesTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createConversationAgentsTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createBackupsTable;
                await command.ExecuteNonQueryAsync();

                command.CommandText = createIndexes;
                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region Gerenciamento de Estado de Conversa

        public async Task<ConversationState> LoadConversationStateAsync(string sessionId)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Carregar estado básico
                var state = await LoadBasicConversationStateAsync(connection, sessionId);
                if (state == null) return null;

                // Carregar variáveis
                state.Variables = await LoadVariablesAsync(connection, sessionId);

                // Carregar histórico de mensagens
                state.MessageHistory = await LoadMessageHistoryAsync(connection, sessionId);

                // Carregar agentes disponíveis
                state.AvailableAgents = await LoadAvailableAgentsAsync(connection, sessionId);

                // Calcular progresso
                state.Progress = CalculateProgress(state.Variables.Values);

                return state;
            }
        }

        private async Task<ConversationState> LoadBasicConversationStateAsync(SqliteConnection connection, string sessionId)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT session_id, user_id, current_agent, status, created_at, updated_at, 
                       last_activity, shared_memory, metadata
                FROM conversation_states 
                WHERE session_id = @sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync()) return null;

                return new ConversationState
                {
                    SessionId = reader.GetString(reader.GetOrdinal("session_id")),
                    UserId = reader.GetString(reader.GetOrdinal("user_id")),
                    CurrentAgent = reader.IsDBNull(reader.GetOrdinal("current_agent")) ? null : reader.GetString(reader.GetOrdinal("current_agent")),
                    Status = (ConversationStatus)reader.GetInt32(reader.GetOrdinal("status")),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("created_at"))),
                    UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("updated_at"))),
                    LastActivity = DateTime.Parse(reader.GetString(reader.GetOrdinal("last_activity"))),
                    SharedMemory = reader.IsDBNull(reader.GetOrdinal("shared_memory")) ? 
                        new Dictionary<string, object>() : 
                        JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("shared_memory"))),
                    Metadata = reader.IsDBNull(reader.GetOrdinal("metadata")) ? 
                        new Dictionary<string, object>() : 
                        JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("metadata")))
                };
            }
        }

        private async Task<Dictionary<string, GlobalVariable>> LoadVariablesAsync(SqliteConnection connection, string sessionId)
        {
            var variables = new Dictionary<string, GlobalVariable>();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name, owned_by, description, is_required, default_value, current_value,
                       is_collected, confidence, captured_by, captured_at, created_at, updated_at
                FROM global_variables 
                WHERE session_id = @sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var variable = new GlobalVariable
                    {
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        OwnedBy = reader.GetString(reader.GetOrdinal("owned_by")),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        IsRequired = reader.GetBoolean(reader.GetOrdinal("is_required")),
                        DefaultValue = reader.IsDBNull(reader.GetOrdinal("default_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("default_value"))),
                        Value = reader.IsDBNull(reader.GetOrdinal("current_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("current_value"))),
                        IsCollected = reader.GetBoolean(reader.GetOrdinal("is_collected")),
                        Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
                        CapturedBy = reader.IsDBNull(reader.GetOrdinal("captured_by")) ? null : reader.GetString(reader.GetOrdinal("captured_by")),
                        CapturedAt = reader.IsDBNull(reader.GetOrdinal("captured_at")) ? (DateTime?)null : DateTime.Parse(reader.GetString(reader.GetOrdinal("captured_at")))
                    };

                    // Carregar histórico de mudanças
                    variable.ChangeHistory = await LoadVariableHistoryAsync(connection, sessionId, variable.Name);

                    variables[variable.Name] = variable;
                }
            }

            return variables;
        }

        private async Task<List<VariableChange>> LoadVariableHistoryAsync(SqliteConnection connection, string sessionId, string variableName)
        {
            var changes = new List<VariableChange>();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT old_value, new_value, modified_by, modified_at, confidence, source, context
                FROM variable_audit 
                WHERE session_id = @sessionId AND variable_name = @variableName
                ORDER BY modified_at ASC";
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@variableName", variableName);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    changes.Add(new VariableChange
                    {
                        OldValue = reader.IsDBNull(reader.GetOrdinal("old_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("old_value"))),
                        NewValue = reader.IsDBNull(reader.GetOrdinal("new_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("new_value"))),
                        UpdatedBy = reader.GetString(reader.GetOrdinal("modified_by")),
                        UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("modified_at"))),
                        Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
                        Source = reader.GetString(reader.GetOrdinal("source"))
                    });
                }
            }

            return changes;
        }

        private async Task<List<ConversationMessage>> LoadMessageHistoryAsync(SqliteConnection connection, string sessionId)
        {
            var messages = new List<ConversationMessage>();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT message_id, agent_name, content, message_type, timestamp, metadata
                FROM conversation_messages 
                WHERE session_id = @sessionId
                ORDER BY timestamp ASC";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    messages.Add(new ConversationMessage
                    {
                        AgentName = reader.GetString(reader.GetOrdinal("agent_name")),
                        Content = reader.GetString(reader.GetOrdinal("content")),
                        MessageType = reader.GetString(reader.GetOrdinal("message_type")),
                        Timestamp = DateTime.Parse(reader.GetString(reader.GetOrdinal("timestamp"))),
                        Metadata = reader.IsDBNull(reader.GetOrdinal("metadata")) ? 
                            new Dictionary<string, object>() : 
                            JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("metadata")))
                    });
                }
            }

            return messages;
        }

        private async Task<List<ConversationAgent>> LoadAvailableAgentsAsync(SqliteConnection connection, string sessionId)
        {
            var agents = new List<ConversationAgent>();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT agent_name, description, triggers, owned_variables, metadata
                FROM conversation_agents 
                WHERE session_id = @sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    agents.Add(new ConversationAgent
                    {
                        Name = reader.GetString(reader.GetOrdinal("agent_name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        Triggers = reader.IsDBNull(reader.GetOrdinal("triggers")) ? 
                            Array.Empty<string>() : 
                            JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("triggers"))),
                        OwnedVariables = reader.IsDBNull(reader.GetOrdinal("owned_variables")) ? 
                            Array.Empty<string>() : 
                            JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("owned_variables"))),
                        Metadata = reader.IsDBNull(reader.GetOrdinal("metadata")) ? 
                            new Dictionary<string, object>() : 
                            JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("metadata")))
                    });
                }
            }

            return agents;
        }

        public async Task SaveConversationStateAsync(string sessionId, ConversationState state)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Salvar estado básico
                        await SaveBasicConversationStateAsync(connection, transaction, state);

                        // Salvar variáveis
                        await SaveVariablesAsync(connection, transaction, sessionId, state.Variables);

                        // Salvar mensagens
                        await SaveMessagesAsync(connection, transaction, sessionId, state.MessageHistory);

                        // Salvar agentes
                        await SaveAgentsAsync(connection, transaction, sessionId, state.AvailableAgents);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task SaveBasicConversationStateAsync(SqliteConnection connection, SqliteTransaction transaction, ConversationState state)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT OR REPLACE INTO conversation_states 
                (session_id, user_id, current_agent, status, created_at, updated_at, last_activity, shared_memory, metadata)
                VALUES (@sessionId, @userId, @currentAgent, @status, @createdAt, @updatedAt, @lastActivity, @sharedMemory, @metadata)";

            command.Parameters.AddWithValue("@sessionId", state.SessionId);
            command.Parameters.AddWithValue("@userId", state.UserId);
            command.Parameters.AddWithValue("@currentAgent", state.CurrentAgent ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@status", (int)state.Status);
            command.Parameters.AddWithValue("@createdAt", state.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@updatedAt", state.UpdatedAt.ToString("O"));
            command.Parameters.AddWithValue("@lastActivity", state.LastActivity.ToString("O"));
            command.Parameters.AddWithValue("@sharedMemory", JsonSerializer.Serialize(state.SharedMemory));
            command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(state.Metadata));

            await command.ExecuteNonQueryAsync();
        }

        private async Task SaveVariablesAsync(SqliteConnection connection, SqliteTransaction transaction, string sessionId, Dictionary<string, GlobalVariable> variables)
        {
            // Limpar variáveis existentes
            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM global_variables WHERE session_id = @sessionId";
            deleteCommand.Parameters.AddWithValue("@sessionId", sessionId);
            await deleteCommand.ExecuteNonQueryAsync();

            // Inserir variáveis atualizadas
            foreach (var variable in variables.Values)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO global_variables 
                    (session_id, name, owned_by, description, is_required, default_value, current_value,
                     is_collected, confidence, captured_by, captured_at, created_at, updated_at)
                    VALUES (@sessionId, @name, @ownedBy, @description, @isRequired, @defaultValue, @currentValue,
                            @isCollected, @confidence, @capturedBy, @capturedAt, @createdAt, @updatedAt)";

                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@name", variable.Name);
                command.Parameters.AddWithValue("@ownedBy", variable.OwnedBy);
                command.Parameters.AddWithValue("@description", variable.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@isRequired", variable.IsRequired);
                command.Parameters.AddWithValue("@defaultValue", variable.DefaultValue != null ? JsonSerializer.Serialize(variable.DefaultValue) : (object)DBNull.Value);
                command.Parameters.AddWithValue("@currentValue", variable.Value != null ? JsonSerializer.Serialize(variable.Value) : (object)DBNull.Value);
                command.Parameters.AddWithValue("@isCollected", variable.IsCollected);
                command.Parameters.AddWithValue("@confidence", variable.Confidence);
                command.Parameters.AddWithValue("@capturedBy", variable.CapturedBy ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@capturedAt", variable.CapturedAt?.ToString("O") ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("O"));
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task SaveMessagesAsync(SqliteConnection connection, SqliteTransaction transaction, string sessionId, List<ConversationMessage> messages)
        {
            // Limpar mensagens existentes
            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM conversation_messages WHERE session_id = @sessionId";
            deleteCommand.Parameters.AddWithValue("@sessionId", sessionId);
            await deleteCommand.ExecuteNonQueryAsync();

            // Inserir mensagens atualizadas
            foreach (var message in messages)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO conversation_messages 
                    (message_id, session_id, agent_name, content, message_type, timestamp, metadata)
                    VALUES (@messageId, @sessionId, @agentName, @content, @messageType, @timestamp, @metadata)";

                command.Parameters.AddWithValue("@messageId", Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@agentName", message.AgentName);
                command.Parameters.AddWithValue("@content", message.Content);
                command.Parameters.AddWithValue("@messageType", message.MessageType);
                command.Parameters.AddWithValue("@timestamp", message.Timestamp.ToString("O"));
                command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(message.Metadata));

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task SaveAgentsAsync(SqliteConnection connection, SqliteTransaction transaction, string sessionId, List<ConversationAgent> agents)
        {
            // Limpar agentes existentes
            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM conversation_agents WHERE session_id = @sessionId";
            deleteCommand.Parameters.AddWithValue("@sessionId", sessionId);
            await deleteCommand.ExecuteNonQueryAsync();

            // Inserir agentes atualizados
            foreach (var agent in agents)
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO conversation_agents 
                    (session_id, agent_name, description, triggers, owned_variables, metadata)
                    VALUES (@sessionId, @agentName, @description, @triggers, @ownedVariables, @metadata)";

                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@agentName", agent.Name);
                command.Parameters.AddWithValue("@description", agent.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@triggers", JsonSerializer.Serialize(agent.Triggers));
                command.Parameters.AddWithValue("@ownedVariables", JsonSerializer.Serialize(agent.OwnedVariables));
                command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(agent.Metadata));

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateConversationVariablesAsync(string sessionId, GlobalVariableCollection variables)
        {
            var state = await LoadConversationStateAsync(sessionId);
            if (state == null) return;

            state.Variables = variables.Variables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            state.UpdatedAt = DateTime.UtcNow;
            state.Progress = CalculateProgress(state.Variables.Values);

            await SaveConversationStateAsync(sessionId, state);
        }

        public async Task<bool> ConversationStateExistsAsync(string sessionId)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM conversation_states WHERE session_id = @sessionId";
                command.Parameters.AddWithValue("@sessionId", sessionId);

                var count = (long)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

        private ConversationProgress CalculateProgress(IEnumerable<GlobalVariable> variables)
        {
            var variableList = variables.ToList();
            var total = variableList.Count;
            var filled = variableList.Count(v => v.IsCollected);
            var required = variableList.Count(v => v.IsRequired);
            var requiredFilled = variableList.Count(v => v.IsRequired && v.IsCollected);

            return new ConversationProgress
            {
                TotalVariables = total,
                FilledVariables = filled,
                RequiredVariables = required,
                RequiredFilled = requiredFilled,
                IsComplete = requiredFilled == required,
                CompletionPercentage = total > 0 ? (double)filled / total : 1.0
            };
        }

        #endregion

        #region Auditoria de Variáveis

        public async Task<List<VariableAuditEntry>> GetVariableAuditAsync(string sessionId, string variableName = null)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                var whereClause = "WHERE session_id = @sessionId";
                command.Parameters.AddWithValue("@sessionId", sessionId);

                if (!string.IsNullOrWhiteSpace(variableName))
                {
                    whereClause += " AND variable_name = @variableName";
                    command.Parameters.AddWithValue("@variableName", variableName);
                }

                command.CommandText = $@"
                    SELECT entry_id, session_id, variable_name, old_value, new_value, modified_by,
                           modified_at, confidence, source, context
                    FROM variable_audit 
                    {whereClause}
                    ORDER BY modified_at ASC";

                var entries = new List<VariableAuditEntry>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new VariableAuditEntry
                        {
                            EntryId = reader.GetString(reader.GetOrdinal("entry_id")),
                            SessionId = reader.GetString(reader.GetOrdinal("session_id")),
                            VariableName = reader.GetString(reader.GetOrdinal("variable_name")),
                            OldValue = reader.IsDBNull(reader.GetOrdinal("old_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("old_value"))),
                            NewValue = reader.IsDBNull(reader.GetOrdinal("new_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("new_value"))),
                            ModifiedBy = reader.GetString(reader.GetOrdinal("modified_by")),
                            ModifiedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("modified_at"))),
                            Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
                            Source = reader.GetString(reader.GetOrdinal("source")),
                            Context = reader.IsDBNull(reader.GetOrdinal("context")) ? 
                                new Dictionary<string, object>() : 
                                JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("context")))
                        });
                    }
                }

                return entries;
            }
        }

        public async Task SaveVariableAuditAsync(string sessionId, VariableAuditEntry entry)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO variable_audit 
                    (entry_id, session_id, variable_name, old_value, new_value, modified_by,
                     modified_at, confidence, source, context)
                    VALUES (@entryId, @sessionId, @variableName, @oldValue, @newValue, @modifiedBy,
                            @modifiedAt, @confidence, @source, @context)";

                command.Parameters.AddWithValue("@entryId", entry.EntryId);
                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@variableName", entry.VariableName);
                command.Parameters.AddWithValue("@oldValue", entry.OldValue != null ? JsonSerializer.Serialize(entry.OldValue) : (object)DBNull.Value);
                command.Parameters.AddWithValue("@newValue", entry.NewValue != null ? JsonSerializer.Serialize(entry.NewValue) : (object)DBNull.Value);
                command.Parameters.AddWithValue("@modifiedBy", entry.ModifiedBy);
                command.Parameters.AddWithValue("@modifiedAt", entry.ModifiedAt.ToString("O"));
                command.Parameters.AddWithValue("@confidence", entry.Confidence);
                command.Parameters.AddWithValue("@source", entry.Source);
                command.Parameters.AddWithValue("@context", JsonSerializer.Serialize(entry.Context));

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<VariableAuditEntry>> GetVariableHistoryAsync(string sessionId, string variableName, int? limit = null)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                var limitClause = limit.HasValue ? $"LIMIT {limit.Value}" : "";
                
                command.CommandText = $@"
                    SELECT entry_id, session_id, variable_name, old_value, new_value, modified_by,
                           modified_at, confidence, source, context
                    FROM variable_audit 
                    WHERE session_id = @sessionId AND variable_name = @variableName
                    ORDER BY modified_at DESC
                    {limitClause}";

                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@variableName", variableName);

                var entries = new List<VariableAuditEntry>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new VariableAuditEntry
                        {
                            EntryId = reader.GetString(reader.GetOrdinal("entry_id")),
                            SessionId = reader.GetString(reader.GetOrdinal("session_id")),
                            VariableName = reader.GetString(reader.GetOrdinal("variable_name")),
                            OldValue = reader.IsDBNull(reader.GetOrdinal("old_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("old_value"))),
                            NewValue = reader.IsDBNull(reader.GetOrdinal("new_value")) ? null : JsonSerializer.Deserialize<object>(reader.GetString(reader.GetOrdinal("new_value"))),
                            ModifiedBy = reader.GetString(reader.GetOrdinal("modified_by")),
                            ModifiedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("modified_at"))),
                            Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
                            Source = reader.GetString(reader.GetOrdinal("source")),
                            Context = reader.IsDBNull(reader.GetOrdinal("context")) ? 
                                new Dictionary<string, object>() : 
                                JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("context")))
                        });
                    }
                }

                return entries;
            }
        }

        #endregion

        #region Stub implementations for remaining IEnhancedStorage methods

        public async Task<List<string>> GetActiveConversationSessionsAsync(string userId = null)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                var whereClause = "WHERE status = @activeStatus";
                command.Parameters.AddWithValue("@activeStatus", (int)ConversationStatus.Active);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    whereClause += " AND user_id = @userId";
                    command.Parameters.AddWithValue("@userId", userId);
                }

                command.CommandText = $"SELECT session_id FROM conversation_states {whereClause}";

                var sessions = new List<string>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sessions.Add(reader.GetString(reader.GetOrdinal("session_id")));
                    }
                }

                return sessions;
            }
        }

        public async Task<List<ConversationSessionMetadata>> GetConversationSessionsAsync(ConversationSearchCriteria criteria)
        {
            // TODO: Implementar busca completa com todos os critérios
            await Task.CompletedTask;
            return new List<ConversationSessionMetadata>();
        }

        public async Task MarkConversationCompleteAsync(string sessionId, string completionReason = null)
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE conversation_states 
                    SET status = @completedStatus, updated_at = @updatedAt
                    WHERE session_id = @sessionId";

                command.Parameters.AddWithValue("@completedStatus", (int)ConversationStatus.Completed);
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));
                command.Parameters.AddWithValue("@sessionId", sessionId);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CleanupExpiredSessionsAsync(TimeSpan maxAge, bool preserveComplete = true)
        {
            if (!_isInitialized) await InitializeAsync();

            var cutoffDate = DateTime.UtcNow.Subtract(maxAge);

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                var whereClause = "WHERE last_activity < @cutoffDate";
                command.Parameters.AddWithValue("@cutoffDate", cutoffDate.ToString("O"));

                if (preserveComplete)
                {
                    whereClause += " AND status != @completedStatus";
                    command.Parameters.AddWithValue("@completedStatus", (int)ConversationStatus.Completed);
                }

                command.CommandText = $"DELETE FROM conversation_states {whereClause}";
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<ConversationBackup> CreateBackupAsync(string sessionId)
        {
            var state = await LoadConversationStateAsync(sessionId);
            if (state == null) return null;

            var audit = await GetVariableAuditAsync(sessionId);

            var backup = new ConversationBackup
            {
                SessionId = sessionId,
                State = state,
                AuditTrail = audit
            };

            // Salvar backup no banco
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO conversation_backups 
                    (session_id, backup_id, created_at, version, backup_data, metadata)
                    VALUES (@sessionId, @backupId, @createdAt, @version, @backupData, @metadata)";

                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@backupId", Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@createdAt", backup.BackupCreatedAt.ToString("O"));
                command.Parameters.AddWithValue("@version", backup.Version);
                command.Parameters.AddWithValue("@backupData", JsonSerializer.Serialize(backup));
                command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(backup.BackupMetadata));

                await command.ExecuteNonQueryAsync();
            }

            return backup;
        }

        public async Task RestoreFromBackupAsync(string sessionId, ConversationBackup backup)
        {
            await SaveConversationStateAsync(sessionId, backup.State);
            
            foreach (var auditEntry in backup.AuditTrail)
            {
                await SaveVariableAuditAsync(sessionId, auditEntry);
            }
        }

        public async Task<byte[]> ExportConversationDataAsync(string sessionId, string format = "json")
        {
            var state = await LoadConversationStateAsync(sessionId);
            if (state == null) return null;

            var data = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            return Encoding.UTF8.GetBytes(data);
        }

        // Implementações stub para análise e relatórios
        public async Task<VariableUsageStatistics> GetVariableUsageStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            await Task.CompletedTask;
            return new VariableUsageStatistics();
        }

        public async Task<ConversationPerformanceMetrics> GetConversationMetricsAsync(ConversationAnalysisCriteria criteria)
        {
            await Task.CompletedTask;
            return new ConversationPerformanceMetrics();
        }

        public async Task<List<ConversationIssue>> IdentifyConversationIssuesAsync(ConversationIssueCriteria criteria)
        {
            await Task.CompletedTask;
            return new List<ConversationIssue>();
        }

        public async Task OptimizeStorageAsync()
        {
            if (!_isInitialized) await InitializeAsync();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "VACUUM; ANALYZE;";
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<StorageHealthInfo> GetStorageHealthAsync()
        {
            await Task.CompletedTask;
            return new StorageHealthInfo
            {
                IsHealthy = true,
                Version = "1.0.0"
            };
        }

        public async Task ConfigureRetentionPolicyAsync(RetentionPolicy policy)
        {
            // TODO: Implementar configuração de política de retenção
            await Task.CompletedTask;
        }

        #endregion

        #region Delegação para IStorage base

        public async Task ClearAllAsync() => await _baseStorage.ClearAllAsync();
        public async Task ConnectAsync() => await _baseStorage.ConnectAsync();
        public async Task DisconnectAsync() => await _baseStorage.DisconnectAsync();
        public async Task SaveMessageAsync(Message message) => await _baseStorage.SaveMessageAsync(message);
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null) => await _baseStorage.GetSessionMessagesAsync(sessionId, limit);
        public async Task SaveMemoryAsync(UserMemory memory) => await _baseStorage.SaveMemoryAsync(memory);
        public async Task<List<UserMemory>> GetMemoriesAsync(MemoryContext context = null, int? limit = null) => await _baseStorage.GetMemoriesAsync(context, limit);
        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10) => await _baseStorage.SearchMemoriesAsync(query, context, limit);
        public async Task UpdateMemoryAsync(UserMemory memory) => await _baseStorage.UpdateMemoryAsync(memory);
        public async Task DeleteMemoryAsync(string id) => await _baseStorage.DeleteMemoryAsync(id);
        public async Task ClearMemoriesAsync(MemoryContext context = null) => await _baseStorage.ClearMemoriesAsync(context);
        public async Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId) => await _baseStorage.GetOrCreateSessionAsync(sessionId, userId);
        public async Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null) => await _baseStorage.GetUserSessionsAsync(userId, limit);
        public async Task DeleteSessionAsync(string sessionId) => await _baseStorage.DeleteSessionAsync(sessionId);
        public async Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10) => await _baseStorage.GetSessionHistoryAsync(userId, sessionId, limit);
        public async Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message) => await _baseStorage.SaveSessionMessageAsync(userId, sessionId, message);

        #endregion
    }
}