using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.GamblerModCode.Patch;

// ReSharper disable InconsistentNaming
// ReSharper disable SuspiciousTypeConversion.Global

[HarmonyPatch(typeof(CardModel), "TitleLocString", MethodType.Getter)]
public static class CardModelTitleLocStringPatch
{
    public static void Postfix(CardModel __instance, ref LocString __result)
    {
        // 呼ばれたカードが IDynamicCardTitle を実装しているかチェック
        if (__instance is IDynamicCardTitleLocString card)
        {
            // インターフェースのメソッドを呼び出し、戻り値（__result）を上書きする
            __result = card.GetDynamicTitleLocString(__result);
        }
    }
}

[HarmonyPatch(typeof(CardModel), "Title", MethodType.Getter)]
public static class CardModelTitlePatch
{
    public static void Postfix(CardModel __instance, ref string __result)
    {
        // 呼ばれたカードが IDynamicCardTitle を実装しているかチェック
        if (__instance is IDynamicCardTitle card)
        {
            // インターフェースのメソッドを呼び出し、戻り値（__result）を上書きする
            __result = card.GetDynamicTitle(__result);
        }
    }
}

[HarmonyPatch(typeof(CardModel), "Description", MethodType.Getter)]
public static class CardModelDescriptionPatch
{
    public static void Postfix(CardModel __instance, ref LocString __result)
    {
        // 呼ばれたカードが IDynamicCardTitle を実装しているかチェック
        if (__instance is IDynamicCardDescription card)
        {
            // インターフェースのメソッドを呼び出し、戻り値（__result）を上書きする
            __result = card.GetDynamicDescription(__result);
        }
    }
}