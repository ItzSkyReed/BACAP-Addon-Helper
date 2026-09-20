using System.Reflection;
using Core.DataComponents.Components;
using Core.DataComponents.Interfaces;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.DataComponents;

/// <summary>
/// A centralized registry for mapping SNBT component identifiers to their respective strongly-typed C# component implementations.
/// </summary>
public static class ComponentRegistry
{
    // A dictionary mapping a component's SNBT identifier (e.g. "minecraft:damage") to its compiled parsing function.
    private static readonly Dictionary<string, Func<ISnbtNode, IDataComponent>> Parsers = new();

    /// <summary>
    /// Automatically scans the specified assembly and registers all non-abstract types implementing <see cref="IParsableComponent{TSelf, TNode}"/>.
    /// This includes specialized interfaces like ICompoundComponent, IStringComponent, and IListComponent.
    /// </summary>
    /// <param name="assembly">The assembly to scan. If null, scans the assembly containing the <see cref="ComponentRegistry"/>.</param>
    [PublicAPI]
    public static void RegisterAll(Assembly? assembly = null)
    {
        assembly ??= typeof(ComponentRegistry).Assembly;

        var openInterface = typeof(IParsableComponent<,>);

        // Find the base generic Register<TComponent, TNode>() method
        var registerMethod = typeof(ComponentRegistry)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Register) && m.GetGenericArguments().Length == 2);

        // Find all non-abstract types that implement IParsableComponent<TSelf, TNode>
        var componentTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && (t.IsClass || t.IsValueType))
            .Select(t => new
            {
                Type = t,
                // Look for the specific generic interface definition in the type's implemented interfaces
                Interface = t.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == openInterface)
            })
            .Where(x => x.Interface != null);

        foreach (var component in componentTypes)
        {
            // Extract the TNode type parameter from the resolved interface (the second generic argument)
            var nodeType = component.Interface!.GetGenericArguments()[1];

            // Construct and invoke the Register<TComponent, TNode>() method dynamically
            var genericMethod = registerMethod.MakeGenericMethod(component.Type, nodeType);
            genericMethod.Invoke(null, null);
        }
    }

    /// <summary>
    /// Registers a specific component type with its required SNBT node type constraint.
    /// </summary>
    /// <typeparam name="TComponent">The strictly typed component class/record.</typeparam>
    /// <typeparam name="TNode">The specific SNBT node type required by this component (e.g., SnbtCompound).</typeparam>
    [PublicAPI]
    public static void Register<TComponent, TNode>()
        where TComponent : IParsableComponent<TComponent, TNode>
        where TNode : ISnbtNode
    {
        Parsers[TComponent.ComponentId] = node =>
        {
            // Strict type validation to ensure the parsed SNBT node matches the component's expected structure
            if (node is not TNode typedNode)
            {
                throw new ArgumentException(
                    $"Component '{TComponent.ComponentId}' requires node of type '{typeof(TNode).Name}', but got '{node.GetType().Name}'."
                );
            }

            return TComponent.Parse(typedNode);
        };
    }

    /// <summary>
    /// Registers a generic component type that handles a flexible or polymorphic SNBT node.
    /// </summary>
    /// <typeparam name="TComponent">The component type that accepts a generic <see cref="ISnbtNode"/>.</typeparam>
    [PublicAPI]
    public static void Register<TComponent>()
        where TComponent : IParsableComponent<TComponent, ISnbtNode>
    {
        Register<TComponent, ISnbtNode>();
    }

    /// <summary>
    /// Parses a raw SNBT node into a strictly typed data component based on its identifier.
    /// Automatically appends the default 'minecraft:' namespace if none is provided.
    /// Falls back to a <see cref="RawComponent"/> if the identifier is not registered.
    /// </summary>
    /// <param name="componentId">The identifier of the component (e.g. <c>custom_data</c> or <c>modded:data</c>).</param>
    /// <param name="node">The SNBT payload to parse.</param>
    /// <returns>A populated <see cref="IDataComponent"/> instance.</returns>
    [PublicAPI]
    public static IDataComponent Parse(string componentId, ISnbtNode node)
    {
        // Normalize the identifier to match Minecraft's default ResourceLocation behavior
        var normalizedId = componentId.Contains(':') ? componentId : $"minecraft:{componentId}";

        if (Parsers.TryGetValue(normalizedId, out var parser))
        {
            return parser(node);
        }

        // Lossless fallback for unregistered, unknown, or modded components
        return new RawComponent(normalizedId, node);
    }
}