using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

[Pool(typeof(RegentCardPool))]
public sealed class ConstellationSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "CONSTELLATION";

    protected override string OriginalVanillaCardPool => "regent";

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
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
