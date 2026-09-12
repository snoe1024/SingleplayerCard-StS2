using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: THE_BALL (public-beta only per loadmap.md; Uncommon, 1 cost, Attack).
// Deal 10 damage; increase this card's damage by 10(15) this combat and pass it to a random ally
// (lands in their Draw Pile).
// Singleplayer rework (see .claude/loadmap.md "ボール" 案1): the increase is halved to 5(10) since
// there's no longer a rotation of allies diluting how often you see it again, and instead of
// passing to an ally, the buffed copy lands in a random spot among your own Draw Pile, Hand, or
// Discard Pile.
[Pool(typeof(ColorlessCardPool))]
public sealed class TheBallSolo : SingleplayerCardCard
{
    private static readonly PileType[] RandomPiles = { PileType.Draw, PileType.Hand, PileType.Discard };

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "THE_BALL";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("Increase", 5m)
    };

    public TheBallSolo() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        DynamicVars.Damage.BaseValue += DynamicVars["Increase"].BaseValue;
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation location = base.GetResultLocationForCardPlay();
        location.pileType = Owner.RunState.Rng.CombatCardGeneration.NextItem(RandomPiles);
        if (location.pileType != PileType.Hand)
        {
            location.position = CardPilePosition.Random;
        }

        return location;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(5m);
    }
}
