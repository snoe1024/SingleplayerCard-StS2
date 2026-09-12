using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Regent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: CONSTELLATION (Uncommon, 0 cost + 2 star, Skill). Another player
// draws 1 card, gains 1 Energy, and gains 9(12) Block.
// Singleplayer rework (see .claude/loadmap.md "星座" 案1): no other player, so the same effect is
// instead granted at the start of the owner's own next turn (see ConstellationPowerSolo).
[Pool(typeof(RegentCardPool))]
public sealed class ConstellationSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public override int CanonicalStarCost => 2;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1),
        new EnergyVar(1),
        new BlockVar(9m, ValueProp.Move)
    };

    public ConstellationSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<ConstellationPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
        {
            power.Cards = DynamicVars.Cards.IntValue;
            power.Energy = DynamicVars.Energy.IntValue;
            power.Block = DynamicVars.Block.BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
