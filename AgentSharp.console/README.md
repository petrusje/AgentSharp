# AgentSharp Console - Educational Examples

An interactive learning system for the AgentSharp framework, providing a structured path from basic concepts to advanced use cases.

## 🚀 New Features

### ✅ **Multilingual Support (i18n)**
- **English (US)** and **Portuguese (BR)** support
- Automatic language selection on startup
- All UI strings externalized to resource files
- Easy to add new languages

### ✅ **Configurable Telemetry System**
- **Optional detailed timing** for LLM requests, memory operations, and tool executions
- **Token count and cost estimation** tracking
- **Performance monitoring** for each example
- Can be enabled/disabled at startup

### ✅ **Clean, Didactic Menu Structure**
- **8 carefully selected examples** (down from 27)
- **Progressive learning path**:
  - 🌱 **Level 1: Foundations** (3 examples)
  - 🚀 **Level 2: Intermediate** (3 examples)
  - ⚡ **Level 3: Advanced** (2 examples)
- Removed confusing/internal test files

### ✅ **Improved Code Organization**
- **Clear separation of concerns** with service classes
- **Comprehensive documentation** and inline comments
- **Removed hardcoded strings** and magic values
- **Better error handling** and user feedback

## 📚 Learning Path

### 🌱 **Level 1: Foundations**
1. **Simple Agent** - First interaction with the framework
2. **Agent with Personality** - Basic customization and personas
3. **Agent with Tools** - Integrating external functionality

### 🚀 **Level 2: Intermediate**
4. **Agent with Reasoning** - Complex reasoning chains
5. **Structured Outputs** - Working with typed data
6. **Agent with Memory** - State persistence and recall

### ⚡ **Level 3: Advanced**
7. **Multi-agent Workflows** - Orchestration and coordination
8. **Semantic Search** - Embeddings and vector operations

## 🛠️ Configuration

### Environment Variables
```bash
OPENAI_API_KEY=your_openai_api_key
OPENAI_ENDPOINT=https://api.openai.com/  # Optional
MODEL_NAME=gpt-4o-mini                   # Optional
TEMPERATURE=0.7                          # Optional
MAX_TOKENS=2048                          # Optional
```

### Command Line Arguments
```bash
# Using command line parameters
dotnet run OPENAI_API_KEY=your_key OPENAI_ENDPOINT=your_endpoint
```

## 📊 Telemetry Features

When enabled, the telemetry system provides detailed insights:

- **⏳ LLM Request Timing** - Start/completion times for AI calls
- **💾 Memory Operations** - Time spent on save/load/search operations
- **🔧 Tool Executions** - Performance of integrated tools
- **💰 Cost Tracking** - Estimated costs based on token usage
- **📈 Summary Statistics** - Overall performance metrics

Example output with telemetry enabled:
```
⏳ LLM Request starting...
✅ LLM Response: 2.34s | Tokens: 150 | Cost: ~$0.0045
💾 Memory operation: save (0.12s)
🔧 Tool 'weather_check' executed: 0.45s
⏱️ Execution time: 3.21s
```

## 🌍 Internationalization

The application automatically prompts for language selection:

```
╔══════════════════════════════════════════════════════════════╗
║              🌍 LANGUAGE / IDIOMA SELECTION                  ║
╚══════════════════════════════════════════════════════════════╝

Select your language / Selecione seu idioma:
1. English (US)
2. Português (BR)
Choose / Escolha (1-2): 
```

All subsequent interface elements will be displayed in the selected language.

## 🗂️ File Structure

```
AgentSharp.console/
├── Services/
│   ├── LocalizationService.cs    # Language and culture management
│   └── TelemetryService.cs       # Performance monitoring
├── Resources/
│   ├── Messages.resx             # English strings
│   └── Messages.pt-BR.resx       # Portuguese strings
├── Program.cs                    # Main application with clean structure
└── README.md                     # This documentation
```

## 🔧 Development Notes

### Removed Files
The following files were removed as they were not suitable for educational purposes:
- `DirectStorageTest.cs` - Internal storage testing
- `InMemoryStorageAnalysis.cs` - Development analysis tool
- `TestVectorSqliteVec.cs` - Specific implementation test
- `TestMemorySeparation.cs` - Internal comparison test

### Code Quality Improvements
- **Comprehensive documentation** for all public methods
- **Clear separation of concerns** between UI, logic, and services
- **Proper exception handling** with localized error messages
- **Resource management** for internationalization
- **Service-oriented architecture** for maintainability

## 🎯 Usage Example

1. **Start the application**
   ```bash
   dotnet run
   ```

2. **Select your language** (English/Portuguese)

3. **Configure telemetry** (Enable for detailed timing info)

4. **Choose an example** from the progressive menu (1-8)

5. **Learn progressively** - start with foundations, move to advanced

## 🔄 Migration from Previous Version

If you were using the previous version with 27 menu options:
- **Options 1-3**: Foundation examples (unchanged)
- **Options 4-6**: Intermediate examples (consolidated)
- **Options 7-8**: Advanced examples (best practices selected)
- **Removed options**: Internal tests and duplicated examples

The core educational value has been preserved while improving the learning experience significantly.