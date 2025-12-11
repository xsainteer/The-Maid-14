namespace Content.Shared.Inventory;

public abstract partial class InventorySystem
{
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

    private void HideSlotsOnConcealerEquip(EntityUid uid, InventoryComponent component, string concealerId)
    {
        if (!SlotsToHide.TryGetValue(concealerId, out string? slotsString))
            return;

        var slotIds = slotsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var slotId in slotIds)
        {
            if (!TryGetSlot(uid, slotId, out var slotDef, inventory: component))
                continue;

            slotDef.StripHidden = true;
        }
    }

    private void HideSlotsOnConcealerUnequip(EntityUid uid, InventoryComponent component, string concealerId)
    {
        if (!SlotsToHide.TryGetValue(concealerId, out string? slotsString))
            return;

        var slotIds = slotsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        foreach (var slotId in slotIds)
        {
            if (!TryGetSlot(uid, slotId, out var slotDef, inventory: component))
                continue;

            slotDef.StripHidden = false;
        }
    }
}
