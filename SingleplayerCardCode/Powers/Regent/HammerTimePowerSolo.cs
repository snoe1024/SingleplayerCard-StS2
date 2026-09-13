using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Regent;

// Backs HammerTimeSolo (see .claude/loadmap.md "ハンマータイム" 案1). Vanilla HAMMER_TIME_POWER
// makes all allies Forge whenever the owner Forges -- meaningless alone. Singleplayer rework
// instead replays every card played THIS TURN that Forges, whenever Sovereign Blade is played.
//
// Deliberately does NOT track Forge plays via an AfterForge hook into an instance list: this power
// might not even be in play yet when an earlier Forge card is played this turn (Forge card -> THEN
// Hammer Time -> THEN Sovereign Blade), so a hook-based list would silently miss anything played
// before Hammer Time itself. Instead, at the moment Sovereign Blade is played, this scans
// CombatHistory for every CardPlayFinishedEntry that happened this turn (order-independent -- it
// doesn't matter when this power entered play) and keeps the ones with a ForgeVar of at least 1
// (the safe cross-card-read pattern -- DynamicVars.Values.OfType<T>() rather than a named accessor,
// see sts2_dev_knowledge/topics/dynamic-vars.md).
public sealed class HammerTimePowerSolo : SingleplayerCardPower
{
    private bool _isReplaying;

    protected override string? OriginalVanillaPowerId => "HAMMER_TIME_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_isReplaying || cardPlay.Card.Owner != Owner.Player || cardPlay.Card is not SovereignBlade)
        {
            return;
        }

        List<CardModel> toReplay = CombatManager.Instance.History.Entries
            .OfType<CardPlayFinishedEntry>()
            .Where(e => e.CardPlay.Card.Owner == Owner.Player && e.CardPlay.Card != cardPlay.Card && e.HappenedThisTurn(Owner.CombatState))
            .Select(e => e.CardPlay.Card)
            .Where(c => c.DynamicVars.Values.OfType<ForgeVar>().Any(v => v.BaseValue >= 1m))
            .Distinct()
            .ToList();

        if (toReplay.Count == 0)
        {
            return;
        }

        _isReplaying = true;
        try
        {
            bool first = true;
            foreach (CardModel card in toReplay)
            {
                await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !first);
                first = false;
            }
        }
        finally
        {
            _isReplaying = false;
        }
    }
}
