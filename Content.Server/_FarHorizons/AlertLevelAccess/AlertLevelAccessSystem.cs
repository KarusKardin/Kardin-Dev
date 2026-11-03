using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.AlertLevel;
using Content.Shared._FarHorizons.AlertLevelAccess;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Robust.Shared.Containers;

namespace Content.Server._FarHorizons.AlertLevelAccess;

public sealed partial class AlertLebelAccessSystem : EntitySystem
{
    [Dependency] private SharedAirlockSystem _airlocks = default!;
    [Dependency] private SharedContainerSystem _containerSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertChanged);
    }


    private void OnAlertChanged(AlertLevelChangedEvent args)
    {
        if (args.AlertLevel == args.OldAlertLevel) // Alert level was not actually changed
            return;

        var query = EntityQueryEnumerator<AlertLevelAccessComponent>();
        while (query.MoveNext(out var uid, out var alertaccesscomp))
        {
            if (!TryComp<AirlockComponent>(uid, out var airlockcomp)) // This isnt an airlock.
                continue;

            if (!GetAlertLevelsProvider(uid, alertaccesscomp, out var alertlevels)) // Get the correct list of alerts we should be EAed for
                alertlevels = alertaccesscomp.AlertLevels;
            
            if (alertlevels == null) // we shouldnt be EAed during any alerts
                continue;

            var desiredstate = alertlevels.Any(p => p == args.AlertLevel); // What state do we want to set it to?

            _airlocks.SetEmergencyAccess((uid, airlockcomp), desiredstate);
        }
    }

    // Copied from AccessReaderSystem with heavy modification
    public bool GetAlertLevelsProvider(EntityUid uid, AlertLevelAccessComponent comp, [NotNullWhen(true)] out List<string>? alertlevels)
    {
        alertlevels = null;

        if (comp.ContainerAlertLevelsProvider == null) 
            return false; // we have no provider

        if (!_containerSystem.TryGetContainer(uid, comp.ContainerAlertLevelsProvider, out var container)) 
            return false; // there is no container matching that id

        foreach (var entity in container.ContainedEntities)
        {
            if (TryComp<AlertLevelAccessComponent>(entity, out var containedProvider) && containedProvider.AlertLevels != null)
            {
                alertlevels = containedProvider.AlertLevels;
                return true;
            }
        }

        return false; // no valid providers in container
    }

}