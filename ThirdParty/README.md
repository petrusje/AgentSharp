# Third Party Dependencies

This directory contains third-party libraries that have been modified or included directly in the AgentSharp project.

## HNSW.Net

**Original Repository**: https://github.com/microsoft/HNSW.Net  
**License**: MIT  
**Version**: Based on original Microsoft implementation  
**Modifications**: Added incremental updates functionality

### What we modified:
- Added `AddItem()` methods to `SmallWorld` class for incremental additions
- Added `Count` property to track number of items in the graph  
- Added `InsertNode()` method to `Graph` class for single item insertion
- Added proper node ID tracking with `nextNodeId` counter

### Why we included it as source:
The original HNSW.Net NuGet package doesn't support incremental updates, which are essential for our hybrid SQLite+HNSW architecture. We needed to modify the core graph insertion logic to support real-time memory additions.

### License Compliance:
HNSW.Net is licensed under MIT, which allows modification and redistribution. All original copyright notices are preserved.

## Usage in AgentSharp

The modified HNSW.Net enables:
- Incremental vector additions without full graph rebuilds
- Real-time semantic search capabilities  
- Hybrid SQLite+HNSW memory storage architecture
- High-performance vector similarity search with O(log n) complexity