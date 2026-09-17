using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SingleplayerCard.SingleplayerCardCode.Cards;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

[HarmonyPatch(typeof(CardFactory), "FilterForPlayerCount")]
public static class CardFactoryFilterForPlayerCountPatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, IEnumerable<CardModel> options, ref IEnumerable<CardModel> __result)
    {
        if (runState.Players.Count <= 1 || !MultiplayerConfigAuthority.EffectiveApplyInMultiplayer)
        {
            return;
        }

        List<CardModel> list = __result.ToList();
        foreach (CardModel card in options)
        {
            if (card is SingleplayerCardCard && !list.Contains(card))
            {
                list.Add(card);
            }
        }

        __result = list;
    }
}
