using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Utils;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// High-performance vector storage implementation using sqlite-vec extension.
    /// Much simpler and more efficient than sqlite-vss.
    /// Requires sqlite-vec extension to be loaded.
    /// </summary>
    public class VectorSqliteVecStorage
    {
        private readonly string _connectionString;
        private readonly string _embeddingModel;
        private readonly int _dimensions;
        private readonly string _distanceMetric;

        /// <summary>
        /// Resultado de busca por similaridade
        /// </summary>
        public class SimilarityResult
        {
            public string Content { get; set; }
            public float Similarity { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

        /// <summary>
        /// Representa um vetor com metadados
        /// </summary>
        public class EmbeddingVector
        {
            public string Content { get; set; }
            public float[] Vector { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

        /// <summary>
        /// Initializes a new VectorSqliteVecStorage instance.
        /// </summary>
        /// <param name="connectionString">SQLite connection string</param>
        /// <param name="embeddingModel">Model used for embeddings</param>
        /// <param name="dimensions">Vector dimensions</param>
        /// <param name="distanceMetric">Distance metric: cosine, l2, or inner_product (default: cosine)</param>
        public VectorSqliteVecStorage(
            string connectionString,
            string embeddingModel,
            int dimensions,
            string distanceMetric = "cosine")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
            _dimensions = dimensions;
            _distanceMetric = distanceMetric;

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    // Load sqlite-vec extension
                    LoadVecExtension(connection);

                    // Create the main embedding table with vec0 virtual table
                    var createTableSql = $@"
                        CREATE VIRTUAL TABLE IF NOT EXISTS vec_embeddings USING vec0(
                            embedding float[{_dimensions}] distance_metric={_distanceMetric}
                        )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createTableSql;
                        command.ExecuteNonQuery();
                    }

                    // Create metadata table for additional information
                    var createMetadataSql = @"
                        CREATE TABLE IF NOT EXISTS embedding_metadata (
                            rowid INTEGER PRIMARY KEY,
                            content TEXT NOT NULL,
                            model TEXT NOT NULL,
                            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                            metadata TEXT
                        )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createMetadataSql;
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    var status = SqliteVecHelper.CheckInstallationStatus();
                    var errorMessage = status.IsInstalled
                        ? $"Failed to load sqlite-vec extension. {status.Message}"
                        : $"sqlite-vec extension not found. {status.Message}\n\n{SqliteVecHelper.GetInstallationInstructions()}";

                    throw new InvalidOperationException(errorMessage, ex);
                }
            }
        }

        private void LoadVecExtension(SqliteConnection connection)
        {
            // Check if binary is available first
            var status = SqliteVecHelper.CheckInstallationStatus();

            try
            {
                // Try to load vec0 extension by name first
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT load_extension('vec0')";
                    command.ExecuteNonQuery();

                    // Test if extension is loaded
                    command.CommandText = "SELECT vec_version()";
                    var version = command.ExecuteScalar();

                    Console.WriteLine($"✅ sqlite-vec loaded successfully. Version: {version}");
                    return; // Success
                }
            }
            catch (Exception)
            {
                // Try loading from specific path if binary was found
                if (status.IsInstalled && status.IsValid)
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $"SELECT load_extension('{status.BinaryPath}')";
                            command.ExecuteNonQuery();

                            // Test if extension is loaded
                            command.CommandText = "SELECT vec_version()";
                            var version = command.ExecuteScalar();

                            Console.WriteLine($"✅ sqlite-vec loaded from {status.BinaryPath}. Version: {version}");
                            return; // Success
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Failed to load from {status.BinaryPath}: {ex.Message}");
                    }
                }

                // If we get here, loading failed
                var errorMessage = status.IsInstalled
                    ? $"sqlite-vec binary found but failed to load: {status.BinaryPath}\nThe binary may be corrupted or incompatible."
                    : $"sqlite-vec binary not found.\n\n{SqliteVecHelper.GetInstallationInstructions()}";

                throw new InvalidOperationException(errorMessage);
            }
        }

        public void StoreEmbeddings(List<EmbeddingVector> embeddings)
        {
            if (embeddings == null || embeddings.Count == 0)
                return;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert metadata first
                        using (var metadataCommand = connection.CreateCommand())
                        {
                            metadataCommand.CommandText = @"
                                INSERT INTO embedding_metadata (content, model, metadata)
                                VALUES (@content, @model, @metadata)";

                            // Insert vectors into vec table
                            using (var vecCommand = connection.CreateCommand())
                            {
                                vecCommand.CommandText = @"
                                    INSERT INTO vec_embeddings (rowid, embedding)
                                    VALUES (@rowid, @embedding)";

                                foreach (var embedding in embeddings)
                                {
                                    // Insert metadata
                                    metadataCommand.Parameters.Clear();
                                    metadataCommand.Parameters.AddWithValue("@content", embedding.Content);
                                    metadataCommand.Parameters.AddWithValue("@model", _embeddingModel);
                                    var metadataValue = embedding.Metadata != null
                                        ? JsonSerializer.Serialize(embedding.Metadata)
                                        : (object)DBNull.Value;
                                    metadataCommand.Parameters.AddWithValue("@metadata", metadataValue);
                                    metadataCommand.ExecuteNonQuery();

                                    // Get the inserted ID
                                    var rowId = GetLastInsertRowId(connection);

                                    // Insert vector - sqlite-vec accepts vectors as binary blobs
                                    vecCommand.Parameters.Clear();
                                    vecCommand.Parameters.AddWithValue("@rowid", rowId);
                                    vecCommand.Parameters.AddWithValue("@embedding", SerializeVector(embedding.Vector));
                                    vecCommand.ExecuteNonQuery();
                                }
                            }
                        }

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

        private long GetLastInsertRowId(SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid()";
                return (long)command.ExecuteScalar();
            }
        }

        private byte[] SerializeVector(float[] vector)
        {
            // sqlite-vec expects vectors as binary data (little-endian float32 array)
            var bytes = new byte[vector.Length * 4];
            for (int i = 0; i < vector.Length; i++)
            {
                var floatBytes = BitConverter.GetBytes(vector[i]);
                Array.Copy(floatBytes, 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        private float[] DeserializeVector(byte[] bytes)
        {
            if (bytes == null || bytes.Length % 4 != 0)
                throw new ArgumentException("Invalid vector byte array");

            var vector = new float[bytes.Length / 4];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            return vector;
        }

        public List<SimilarityResult> SearchSimilar(float[] queryVector, int topK, float? threshold = null)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT m.content, v.distance, m.metadata
                    FROM vec_embeddings v
                    JOIN embedding_metadata m ON v.rowid = m.rowid
                    WHERE v.embedding MATCH @query
                    ORDER BY distance
                    LIMIT @k";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@query", SerializeVector(queryVector));
                    command.Parameters.AddWithValue("@k", topK);

                    var results = new List<SimilarityResult>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var contentOrdinal = reader.GetOrdinal("content");
                            var distanceOrdinal = reader.GetOrdinal("distance");
                            var metadataOrdinal = reader.GetOrdinal("metadata");

                            var content = reader.GetString(contentOrdinal);
                            var distance = reader.GetFloat(distanceOrdinal);
                            var metadataValue = reader.IsDBNull(metadataOrdinal) ? null : reader.GetString(metadataOrdinal);

                            var metadataDict = string.IsNullOrEmpty(metadataValue)
                                ? null
                                : JsonSerializer.Deserialize<Dictionary<string, object>>(metadataValue);

                            results.Add(new SimilarityResult
                            {
                                Content = content,
                                Similarity = ConvertDistanceToSimilarity(distance),
                                Metadata = metadataDict
                            });
                        }
                    }

                    return results;
                }
            }
        }

        private float ConvertDistanceToSimilarity(float distance)
        {
            switch (_distanceMetric.ToLower())
            {
                case "cosine":
                    // Cosine distance is 1 - cosine_similarity
                    // So similarity = 1 - distance
                    return 1.0f - distance;

                case "l2":
                    // For L2 distance: similarity = 1 / (1 + distance)
                    return 1.0f / (1.0f + distance);

                case "inner_product":
                    // Inner product: higher is more similar (negate distance)
                    return -distance;

                default:
                    return 1.0f / (1.0f + distance);
            }
        }

        public void DeleteEmbeddings(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));

                        // Delete from metadata table
                        using (var metadataCommand = connection.CreateCommand())
                        {
                            metadataCommand.CommandText = $"DELETE FROM embedding_metadata WHERE rowid IN ({placeholders})";

                            // Delete from vec table
                            using (var vecCommand = connection.CreateCommand())
                            {
                                vecCommand.CommandText = $"DELETE FROM vec_embeddings WHERE rowid IN ({placeholders})";

                                for (int i = 0; i < ids.Count; i++)
                                {
                                    var paramName = $"@id{i}";
                                    metadataCommand.Parameters.AddWithValue(paramName, ids[i]);
                                    vecCommand.Parameters.AddWithValue(paramName, ids[i]);
                                }

                                metadataCommand.ExecuteNonQuery();
                                vecCommand.ExecuteNonQuery();
                            }
                        }

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

        public void ClearAllEmbeddings()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Clear metadata
                    command.CommandText = "DELETE FROM embedding_metadata";
                    command.ExecuteNonQuery();

                    // Clear vec table
                    command.CommandText = "DELETE FROM vec_embeddings";
                    command.ExecuteNonQuery();
                }
            }
        }

        public long GetEmbeddingCount()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM embedding_metadata";
                    return (long)command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Gets performance information about the current index
        /// </summary>
        public string GetIndexInfo()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT vec_version()";
                        var version = command.ExecuteScalar()?.ToString();

                        var info = new StringBuilder();
                        info.AppendLine($"SQLite-Vec Version: {version}");
                        info.AppendLine($"Distance Metric: {_distanceMetric}");
                        info.AppendLine($"Dimensions: {_dimensions}");
                        info.AppendLine($"Total Vectors: {GetEmbeddingCount()}");
                        info.AppendLine($"Embedding Model: {_embeddingModel}");

                        // Try to get additional vec info
                        try
                        {
                            command.CommandText = "SELECT vec_info('vec_embeddings')";
                            var vecInfo = command.ExecuteScalar()?.ToString();
                            if (!string.IsNullOrEmpty(vecInfo))
                            {
                                info.AppendLine("Vec Table Info:");
                                info.AppendLine(vecInfo);
                            }
                        }
                        catch
                        {
                            // vec_info might not be available in all versions
                        }

                        return info.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return $"Index info unavailable: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Rebuild the index for optimal performance (if needed)
        /// </summary>
        public void RebuildIndex()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO vec_embeddings(vec_embeddings) VALUES('rebuild')";
                        command.ExecuteNonQuery();
                        Console.WriteLine("Index rebuilt successfully");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Index rebuild failed (might not be needed): {ex.Message}");
                }
            }
        }
    }
}
