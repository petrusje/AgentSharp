using AgentSharp.Core;
using AgentSharp.Exceptions;
using AgentSharp.Tools;
using AgentSharp.Utils;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace AgentSharp.Models
{
  /// <summary>
  /// Implementation of the IModel interface for OpenAI's language models.
  /// </summary>
  /// <remarks>
  /// This class provides integration with OpenAI's GPT models using the official OpenAI SDK.
  /// It supports both standard completions and streaming responses, along with comprehensive
  /// function calling capabilities through the Tool system.
  ///
  /// The implementation handles:
  /// - API authentication and request management
  /// - Function calling with automatic tool invocation
  /// - Streaming responses for real-time output
  /// - Error handling and retries for resilient operation
  /// - Token usage tracking and cost estimation
  /// </remarks>
  /// <example>
  /// <code>
  /// // Create a simple OpenAI model instance
  /// var model = new OpenAIModel("gpt-4", "your-api-key");
  ///
  /// // Create a model with custom endpoint (e.g., Azure OpenAI)
  /// var azureModel = new OpenAIModel(
  ///     "gpt-35-turbo",
  ///     "your-azure-api-key",
  ///     "https://your-resource.openai.azure.com/"
  /// );
  /// </code>
  /// </example>

  /* Unmerged change from project 'AgentSharp (netstandard2.0)'
  Before:
    public class OpenAIModel : AgentSharp.Models.ModelBase
      {
  After:
    public class OpenAIModel : ModelBase
      {
  */
  public class OpenAIModel : ModelBase
  {
    private readonly OpenAIClient _client;
    private readonly string _modelName;

    /// <summary>
    /// Helper class to maintain streaming state, allowing asynchronous methods
    /// to modify values without needing ref parameters (not supported in .NET Standard 2.0).
    /// </summary>
    private class StreamingState
    {
      /// <summary>
      /// Buffer to accumulate streaming text chunks.
      /// </summary>
      public StringBuilder Buffer { get; } = new StringBuilder();

      /// <summary>
      /// The reason why the response generation completed.
      /// </summary>
      public ChatFinishReason? FinishReason { get; set; }

      /// <summary>
      /// Counter for tracking the number of streaming chunks processed.
      /// </summary>
      public int IterationCount { get; set; }

      /// <summary>
      /// Flag indicating whether content was received in the latest update.
      /// </summary>
      public bool ContentReceived { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the OpenAIModel class.
    /// </summary>
    /// <param name="modelName">The name of the OpenAI model to use (e.g., "gpt-4", "gpt-3.5-turbo")</param>
    /// <param name="apiKey">Your OpenAI API key for authentication</param>
    /// <param name="endpoint">Optional custom API endpoint, useful for Azure OpenAI Service</param>
    /// <exception cref="AuthorizationException">Thrown if the API key is missing or invalid</exception>
    /// <exception cref="ArgumentException">Thrown if the endpoint URL is invalid</exception>
    /// <exception cref="ModelException">Thrown if there's an error initializing the OpenAI client</exception>
    /// <remarks>
    /// When using Azure OpenAI Service, provide the full endpoint URL in the format:
    /// https://your-resource-name.openai.azure.com/
    ///
    /// The default endpoint (when null is provided) is the standard OpenAI API endpoint.
    /// </remarks>
    public OpenAIModel(string modelName, string apiKey, string endpoint = null) : base(modelName)
    {
      if (string.IsNullOrEmpty(apiKey))
        throw new AuthorizationException("OpenAI API key is required");

      if (!InputValidator.IsValidApiKey(apiKey))
        throw new AuthorizationException("Invalid API key format");

      if (endpoint != null && !InputValidator.IsValidEndpoint(endpoint))
        throw new ArgumentException("Invalid endpoint", nameof(endpoint));

      try
      {
        _client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint ?? "https://api.openai.com/") }
        );
        _modelName = modelName;

        Logger.Info($"OpenAIModel initialized with model {modelName}");
      }
      catch (Exception ex)
      {
        Logger.Error("Error initializing OpenAIModel", ex);
        throw new ModelException("Failed to initialize OpenAI client", ex);
      }
    }

    /// <summary>
    /// Generates a response from the OpenAI model based on the provided request.
    /// </summary>
    /// <param name="request">The model request containing messages and tools</param>
    /// <param name="config">Configuration parameters for the model</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The model's response</returns>
    /// <exception cref="ModelException">Thrown when the API call fails</exception>
    /// <exception cref="ToolExecutionException">Thrown when a tool execution fails</exception>
    /// <remarks>
    /// This method handles the complete lifecycle of a model request, including:
    /// - Initial model call
    /// - Processing any tool calls requested by the model
    /// - Making follow-up calls with tool results if needed
    /// - Calculating token usage and estimated costs
    ///
    /// The method uses RetryHelper for automatic retries in case of transient failures.
    /// </remarks>
    public override async Task<ModelResponse> GenerateResponseAsync(
       ModelRequest request,
       ModelConfiguration config,
       CancellationToken cancellationToken = default)
    {
      try
      {
        // Validate configuration
        InputValidator.ValidateModelConfig(config);

        Logger.Debug($"Starting request to model {_modelName} with {request.Messages.Count} messages and {request.Tools.Count} tools");

        // Use RetryHelper for retries in case of network failures
        return await GenerateResponseInternalAsync(request, config, cancellationToken);
      }
      catch (Exception ex)
      {
        Logger.Error($"Error generating response from model {_modelName}", ex);
        throw new ModelException($"Error calling OpenAI API: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Internal implementation of the response generation logic.
    /// </summary>
    /// <param name="request">The model request</param>
    /// <param name="config">Configuration parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The model's response</returns>
    /// <remarks>
    /// This method contains the core logic for API communication, separated from
    /// the public method to facilitate retry handling and error processing.
    /// </remarks>
    private async Task<ModelResponse> GenerateResponseInternalAsync(
        ModelRequest request,
        ModelConfiguration config,
        CancellationToken cancellationToken)
    {
      // Prepare the request
      var nativeMessages = ConvertToNativeMessages(request.Messages);
      var chatRequest = CreateChatRequest(request, config);

      // First call to get initial response
      ChatCompletion response = await _client.GetChatClient(_modelName).CompleteChatAsync(nativeMessages, chatRequest, cancellationToken);

      var modelResponse = new ModelResponse();

      // Process response based on finish reason
      await ProcessFinishReason(response, request, config, nativeMessages, modelResponse, cancellationToken);

      return modelResponse;
    }

    /// <summary>
    /// Processes the response based on its finish reason (stop, tool calls, etc.).
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="request">The original request</param>
    /// <param name="config">Model configuration</param>
    /// <param name="nativeMessages">Message history in native format</param>
    /// <param name="modelResponse">Response object to populate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// This method routes the processing based on how the model finished its response:
    /// - Normal completion (STOP)
    /// - Tool calls request
    /// - Truncated due to token limit (LENGTH)
    /// - Content filtered by safety systems
    /// </remarks>
    private async Task ProcessFinishReason(
        ChatCompletion response,
        ModelRequest request,
        ModelConfiguration config,
        List<ChatMessage> nativeMessages,
        ModelResponse modelResponse,
        CancellationToken cancellationToken)
    {
      switch (response.FinishReason)
      {
        case ChatFinishReason.Stop:
          await HandleFinishReasonStop(response, modelResponse, config);
          break;

        case ChatFinishReason.ToolCalls:
          await HandleFinishReasonToolCalls(response, request, config, nativeMessages, modelResponse, cancellationToken);
          break;

        case ChatFinishReason.Length:
          HandleFinishReasonLength(modelResponse);
          break;

        case ChatFinishReason.ContentFilter:
          HandleFinishReasonContentFilter(modelResponse);
          break;

        default:
          HandleFinishReasonUnknown(response, modelResponse);
          break;
      }
    }

    /// <summary>
    /// Handles the normal completion case (FinishReason.Stop).
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="modelResponse">Response object to populate</param>
    private async Task HandleFinishReasonStop(ChatCompletion response, ModelResponse modelResponse)
    {
      // Normal response without tool calls
      if (response.Content.Count > 0)
      {
        modelResponse.Content = response.Content[0].Text;
      }
      modelResponse.Usage = ConvertUsage(response.Usage);
      Logger.Debug($"Complete response generated with {modelResponse.Usage?.TotalTokens ?? 0} tokens");

      await Task.CompletedTask; // To maintain consistent async signature
    }

    /// <summary>
    /// Handles the normal completion case with structured output processing.
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="modelResponse">Response object to populate</param>
    /// <param name="config">Model configuration with structured output settings</param>
    private async Task HandleFinishReasonStop(ChatCompletion response, ModelResponse modelResponse, ModelConfiguration config)
    {
      // Set content first
      if (response.Content.Count > 0)
      {
        modelResponse.Content = response.Content[0].Text;
      }

      // Process structured output if enabled
      if (config.EnableStructuredOutput && !string.IsNullOrEmpty(modelResponse.Content))
      {
        ProcessStructuredOutput(modelResponse, config);
      }

      modelResponse.Usage = ConvertUsage(response.Usage);
      Logger.Debug($"Complete response generated with {modelResponse.Usage?.TotalTokens ?? 0} tokens");

      await Task.CompletedTask; // To maintain consistent async signature
    }

    /// <summary>
    /// Processes structured output from the model response.
    /// </summary>
    /// <param name="modelResponse">The model response to process</param>
    /// <param name="config">Model configuration with structured output settings</param>
    private void ProcessStructuredOutput(ModelResponse modelResponse, ModelConfiguration config)
    {
      try
      {
        if (string.IsNullOrEmpty(modelResponse.Content))
          return;

        modelResponse.IsStructuredResponse = true;

        // If ResponseType is specified, deserialize to that type
        if (config.ResponseType != null)
        {
          // Debug: Log the raw JSON response to understand the format
          Logger.Debug($"Raw JSON for debugging: {modelResponse.Content}");

          // Configure JSON options to handle enums properly
          var jsonOptions = new JsonSerializerOptions
          {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
          };
          jsonOptions.Converters.Add(new JsonStringEnumConverter());

          var deserializedObject = JsonSerializer.Deserialize(modelResponse.Content, config.ResponseType, jsonOptions);
          modelResponse.StructuredData = deserializedObject;
        }
        else
        {
          // If only schema is provided, keep as JSON but mark as structured
          Logger.Debug("Structured response received but no ResponseType specified - keeping as JSON");
        }
      }
      catch (Exception ex)
      {
        Logger.Warning($"Failed to process structured output: {ex.Message}");
        // Don't fail the entire response - structured data will be null but Content will still be available
      }
    }

    /// <summary>
    /// Handles the tool calls case (FinishReason.ToolCalls).
    /// </summary>
    /// <param name="response">The API response containing tool calls</param>
    /// <param name="request">The original request</param>
    /// <param name="config">Model configuration</param>
    /// <param name="nativeMessages">Message history in native format</param>
    /// <param name="modelResponse">Response object to populate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// This complex workflow handles the entire tool calling process:
    /// 1. Extracts tool calls from the response
    /// 2. Finds and executes the appropriate tools
    /// 3. Adds tool results back to the conversation
    /// 4. Makes a final call to the model with the results
    /// </remarks>
    private async Task HandleFinishReasonToolCalls(
        ChatCompletion response,
        ModelRequest request,
        ModelConfiguration config,
        List<ChatMessage> nativeMessages,
        ModelResponse modelResponse,
        CancellationToken cancellationToken)
    {
      Logger.Info($"Tool calls detected: {response.ToolCalls.Count} calls");
      var tools = request.Tools;

      // Add the assistant message with tool calls
      var assistantMessage = new AIMessage
      {
        Role = Role.Assistant,
        Content = response.Content.Count > 0 ? response.Content[0].Text : string.Empty
      };
      request.Messages.Add(assistantMessage);
      nativeMessages.Add(new AssistantChatMessage(response));

      // Process each tool call
      foreach (var toolCall in response.ToolCalls)
      {
        await ProcessToolCall(toolCall, tools, nativeMessages, modelResponse);
      }

      // New call to the model with tool results
      await MakeFinalModelCallWithToolResults(nativeMessages, config, modelResponse, cancellationToken);
    }

    /// <summary>
    /// Processes a single tool call from the model.
    /// </summary>
    /// <param name="toolCall">The tool call details</param>
    /// <param name="tools">Available tools list</param>
    /// <param name="nativeMessages">Message history in native format</param>
    /// <param name="modelResponse">Response object to update</param>
    /// <exception cref="ToolExecutionException">Thrown if the tool execution fails or tool isn't found</exception>
    /// <remarks>
    /// This method:
    /// 1. Finds the requested tool by name
    /// 2. Sanitizes and validates the arguments
    /// 3. Executes the tool with error handling
    /// 4. Adds the result to both the response and message history
    /// </remarks>
    private async Task ProcessToolCall(
        ChatToolCall toolCall,
        List<Tool> tools,
        List<ChatMessage> nativeMessages,
        ModelResponse modelResponse)
    {
      var toolName = toolCall.FunctionName;
      var argsString = toolCall.FunctionArguments.ToString();

      Logger.Debug($"Processing tool call: {toolName}");

      // Find the corresponding tool
      var tool = tools.FirstOrDefault(t => t.Name == toolName);

      if (tool != null)
      {
        try
        {
          // Validate arguments before calling
          argsString = InputValidator.SanitizeInput(argsString);

          // Invoke the tool
          var result = tool.Invoke(argsString);
          var resultStr = result?.ToString() ?? "No result";

          // Add the result to tools
          modelResponse.Tools.Add(new ToolResult
          {
            Name = toolName,
            Arguments = argsString,
            Result = resultStr,
            ToolCallId = toolCall.Id
          });

          // Add the result as a message for the model
          nativeMessages.Add(ChatMessage.CreateToolMessage(toolCall.Id, resultStr));

          Logger.Info($"Tool {toolName} executed successfully");
        }
        catch (Exception ex)
        {
          Logger.Error($"Error executing tool {toolName}", ex);

          modelResponse.Error = new ModelError
          {
            Code = "tool_execution_error",
            Message = ex.Message,
            Type = "error"
          };

          throw new ToolExecutionException(toolName, $"Error executing tool: {ex.Message}", ex);
        }
      }
      else
      {
        Logger.Warning($"Tool {toolName} not found");

        modelResponse.Error = new ModelError
        {
          Code = "unknown_tool",
          Message = $"Tool {toolName} not found",
          Type = "error"
        };

        throw new ToolExecutionException(toolName, $"Tool {toolName} not found");
      }

      await Task.CompletedTask; // To maintain consistent async signature
    }

    /// <summary>
    /// Makes the final call to the model with tool results and updates the response.
    /// </summary>
    /// <param name="nativeMessages">Message history including tool results</param>
    /// <param name="config">Model configuration</param>
    /// <param name="modelResponse">Response object to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// This method sends a follow-up request to the model with the results of all tool calls,
    /// allowing the model to generate a final response that incorporates the tool outputs.
    /// It also calculates and updates the cumulative token usage.
    /// </remarks>
    private async Task MakeFinalModelCallWithToolResults(
        List<ChatMessage> nativeMessages,
        ModelConfiguration config,
        ModelResponse modelResponse,
        CancellationToken cancellationToken)
    {
      Logger.Debug("Sending second request with tool results");

      var chatRequest = CreateChatRequestOptions(config);
      ChatCompletion finalResponse = await _client.GetChatClient(_modelName).CompleteChatAsync(nativeMessages, chatRequest, cancellationToken);

      // Update final response content
      if (finalResponse.Content.Count > 0)
      {
        modelResponse.Content = finalResponse.Content[0].Text;
      }

      // Process structured output if enabled
      if (config.EnableStructuredOutput && !string.IsNullOrEmpty(modelResponse.Content))
      {
        ProcessStructuredOutput(modelResponse, config);
      }

      // Update usage information
      var totalPromptTokens = modelResponse.Usage?.PromptTokens ?? 0 + finalResponse.Usage.InputTokenCount;
      var totalCompletionTokens = modelResponse.Usage?.CompletionTokens ?? 0 + finalResponse.Usage.OutputTokenCount;

      modelResponse.Usage = new UsageInfo
      {
        PromptTokens = totalPromptTokens,
        CompletionTokens = totalCompletionTokens,
        EstimatedCost = CalculateEstimatedCost(totalPromptTokens, totalCompletionTokens)
      };

      Logger.Info($"Final response generated with {modelResponse.Usage.TotalTokens} tokens");
    }

    /// <summary>
    /// Handles the token limit case (FinishReason.Length).
    /// </summary>
    /// <param name="modelResponse">Response object to update with error info</param>
    /// <remarks>
    /// This occurs when the model's output was truncated due to reaching
    /// the max_tokens parameter or the model's context length limit.
    /// </remarks>
    private void HandleFinishReasonLength(ModelResponse modelResponse)
    {
      Logger.Warning("Model output incomplete due to token limit");

      modelResponse.Error = new ModelError
      {
        Code = "max_tokens_exceeded",
        Message = "Model output incomplete due to Max_Tokens parameter or token limit exceeded",
        Type = "error"
      };
    }

    /// <summary>
    /// Handles the content filter case (FinishReason.ContentFilter).
    /// </summary>
    /// <param name="modelResponse">Response object to update with error info</param>
    /// <remarks>
    /// This occurs when the model's output was filtered due to safety systems
    /// detecting potentially harmful or inappropriate content.
    /// </remarks>
    private void HandleFinishReasonContentFilter(ModelResponse modelResponse)
    {
      Logger.Warning("Content filtered by moderation policies");

      modelResponse.Error = new ModelError
      {
        Code = "content_filter",
        Message = "Content omitted due to content filter flag",
        Type = "error"
      };
    }

    /// <summary>
    /// Handles unknown or unhandled finish reasons.
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="modelResponse">Response object to update with error info</param>
    private void HandleFinishReasonUnknown(ChatCompletion response, ModelResponse modelResponse)
    {
      Logger.Warning($"Unknown finish reason: {response.FinishReason}");

      modelResponse.Error = new ModelError
      {
        Code = "unknown_finish_reason",
        Message = $"Unknown finish reason: {response.FinishReason}",
        Type = "error"
      };
    }

    /// <summary>
    /// Generates a streaming response from the OpenAI model.
    /// </summary>
    /// <param name="request">The model request containing messages and tools</param>
    /// <param name="config">Configuration parameters for the model</param>
    /// <param name="handler">Callback to process each chunk of the streaming response</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The final model response after streaming completes</returns>
    /// <exception cref="ModelException">Thrown when streaming fails</exception>
    /// <remarks>
    /// This method provides real-time output from the model, calling the handler
    /// function with each text chunk as it becomes available.
    ///
    /// Note: When the model requests to use tools, the streaming will pause and switch
    /// to a non-streaming approach for tool execution. This is due to limitations in
    /// the OpenAI API's handling of tool calls in streaming mode.
    /// </remarks>
    public override async Task<ModelResponse> GenerateStreamingResponseAsync(
        ModelRequest request,
        ModelConfiguration config,
        Action<string> handler,
        CancellationToken cancellationToken = default)
    {
      try
      {
        InputValidator.ValidateModelConfig(config);
        Logger.Debug($"Starting streaming request for {_modelName}");

        // Prepare the request
        var nativeMessages = ConvertToNativeMessages(request.Messages);
        var chatRequestOptions = CreateChatRequest(request, config);

        // Initialize response values using the state class
        var streamingState = new StreamingState();
        var modelResponse = new ModelResponse();

        // Start the stream
        Logger.Debug("Calling CompleteChatStreamingAsync...");
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = _client.GetChatClient(_modelName)
            .CompleteChatStreamingAsync(nativeMessages, chatRequestOptions, cancellationToken);

        if (completionUpdates == null)
        {
          Logger.Error("Failed to get completionUpdates stream.");
          throw new ModelException("Failed to start stream.");
        }

        // Process the stream using the state class
        await ProcessStreamingUpdates(completionUpdates, streamingState, handler, cancellationToken);

        // Store initial streaming content
        modelResponse.Content = streamingState.Buffer.ToString();
        Logger.Debug($"Final buffer content: '{modelResponse.Content}'");

        // If a tool call was detected, we need to make a non-streaming request
        if (streamingState.FinishReason == ChatFinishReason.ToolCalls)
        {
          await HandleToolCallsInStreaming(request, config, handler, modelResponse, cancellationToken);
        }
        else
        {
          // No tool calls, just return the response with token estimate
          EstimateTokenUsageForStreaming(request, modelResponse);
        }

        Logger.Info($"Streaming completed. FinishReason: {streamingState.FinishReason?.ToString() ?? "N/A"}. Estimated total tokens: {modelResponse.Usage?.TotalTokens ?? 0}");
        return modelResponse;
      }
      catch (Exception ex)
      {
        Logger.Error($"Error in streaming from model {_modelName}", ex);
        throw new ModelException($"Streaming error: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Processes streaming updates from the API.
    /// </summary>
    /// <param name="completionUpdates">Stream of updates from the API</param>
    /// <param name="state">State object to track streaming progress</param>
    /// <param name="handler">Callback for each text chunk</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// This method handles the enumeration of the async stream from the API,
    /// extracting text chunks and finish reason information.
    /// </remarks>
    private async Task ProcessStreamingUpdates(
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates,
        StreamingState state,
        Action<string> handler,
        CancellationToken cancellationToken)
    {
      var enumerator = completionUpdates.GetAsyncEnumerator(cancellationToken);
      Logger.Debug("Enumerator obtained. Starting loop...");

      try
      {
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
          state.IterationCount++;
          var completionUpdate = enumerator.Current;
          Logger.Debug($"Iteration {state.IterationCount}: Received completionUpdate.");

          // Use shared state instead of ref parameters
          await ProcessStreamChunk(completionUpdate, state, handler);
        }

        Logger.Debug($"Loop completed after {state.IterationCount} iterations.");
      }
      finally
      {
        await enumerator.DisposeAsync().ConfigureAwait(false);
        Logger.Debug("Enumerator disposed.");
      }
    }

    /// <summary>
    /// Processes a single chunk from the streaming response.
    /// </summary>
    /// <param name="completionUpdate">The update from the API</param>
    /// <param name="state">State object to update</param>
    /// <param name="handler">Callback for text content</param>
    /// <remarks>
    /// This method extracts text content and finish reason from each update,
    /// maintains the buffer of accumulated text, and invokes the handler.
    /// </remarks>
    private async Task ProcessStreamChunk(
        StreamingChatCompletionUpdate completionUpdate,
        StreamingState state,
        Action<string> handler)
    {
      state.ContentReceived = false;

      // Process content updates (text)
      if (completionUpdate.ContentUpdate != null && completionUpdate.ContentUpdate.Count > 0)
      {
        foreach (var contentPart in completionUpdate.ContentUpdate)
        {
          var chunk = contentPart.Text;
          if (!string.IsNullOrEmpty(chunk))
          {
            state.ContentReceived = true;
            Logger.Debug($"  Text chunk received: '{chunk}'");
            state.Buffer.Append(chunk);
            try
            {
              handler?.Invoke(chunk);
            }
            catch (Exception handlerEx)
            {
              Logger.Error("  Error inside streaming handler.", handlerEx);
            }
          }
        }
      }

      // Log if no content was received in this specific update
      if (!state.ContentReceived)
      {
        Logger.Debug($"  Iteration {state.IterationCount}: No text content update in this message.");
      }

      // Check finish reason
      if (completionUpdate.FinishReason.HasValue)
      {
        state.FinishReason = completionUpdate.FinishReason.Value;
        Logger.Info($"  Iteration {state.IterationCount}: FinishReason received: {state.FinishReason}");
        LogStreamingFinishReason(state.FinishReason.Value);
      }
      else
      {
        Logger.Debug($"  Iteration {state.IterationCount}: No FinishReason in this update.");
      }

      await Task.CompletedTask; // To maintain consistent async signature
    }

    /// <summary>
    /// Logs information about streaming finish reasons.
    /// </summary>
    /// <param name="finishReason">The finish reason received from the API</param>
    private void LogStreamingFinishReason(ChatFinishReason finishReason)
    {
      switch (finishReason)
      {
        case ChatFinishReason.Stop:
          Logger.Debug("  Reason: Stop - Stream completed normally.");
          break;
        case ChatFinishReason.Length:
          Logger.Warning("  Reason: Length - Incomplete output due to MaxTokens or limit.");
          break;
        case ChatFinishReason.ContentFilter:
          Logger.Warning("  Reason: ContentFilter - Content omitted due to filter.");
          break;
        case ChatFinishReason.ToolCalls:
          Logger.Info("  Reason: ToolCalls - Model requesting tool call.");
          break;
        default:
          Logger.Warning($"  Reason: Unknown or unhandled FinishReason: {finishReason}");
          break;
      }
    }

    /// <summary>
    /// Processes tool calls detected during streaming.
    /// </summary>
    /// <param name="request">The original request</param>
    /// <param name="config">Model configuration</param>
    /// <param name="handler">Callback for text output</param>
    /// <param name="modelResponse">Response object to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// Due to limitations in the API's handling of tool calls in streaming mode,
    /// this method switches to non-streaming mode when tools are involved.
    /// It notifies the user of this switch and provides the final result.
    /// </remarks>
    private async Task HandleToolCallsInStreaming(
        ModelRequest request,
        ModelConfiguration config,
        Action<string> handler,
        ModelResponse modelResponse,
        CancellationToken cancellationToken)
    {
      Logger.Info("Detected ToolCalls. Making non-streaming call to get complete details.");

      // Notify the user just once
      handler?.Invoke("\n\n[Processing tool calls...]\n");

      // Use existing GenerateResponseInternalAsync method, which already contains ToolCalls logic
      var nonStreamingResponse = await GenerateResponseInternalAsync(request, config, cancellationToken);

      // Copy the content, but WITHOUT invoking the handler again
      if (!string.IsNullOrEmpty(nonStreamingResponse.Content))
      {
        if (!string.IsNullOrEmpty(modelResponse.Content))
        {
          modelResponse.Content += "\n\n" + nonStreamingResponse.Content;
        }
        else
        {
          modelResponse.Content = nonStreamingResponse.Content;
        }
      }

      foreach (var toolResult in nonStreamingResponse.Tools)
      {
        modelResponse.Tools.Add(toolResult);
      }

      modelResponse.Usage = nonStreamingResponse.Usage;

      if (nonStreamingResponse.Error != null)
      {
        modelResponse.Error = nonStreamingResponse.Error;
      }

      Logger.Info($"Tool processing completed. Got {modelResponse.Tools.Count} results.");
    }

    /// <summary>
    /// Estimates token usage for streaming responses.
    /// </summary>
    /// <param name="request">The original request</param>
    /// <param name="modelResponse">Response object to update</param>
    /// <remarks>
    /// Since the OpenAI streaming API doesn't provide token counts,
    /// this method uses a simple heuristic to estimate usage based on text length.
    /// </remarks>
    private void EstimateTokenUsageForStreaming(ModelRequest request, ModelResponse modelResponse)
    {
      int estimatedTokens = string.IsNullOrEmpty(modelResponse.Content) ? 0 : modelResponse.Content.Length / 4;
      int promptTokens = EstimatePromptTokens(request);
      modelResponse.Usage = new UsageInfo
      {
        PromptTokens = promptTokens,
        CompletionTokens = estimatedTokens,
        EstimatedCost = CalculateEstimatedCost(promptTokens, estimatedTokens)
      };
    }

    #region Helper Methods

    /// <summary>
    /// Estimates the number of tokens in the prompt.
    /// </summary>
    /// <param name="request">The model request</param>
    /// <returns>Estimated token count</returns>
    /// <remarks>
    /// Uses a simple heuristic based on character count.
    /// In practice, each token is roughly 4 characters in English.
    /// </remarks>
    private int EstimatePromptTokens(ModelRequest request)
    {
      int total = 0;

      foreach (var message in request.Messages)
      {
        // Rough estimate: approximately 4 characters per token
        total += (message.Content?.Length ?? 0) / 4;
      }

      // Add overhead for metadata
      total += 20 * request.Messages.Count;

      return total;
    }

    /// <summary>
    /// Converts messages from library format to OpenAI API format.
    /// </summary>
    /// <param name="messages">Library message objects</param>
    /// <returns>Native API message objects</returns>
    public List<ChatMessage> ConvertToNativeMessages(List<AIMessage> messages)
    {
      return messages.Select<AIMessage, ChatMessage>(message =>
      {
        if (message is ToolResultMessage toolResult)
        {
          // For tool result messages
          return new ToolChatMessage(toolResult.ToolCallId, toolResult.Content);
        }
        else
        {
          // For regular messages
          switch (message.Role)
          {
            case Role.User:
              return new UserChatMessage(message.Content);
            case Role.Assistant:
              return new AssistantChatMessage(message.Content);
            case Role.System:
              return new SystemChatMessage(message.Content);
            default:
              return new UserChatMessage(message.Content);
          }
        }
      }).ToList();
    }

    /// <summary>
    /// Converts tools from library format to OpenAI API format.
    /// </summary>
    /// <param name="tools">Library tool objects</param>
    /// <returns>Native API tool objects</returns>
    public List<ChatTool> ConvertToNativeTools(List<Tool> tools)
    {
      return tools.Select(tool =>
      {
        if (string.IsNullOrWhiteSpace(tool.ParametersSchema))
        {
          // If the tool doesn't have a parameters schema, return a simple function tool
          return ChatTool.CreateFunctionTool(tool.Name, tool.Description);
        }

        try
        {
          var parameters = JsonNode.Parse(tool.ParametersSchema);
          return ChatTool.CreateFunctionTool(tool.Name, tool.Description, new BinaryData(parameters));
        }
        catch (Exception ex)
        {
          Logger.Warning($"Failed to parse parameters schema for tool '{tool.Name}': {ex.Message}");
          // Fallback to creating a tool without parameters schema
          return ChatTool.CreateFunctionTool(tool.Name, tool.Description);
        }
      }).ToList();
    }

    /// <summary>
    /// Creates a request configuration with tools.
    /// </summary>
    /// <param name="request">The model request with tools</param>
    /// <param name="config">Model configuration</param>
    /// <returns>Configured API request object</returns>
    private ChatCompletionOptions CreateChatRequest(ModelRequest request, ModelConfiguration config)
    {
      var chatOptions = CreateChatRequestOptions(config);

      var tools = ConvertToNativeTools(request.Tools);
      foreach (var tool in tools)
      {
        chatOptions.Tools.Add(tool);
      }

      return chatOptions;
    }

    /// <summary>
    /// Creates basic request options from model configuration.
    /// </summary>
    /// <param name="config">Model configuration</param>
    /// <returns>API request options object</returns>
    private ChatCompletionOptions CreateChatRequestOptions(ModelConfiguration config)
    {
      var options = new ChatCompletionOptions
      {
        Temperature = (float?)config.Temperature,
        TopP = (float?)config.TopP,
        FrequencyPenalty = (float?)config.FrequencyPenalty,
        PresencePenalty = (float?)config.PresencePenalty
      };

      // Configure structured output if enabled
      if (config.EnableStructuredOutput)
      {
        ConfigureStructuredOutput(options, config);
      }

      return options;
    }

    /// <summary>
    /// Configures structured output for the chat completion options.
    /// </summary>
    /// <param name="options">The chat completion options to configure</param>
    /// <param name="config">Model configuration containing structured output settings</param>
    private void ConfigureStructuredOutput(ChatCompletionOptions options, ModelConfiguration config)
    {
      try
      {
        string schemaJson;

        // Generate schema from type if ResponseType is provided
        if (config.ResponseType != null)
        {
          schemaJson = Utils.JsonSchemaGenerator.GenerateSchema(config.ResponseType);
          Logger.Debug($"Generated schema from type {config.ResponseType.Name}: {schemaJson}");
        }
        // Use provided schema if available
        else if (!string.IsNullOrEmpty(config.ResponseSchema))
        {
          schemaJson = config.ResponseSchema;
          Logger.Debug($"Using provided schema: {schemaJson}");
        }
        else
        {
          Logger.Warning("Structured output enabled but no ResponseType or ResponseSchema provided");
          return;
        }

        // Parse and set the response format
        var schemaNode = JsonNode.Parse(schemaJson);
        if (schemaNode != null)
        {
          options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
              "structured_response",
              new BinaryData(schemaNode),
              "Structured response format"
          );
          Logger.Info("Structured output configured successfully");
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Failed to configure structured output: {ex.Message}", ex);
        // Don't throw - fall back to regular response format
      }
    }

    /// <summary>
    /// Converts token usage information from API format to library format.
    /// </summary>
    /// <param name="usage">API usage information</param>
    /// <returns>Library usage information object</returns>
    private UsageInfo ConvertUsage(ChatTokenUsage usage)
    {
      if (usage == null) return null;

      return new UsageInfo
      {
        PromptTokens = usage.InputTokenCount,
        CompletionTokens = usage.OutputTokenCount,
        EstimatedCost = CalculateEstimatedCost(
              usage.InputTokenCount,
              usage.OutputTokenCount
          )
      };
    }

    /// <summary>
    /// Calculates estimated cost based on token usage.
    /// </summary>
    /// <param name="promptTokens">Number of tokens in the prompt</param>
    /// <param name="completionTokens">Number of tokens in the completion</param>
    /// <returns>Estimated cost in USD</returns>
    /// <remarks>
    /// Uses approximate pricing for GPT-4 as a baseline.
    /// For accurate pricing, these constants should be updated to match
    /// the specific model being used and current pricing.
    /// </remarks>
    private decimal CalculateEstimatedCost(int promptTokens, int completionTokens)
    {
      const decimal inputCostPer1kTokens = 0.01m;
      const decimal outputCostPer1kTokens = 0.03m;

      return promptTokens / 1000.0m * inputCostPer1kTokens +
             completionTokens / 1000.0m * outputCostPer1kTokens;
    }

    /// <summary>
    /// Creates an embedding service using the same OpenAI client configuration
    /// </summary>
    /// <param name="embeddingModel">The embedding model to use (default: text-embedding-3-small)</param>
    /// <returns>Configured OpenAI embedding service</returns>
    public AgentSharp.Core.Memory.Services.OpenAIEmbeddingService GetEmbeddingService(string embeddingModel = "text-embedding-3-small")
    {
      try
      {
        // Extract endpoint from the current client options
        var endpoint = _client.GetType()
          .GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
          .GetValue(_client) as OpenAIClientOptions;

        var endpointUri = endpoint?.Endpoint?.ToString() ?? "https://api.openai.com";

        // Extract API key - this is more complex since it's wrapped in credentials
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
          throw new InvalidOperationException("Unable to retrieve API key. Ensure OPENAI_API_KEY environment variable is set.");
        }

        return new AgentSharp.Core.Memory.Services.OpenAIEmbeddingService(
          apiKey: apiKey,
          endpoint: endpointUri,
          logger: new ConsoleLogger(), // Use a new logger instance
          model: embeddingModel
        );
      }
      catch (Exception ex)
      {
        Logger.Error("Error creating embedding service", ex);
        throw new ModelException($"Failed to create embedding service: {ex.Message}", ex);
      }
    }

    #endregion
  }
}
