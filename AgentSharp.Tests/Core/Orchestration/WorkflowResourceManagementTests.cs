using System;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core.Orchestration;
using AgentSharp.Utils;
using Xunit;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Testes para validar resource management do WorkflowBase e AdvancedWorkflow
    /// </summary>
    public class WorkflowResourceManagementTests
    {
        private class TestWorkflow : Workflow<string>
        {
            public bool IsDisposed { get; private set; }
            
            public TestWorkflow(string name, ILogger logger = null) : base(name, logger)
            {
            }

            public override async Task<string> ExecuteAsync(string context, CancellationToken cancellationToken = default)
            {
                ThrowIfDisposed();
                await Task.Delay(100, cancellationToken);
                return context + " processed";
            }

            public void TestRegisterDisposable(IDisposable disposable)
            {
                RegisterDisposable(disposable);
            }

            public void TestRegisterAsyncDisposable(IAsyncDisposable asyncDisposable)
            {
                RegisterAsyncDisposable(asyncDisposable);
            }

            public override void Dispose()
            {
                IsDisposed = true;
                base.Dispose();
            }

            public override async ValueTask DisposeAsync()
            {
                IsDisposed = true;
                await base.DisposeAsync();
            }
        }

        private class TestDisposableResource : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class TestAsyncDisposableResource : IAsyncDisposable
        {
            public bool IsDisposed { get; private set; }

            public async ValueTask DisposeAsync()
            {
                await Task.Delay(1); // Simular operação async
                IsDisposed = true;
            }
        }

        [Fact]
        public void Workflow_ShouldImplementIDisposable()
        {
            // Arrange & Act
            using var workflow = new TestWorkflow("test");

            // Assert
            Assert.IsAssignableFrom<IDisposable>(workflow);
            Assert.IsAssignableFrom<IAsyncDisposable>(workflow);
        }

        [Fact]
        public void Workflow_ShouldDisposeRegisteredResources()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            var resource1 = new TestDisposableResource();
            var resource2 = new TestDisposableResource();

            workflow.TestRegisterDisposable(resource1);
            workflow.TestRegisterDisposable(resource2);

            // Act
            workflow.Dispose();

            // Assert
            Assert.True(workflow.IsDisposed);
            Assert.True(resource1.IsDisposed);
            Assert.True(resource2.IsDisposed);
        }

        [Fact]
        public async Task Workflow_ShouldDisposeAsyncRegisteredResources()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            var resource1 = new TestAsyncDisposableResource();
            var resource2 = new TestAsyncDisposableResource();

            workflow.TestRegisterAsyncDisposable(resource1);
            workflow.TestRegisterAsyncDisposable(resource2);

            // Act
            await workflow.DisposeAsync();

            // Assert
            Assert.True(workflow.IsDisposed, "Workflow should be disposed");
            Assert.True(resource1.IsDisposed, "Resource1 should be disposed");
            Assert.True(resource2.IsDisposed, "Resource2 should be disposed");
        }

        [Fact]
        public void Workflow_ShouldThrowObjectDisposedExceptionAfterDispose()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            workflow.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => workflow.TestRegisterDisposable(new TestDisposableResource()));
            Assert.Throws<ObjectDisposedException>(() => workflow.TestRegisterAsyncDisposable(new TestAsyncDisposableResource()));
        }

        [Fact]
        public async Task Workflow_ShouldThrowObjectDisposedExceptionOnExecuteAfterDispose()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            workflow.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => workflow.ExecuteAsync("test"));
        }

        [Fact]
        public void Workflow_ShouldHandleMultipleDisposeCallsSafely()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            var resource = new TestDisposableResource();
            workflow.TestRegisterDisposable(resource);

            // Act - Dispose multiple times
            workflow.Dispose();
            workflow.Dispose();
            workflow.Dispose();

            // Assert
            Assert.True(workflow.IsDisposed);
            Assert.True(resource.IsDisposed);
        }

        [Fact]
        public async Task Workflow_ShouldHandleMultipleDisposeAsyncCallsSafely()
        {
            // Arrange
            var workflow = new TestWorkflow("test");
            var resource = new TestAsyncDisposableResource();
            workflow.TestRegisterAsyncDisposable(resource);

            // Act - DisposeAsync multiple times
            await workflow.DisposeAsync();
            await workflow.DisposeAsync();
            await workflow.DisposeAsync();
            
            // Aguardar um pouco para garantir que disposal async foi completado
            await Task.Delay(50);

            // Assert
            Assert.True(workflow.IsDisposed);
            Assert.True(resource.IsDisposed);
        }

        [Fact]
        public void Workflow_ShouldHandleNullResourcesGracefully()
        {
            // Arrange
            var workflow = new TestWorkflow("test");

            // Act & Assert - Should not throw
            workflow.TestRegisterDisposable(null);
            workflow.TestRegisterAsyncDisposable(null);
            workflow.Dispose();

            Assert.True(workflow.IsDisposed);
        }

        [Fact]
        public async Task AdvancedWorkflow_ShouldCleanupResourcesProperly()
        {
            // Arrange
            using var workflow = new AdvancedWorkflow<string, string>("test");

            // Act & Assert - Should not throw
            await workflow.ExecuteAsync("test");
        }

        [Fact]
        public void AdvancedWorkflow_ShouldThrowAfterDispose()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("test");
            workflow.Dispose();

            // Act & Assert
            Assert.ThrowsAsync<ObjectDisposedException>(() => workflow.ExecuteAsync("test"));
            Assert.Throws<ObjectDisposedException>(() => workflow.GetMetrics());
        }

        [Fact]
        public void CompositeDisposable_ShouldDisposeResourcesInReverseOrder()
        {
            // Arrange
            var disposedOrder = new System.Collections.Generic.List<int>();
            var resource1 = new TestOrderedDisposableResource(1, disposedOrder);
            var resource2 = new TestOrderedDisposableResource(2, disposedOrder);
            var resource3 = new TestOrderedDisposableResource(3, disposedOrder);

            var composite = new CompositeDisposable();
            composite.Add(resource1);
            composite.Add(resource2);
            composite.Add(resource3);

            // Act
            composite.Dispose();

            // Assert - Should dispose in reverse order (LIFO: 3, 2, 1)
            Assert.Equal(new[] { 3, 2, 1 }, disposedOrder);
            Assert.True(resource1.IsDisposed);
            Assert.True(resource2.IsDisposed);
            Assert.True(resource3.IsDisposed);
            Assert.True(composite.IsDisposed);
        }

        private class TestOrderedDisposableResource : IDisposable
        {
            private readonly int _id;
            private readonly System.Collections.Generic.List<int> _disposedOrder;

            public bool IsDisposed { get; private set; }

            public TestOrderedDisposableResource(int id, System.Collections.Generic.List<int> disposedOrder)
            {
                _id = id;
                _disposedOrder = disposedOrder;
            }

            public void Dispose()
            {
                IsDisposed = true;
                _disposedOrder.Add(_id);
            }
        }
    }
}