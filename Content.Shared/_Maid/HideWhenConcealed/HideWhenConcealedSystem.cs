using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Maid.HideWhenConcealed;

/// <summary>
/// This handles slots getting hidden when a concealing item is equipped.
/// </summary>
public sealed class HideWhenConcealedSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public Dictionary<(EntityUid, string), int> SlotHideCounts { get; set; } = new();

    /// <summary>
    /// Key - concealer slot (e.g., outerClothing)
    /// Value - Comma-separated string of slots to hide (e.g., "underwearb,underweart")
    /// Couldn't use string List because of serialization issues.
    /// </summary>
    private static readonly Dictionary<string, string> SlotsToHide = new()
    {
        { "outerClothing", "underwearb,underweart" },
        { "jumpsuit", "underwearb,underweart" },
        { "shoes", "socks" }
    };

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateComponent, DidUnequipEvent>(OnUnequip);
        SubscribeLocalEvent<MobStateComponent, DidEquipEvent>(OnEquip);
    }

    private void OnUnequip(Entity<MobStateComponent> ent, ref DidUnequipEvent args)
    {
        if (!TryComp<InventoryComponent>(ent.Owner, out var inv))
            return;

        var proto = Prototype(ent.Owner);

        if (!SlotsToHide.TryGetValue(proto!.ID, out string? slotsString))
            return;

        var slotIds = slotsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        foreach (var slotId in slotIds)
        {
            if (!_inventorySystem.TryGetSlot(args.Equipee, slotId, out var slotDef, inventory: inv))
                continue;

            var currentCount = slotDef.HideCount;

            if (currentCount == 0)
                continue;

            var newCount = currentCount - 1;

            if (newCount == 0)
            {
                slotDef.StripHidden = false;
            }
            slotDef.HideCount = newCount;
        }
    }

    private void OnEquip(Entity<MobStateComponent> ent, ref DidEquipEvent args)
    {
        if (!TryComp<InventoryComponent>(ent.Owner, out var inv))
            return;

        var proto = Prototype(ent.Owner);

        if(!SlotsToHide.TryGetValue(proto!.ID, out string? slotsString))
            return;

        var slotIds = slotsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        foreach (var slotId in slotIds)
        {
            if (!_inventorySystem.TryGetSlot(args.Equipee, slotId, out var slotDef, inventory: inv))
                continue;

            var currentCount = slotDef.HideCount;
            var newCount = currentCount + 1;

            if (newCount == 1)
            {
                slotDef.StripHidden = true;
            }

            slotDef.HideCount = newCount;
        }
    }
}
