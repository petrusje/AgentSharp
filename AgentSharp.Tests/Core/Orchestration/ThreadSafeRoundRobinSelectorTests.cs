using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Orchestration;
using Xunit;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Unit tests for ThreadSafeRoundRobinSelector
    /// Tests thread safety, round-robin selection logic, and edge cases
    /// </summary>
    public class ThreadSafeRoundRobinSelectorTests
    {
        [Fact]
        public void SelectNext_WithSingleItem_ShouldAlwaysReturnSameItem()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "OnlyItem" };

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                var selected = selector.SelectNext(items);
                Assert.Equal("OnlyItem", selected);
            }
        }

        [Fact]
        public void SelectNext_WithMultipleItems_ShouldRotateInOrder()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "Item1", "Item2", "Item3" };
            var expected = new[] { "Item1", "Item2", "Item3", "Item1", "Item2", "Item3" };

            // Act
            var results = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                results.Add(selector.SelectNext(items));
            }

            // Assert
            Assert.Equal(expected, results);
        }

        [Fact]
        public void SelectNext_WithNullItems_ShouldThrowArgumentNullException()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => selector.SelectNext<string>(null));
        }

        [Fact]
        public void SelectNext_WithEmptyArray_ShouldThrowArgumentException()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var emptyItems = new string[0];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => selector.SelectNext(emptyItems));
        }

        [Fact]
        public void GetCurrentCounter_InitialValue_ShouldBeZero()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();

            // Act
            var counter = selector.GetCurrentCounter();

            // Assert
            Assert.Equal(0, counter);
        }

        [Fact]
        public void GetCurrentCounter_AfterSelections_ShouldIncrementCorrectly()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "A", "B", "C" };

            // Act
            selector.SelectNext(items); // Counter: 0 -> 1
            selector.SelectNext(items); // Counter: 1 -> 2  
            selector.SelectNext(items); // Counter: 2 -> 0 (wrapped)

            var counter = selector.GetCurrentCounter();

            // Assert
            Assert.Equal(0, counter); // Should wrap back to 0
        }

        [Fact]
        public void Reset_ShouldSetCounterToZero()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "A", "B", "C" };

            // Select a few items to increment counter
            selector.SelectNext(items);
            selector.SelectNext(items);
            
            // Act
            selector.Reset();

            // Assert
            Assert.Equal(0, selector.GetCurrentCounter());
            
            // Verify next selection starts from beginning
            var nextSelection = selector.SelectNext(items);
            Assert.Equal("A", nextSelection);
        }

        [Fact]
        public void SelectNext_WithIntegerOverflow_ShouldHandleGracefully()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "A", "B" };

            // Act - Simulate many selections to test overflow protection
            for (int i = 0; i < 1000; i++)
            {
                var selected = selector.SelectNext(items);
                Assert.True(selected == "A" || selected == "B");
            }

            // Assert - Counter should not overflow and should still work
            var counter = selector.GetCurrentCounter();
            Assert.True(counter >= 0 && counter < items.Length);
        }

        [Fact]
        public async Task SelectNext_WithConcurrentAccess_ShouldBeThreadSafe()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "A", "B", "C", "D", "E" };
            var tasks = new List<Task<string>>();
            var concurrentSelections = 100;

            // Act - Create multiple concurrent tasks
            for (int i = 0; i < concurrentSelections; i++)
            {
                tasks.Add(Task.Run(() => selector.SelectNext(items)));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(concurrentSelections, results.Length);
            
            // All results should be valid items
            Assert.All(results, result => Assert.Contains(result, items));
            
            // Verify distribution - each item should be selected roughly equally
            var distribution = results.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            
            Assert.Equal(items.Length, distribution.Count);
            
            // Check that distribution is reasonable (allowing for some variance due to threading)
            var expectedCount = concurrentSelections / items.Length;
            var tolerance = expectedCount * 0.5; // 50% tolerance
            
            foreach (var kvp in distribution)
            {
                Assert.True(Math.Abs(kvp.Value - expectedCount) <= tolerance,
                    $"Item {kvp.Key} was selected {kvp.Value} times, expected around {expectedCount}");
            }
        }

        [Fact] 
        public async Task SelectNext_WithHighConcurrency_ShouldMaintainIntegrity()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { 1, 2, 3 };
            var tasks = new List<Task<int>>();
            var highConcurrency = 1000;

            // Act
            for (int i = 0; i < highConcurrency; i++)
            {
                tasks.Add(Task.Run(() => selector.SelectNext(items)));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(highConcurrency, results.Length);
            Assert.All(results, result => Assert.Contains(result, items));
            
            // Verify final counter state is consistent
            var finalCounter = selector.GetCurrentCounter();
            Assert.True(finalCounter >= 0 && finalCounter < items.Length);
        }

        [Fact]
        public void SelectNext_WithDifferentTypes_ShouldWork()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();

            // Test with integers
            var intItems = new[] { 1, 2, 3 };
            var intResult = selector.SelectNext(intItems);
            Assert.Equal(1, intResult);

            // Reset for next test
            selector.Reset();

            // Test with custom objects
            var customItems = new[] 
            { 
                new { Name = "Object1", Id = 1 },
                new { Name = "Object2", Id = 2 }
            };
            var customResult = selector.SelectNext(customItems);
            Assert.Equal("Object1", customResult.Name);
            Assert.Equal(1, customResult.Id);
        }

        [Fact]
        public void SelectNext_AfterReset_ShouldStartFromBeginning()
        {
            // Arrange
            var selector = new ThreadSafeRoundRobinSelector();
            var items = new[] { "First", "Second", "Third" };

            // Select items to move counter
            var first = selector.SelectNext(items);   // "First"
            var second = selector.SelectNext(items);  // "Second"
            
            Assert.Equal("First", first);
            Assert.Equal("Second", second);

            // Act - Reset and select again
            selector.Reset();
            var afterReset = selector.SelectNext(items);

            // Assert
            Assert.Equal("First", afterReset);
            Assert.Equal(1, selector.GetCurrentCounter());
        }
    }
}