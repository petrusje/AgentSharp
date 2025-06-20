using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using System;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Tools
{

  /// <summary>
  /// Base class for reusable tool packages that can be registered with AI agents.
  /// Tool packages group related functionalities that can be exposed to AI models.
  /// </summary>
  /// <remarks>
  /// The ToolPack architecture allows developers to create modular, reusable sets of tools
  /// that can be easily attached to different agents. Methods decorated with the FunctionCall
  /// attribute are automatically registered as tools when the package is added to an agent.
  /// </remarks>
  public abstract class ToolPack
  {
    /// <summary>
    /// Gets or sets the name of the tool package.
    /// This identifies the package when registered with an agent.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets or sets the description of the tool package.
    /// This should explain the general purpose and capabilities of the package.
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// Gets or sets the version of the tool package.
    /// Should follow semantic versioning (MAJOR.MINOR.PATCH).
    /// </summary>
    public string Version { get; protected set; }

    /// <summary>
    /// Reference to the agent context channel that provides access to the agent properties.
    /// </summary>
    private IAgentCtxChannel _context;

    /// <summary>
    /// Initializes a new instance of the ToolPack class with default properties.
    /// </summary>
    /// <remarks>
    /// Derived classes should override the Name, Description, and Version properties
    /// with appropriate values in their constructors.
    /// </remarks>
    protected ToolPack()
    {
      Name = GetType().Name;
      Description = "Base tool package";
      Version = "1.0.0";
    }

    /// <summary>
    /// Registers the agent context channel to enable communication with the agent.
    /// </summary>
    /// <param name="context">The agent context channel</param>
    /// <exception cref="ArgumentNullException">Thrown when the context is null</exception>
    /// <remarks>
    /// This method is called automatically when the tool pack is added to an agent.
    /// It establishes the connection between the tool pack and the agent.
    /// </remarks>
    public void RegisterContext(IAgentCtxChannel context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a property value from the agent or its context.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve</param>
    /// <returns>The value of the requested property</returns>
    /// <exception cref="InvalidOperationException">Thrown when the context has not been registered</exception>
    /// <exception cref="ArgumentException">Thrown when the property doesn't exist</exception>
    /// <remarks>
    /// This method enables tools to access properties of the agent or its context,
    /// allowing for more flexible and stateful tool implementations.
    /// </remarks>
    protected object GetProperty(string propertyName)
    {
      if (_context == null)
      {
        throw new InvalidOperationException("Agent context has not been registered. Call RegisterContext first.");
      }
      return _context.GetProperty(propertyName);
    }

    /// <summary>
    /// Initializes the tool package asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <remarks>
    /// Override this method in derived classes to perform initialization tasks
    /// such as loading resources, establishing connections, or setting up state.
    /// This is called when the tool pack is added to an agent.
    /// </remarks>
    public virtual Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// Cleans up resources used by the tool package asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <remarks>
    /// Override this method in derived classes to perform cleanup tasks
    /// such as disposing resources, closing connections, or saving state.
    /// This should be called when the agent is being disposed.
    /// </remarks>
    public virtual Task CleanupAsync()
    {
      return Task.CompletedTask;
    }
  }

  /// <summary>
  /// Example implementation of a tool package providing mathematical operations.
  /// </summary>
  /// <remarks>
  /// This class demonstrates how to create a concrete tool pack with functions
  /// that can be called by AI models. It showcases the use of FunctionCall and
  /// FunctionCallParameter attributes to expose methods as tools.
  /// </remarks>
  public class MathToolPack : ToolPack
  {
    /// <summary>
    /// Initializes a new instance of the MathToolPack class.
    /// </summary>
    public MathToolPack()
    {
      Name = "MathToolPack";
      Description = "Math tools package";
      Version = "1.0.0";
    }

    /// <summary>
    /// Adds two numbers together.
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>The sum of a and b</returns>
    [FunctionCall("Add two numbers")]
    [FunctionCallParameter("a", "First number")]
    [FunctionCallParameter("b", "Second number")]
    private double Soma(double a, double b)
    {
      return a + b;
    }

    /// <summary>
    /// Calculates the square root of a number.
    /// </summary>
    /// <param name="numero">Number to calculate the square root</param>
    /// <returns>The square root of the number</returns>
    /// <exception cref="ArgumentException">Thrown when the number is negative</exception>
    [FunctionCall("Calculate the square root of a number")]
    [FunctionCallParameter("numero", "Number to calculate the square root")]
    private double RaizQuadrada(double numero)
    {
      return Math.Sqrt(numero);
    }
  }


}
