using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// CardEnergyCost.GetWithModifiers short-circuits to the raw (never-mutated) _base field for canonical
// instances, and SetCustomBaseCost -- what SingleplayerCardCard.RefreshDroBranchState uses to set a
// DRO-dependent cost -- requires AssertMutable, so it can never run on those same instances. This
// leaves canonical (Card Library) display frozen at whichever cost the constructor happened to pass,
// regardless of the current DRO/Original selection. This Postfix substitutes the correct live value
// for canonical instances only, without ever mutating the immutable CardEnergyCost.
[HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.GetWithModifiers))]
public static class CardEnergyCostCanonicalDisplayPatch
{
    private static readonly FieldInfo CardField = AccessTools.Field(typeof(CardEnergyCost), "_card");

    [HarmonyPostfix]
    public static void Postfix(CardEnergyCost __instance, ref int __result)
    {
        if (CardField.GetValue(__instance) is SingleplayerCardCard { IsCanonical: true } soloCard
            && soloCard.CanonicalDisplayCost is { } cost)
        {
            __result = cost;
        }
    }
}
