using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Regent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: CONSTELLATION (Uncommon Skill) -- see .claude/loadmap.md "星座" for
// current numbers (cost includes a star-cost component). Another player draws a card, gains
// Energy, and gains Block.
// Singleplayer rework (see .claude/loadmap.md "星座"):
// - DRO on (案1): no other player, so the same effect is instead granted at the start of the
//   owner's own next turn (see ConstellationPowerSolo).
// - DRO off (xDRO): matches the original -- grants the same draw/Energy/Block immediately instead
//   of deferring, same numbers as 案1.
[Pool(typeof(RegentCardPool))]
public sealed class ConstellationSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

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
            var power = await PowerCmd.Apply<ConstellationPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            if (power != null)
            {
                power.Cards = DynamicVars.Cards.IntValue;
                power.Energy = DynamicVars.Energy.IntValue;
                power.Block = DynamicVars.Block.BaseValue;
            }
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

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
