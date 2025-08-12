using System;
using System.Collections.Generic;
using HNSW.Net;

namespace TestIncrementalUpdates
{
    class Program
    {
        static void Main(string[] args)
        {
            // Test incremental updates functionality
            var vectors = new[]
            {
                new float[] { 0.1f, 0.2f, 0.3f },
                new float[] { 0.4f, 0.5f, 0.6f },
                new float[] { 0.7f, 0.8f, 0.9f },
                new float[] { 0.2f, 0.3f, 0.4f }
            };

            // Create HNSW with cosine distance
            var smallWorld = new SmallWorld<float[], float>(CosineDistance.NonOptimized);
            
            Console.WriteLine($"Initial count: {smallWorld.Count}");
            
            // Add vectors incrementally
            var random = new Random(42);
            for (int i = 0; i < vectors.Length; i++)
            {
                var id = smallWorld.AddItem(vectors[i], random);
                Console.WriteLine($"Added vector {i} with ID: {id}, Count: {smallWorld.Count}");
            }

            // Test search
            var queryVector = new float[] { 0.15f, 0.25f, 0.35f };
            var results = smallWorld.KNNSearch(queryVector, 2);
            
            Console.WriteLine($"Search results for query vector:");
            foreach (var result in results)
            {
                Console.WriteLine($"  ID: {result.Id}, Distance: {result.Distance:F4}");
            }

            Console.WriteLine("Incremental updates test completed successfully!");
        }
    }
}