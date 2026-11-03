using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.AlertLevelAccess;

/// <summary>
/// Entites with this component will automatically be EAed during certain alert levels
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AlertLevelAccessComponent : Component
{
    /// <summary>
    /// A list of alert levels that this airlock should be EAed during, if null this component does nothing
    /// </summary>
    [DataField]
    public List<string>? AlertLevels;

    /// <summary>
    /// If specified, then this will instead pull AlertLevels from entities contained in the given container.
    /// </summary>
    [DataField]
    public string? ContainerAlertLevelsProvider;
}