using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: BLAZE (public-beta only per loadmap.md; Uncommon, 2 cost, Skill).
// Give another player 5(7) Strength.
// Singleplayer rework (see .claude/loadmap.md "ブレイズ" 案1): no other player to buff, so this
// instead grants 2(3) Strength per card discarded this turn (so far), rewarding a discard-heavy
// turn instead of being a flat repeatable Strength card. Tracked via a per-instance counter reset
// each of the owner's turns and incremented on discard (all cards, including ones sitting in a
// pile, receive combat hooks — see CombatState.IterateHookListeners in the decompiled source).
[Pool(typeof(IroncladCardPool))]
public sealed class BlazeSolo : SingleplayerCardCard
{
    private int _discardedThisTurn;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<StrengthPower>(2m) };

    public BlazeSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _discardedThisTurn = 0;
        }

        return base.AfterPlayerTurnStart(choiceContext, player);
    }

    public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        _discardedThisTurn++;
        return base.AfterCardDiscarded(choiceContext, card);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_discardedThisTurn > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue * _discardedThisTurn, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
    }
}
