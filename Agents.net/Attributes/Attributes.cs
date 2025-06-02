using System;

namespace Agents.net.Attributes
{
    /// <summary>
    /// Attribute for marking a class as an agent with specific role and capabilities.
    /// </summary>
    /// <remarks>
    /// Used to declaratively define agent properties without requiring manual configuration.
    /// The framework can scan for classes with this attribute to automatically register agents.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AgentAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the role or purpose of the agent in the system.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the agent's capabilities and behavior.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Attribute for marking a class as a task to be performed by an agent.
    /// </summary>
    /// <remarks>
    /// Tasks represent discrete work items that can be assigned to specific agents.
    /// The framework can use this metadata to schedule and coordinate work.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TaskAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the description of what this task does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the agent assigned to perform this task.
        /// </summary>
        public string Agent { get; set; }
    }

    /// <summary>
    /// Attribute for marking a class as a team of agents that work together.
    /// </summary>
    /// <remarks>
    /// Teams provide a way to group related agents that collaborate on complex tasks.
    /// The framework can use this metadata for orchestration and management.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TeamAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the team.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the team's purpose and capabilities.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Attribute for marking a method as callable by AI models through function calling.
    /// </summary>
    /// <remarks>
    /// This attribute enables the AI model to invoke .NET methods directly. Methods decorated
    /// with this attribute will be exposed to the model as tools it can use during response
    /// generation, allowing for integration with external systems and data sources.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class FunctionCallAttribute : Attribute
    {
        /// <summary>
        /// Gets the descriptive text that explains the purpose and behavior of this function.
        /// </summary>
        /// <remarks>
        /// This description is provided to the AI model to help it understand when and how
        /// to use this function during response generation.
        /// </remarks>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the FunctionCallAttribute class.
        /// </summary>
        /// <param name="description">The description of the function to be exposed to the AI model</param>
        public FunctionCallAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Attribute for marking an asynchronous method as callable by AI models through function calling.
    /// </summary>
    /// <remarks>
    /// Similar to FunctionCallAttribute, but specifically designed for async methods that return Task.
    /// This attribute enables the AI model to invoke asynchronous .NET methods directly, with the framework
    /// handling the await operation automatically.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class FunctionCallAsyncAttribute : Attribute
    {
        /// <summary>
        /// Gets the descriptive text that explains the purpose and behavior of this async function.
        /// </summary>
        /// <remarks>
        /// This description is provided to the AI model to help it understand when and how
        /// to use this function during response generation.
        /// </remarks>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the FunctionCallAsyncAttribute class.
        /// </summary>
        /// <param name="description">The description of the async function to be exposed to the AI model</param>
        public FunctionCallAsyncAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Attribute for describing parameters of methods marked with FunctionCallAttribute.
    /// </summary>
    /// <remarks>
    /// Provides additional metadata about function parameters that helps the AI model
    /// understand how to correctly call the function with appropriate arguments.
    /// Multiple instances can be applied to a single method to describe each parameter.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class FunctionCallParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the parameter being described.
        /// </summary>
        /// <remarks>
        /// This must match the actual parameter name in the method signature.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the parameter that explains its purpose and expected values.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the FunctionCallParameterAttribute class.
        /// </summary>
        /// <param name="name">The name of the parameter (must match method parameter name)</param>
        /// <param name="description">The description of the parameter for the AI model</param>
        public FunctionCallParameterAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}