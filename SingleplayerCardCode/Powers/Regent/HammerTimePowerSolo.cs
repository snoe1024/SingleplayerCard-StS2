using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Regent;

// Backs HammerTimeSolo (see .claude/loadmap.md "ハンマータイム" 案1). Vanilla HAMMER_TIME_POWER
// makes all allies Forge whenever the owner Forges -- meaningless alone. Singleplayer rework
// instead tracks every card that Forged this turn, and whenever Sovereign Blade is played, replays
// all of them (once each) against Sovereign Blade's target. Guarded against re-entrancy so replayed
// cards forging again don't trigger another replay pass within the same Sovereign Blade play.
public sealed class HammerTimePowerSolo : SingleplayerCardPower
{
    private readonly List<CardModel> _forgedThisTurn = new();
    private bool _isReplaying;

    protected override string? OriginalVanillaPowerId => "HAMMER_TIME_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();

    public override Task AfterForge(decimal amount, Player forger, AbstractModel? source)
    {
        if (!_isReplaying && forger == Owner.Player && source is CardModel card && !_forgedThisTurn.Contains(card))
        {
            _forgedThisTurn.Add(card);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_isReplaying || cardPlay.Card.Owner != Owner.Player || cardPlay.Card is not SovereignBlade)
        {
            return;
        }

        var toReplay = _forgedThisTurn.Where(c => c != cardPlay.Card).ToList();
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

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            _forgedThisTurn.Clear();
        }

        return Task.CompletedTask;
    }
}
