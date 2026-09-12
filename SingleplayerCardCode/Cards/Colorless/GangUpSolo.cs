using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: GANG_UP (Uncommon, 1 cost, Attack). Deal 5 damage, +5(7) more for
// each time ANOTHER player attacked the enemy this turn.
// Singleplayer rework (see .claude/loadmap.md "総攻撃" 案1): no other player to count, so instead
// this deals a flat 10 damage and replays every OTHER Attack card played against this same enemy
// so far this turn. Tracked by this card instance itself listening for AfterCardPlayed while
// sitting in any pile (all cards receive combat hooks regardless of pile), reset each turn start.
[Pool(typeof(ColorlessCardPool))]
public sealed class GangUpSolo : SingleplayerCardCard
{
    private readonly List<(CardModel Card, Creature Target)> _attacksThisTurn = new();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public GangUpSolo() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _attacksThisTurn.Clear();
        }

        return base.AfterPlayerTurnStart(choiceContext, player);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this && cardPlay.Card.Type == CardType.Attack && cardPlay.Target != null)
        {
            _attacksThisTurn.Add((cardPlay.Card, cardPlay.Target));
        }

        return base.AfterCardPlayed(choiceContext, cardPlay);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(10m).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var toReplay = _attacksThisTurn.Where(a => a.Target == cardPlay.Target).Select(a => a.Card).ToList();
        bool first = true;
        foreach (CardModel card in toReplay)
        {
            await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !first);
            first = false;
        }
    }
}
