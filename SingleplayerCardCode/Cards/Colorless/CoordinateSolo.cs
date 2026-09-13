using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: COORDINATE (Uncommon Skill) -- see .claude/loadmap.md "連携" for
// current numbers. Give another player Strength this turn.
// Singleplayer rework (see .claude/loadmap.md "連携"). Reference implementation for how a card's
// DRO-on (Rework/案1) and DRO-off (xDRO) effects coexist in one class -- see SingleplayerCardCard's
// DroActiveForDisplay for the shared plumbing this relies on.
// - DRO on (案1): no other player, so instead grants Strength this turn equal to the number of
//   cards currently in hand. Cost lowers on upgrade.
// - DRO off (xDRO): grants a flat Strength this turn instead (matching the original amount), with
//   cost staying the same always.
[Pool(typeof(ColorlessCardPool))]
public sealed class CoordinateSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "COORDINATE";

    protected override string OriginalVanillaCardPool => "colorless";

    // Only consumed by the xDRO branch, but declared unconditionally: CanonicalVars is read once (on
    // the canonical instance) and its resulting DynamicVarSet is value-cloned onto every owned copy
    // (see AfterCloned's comment in SingleplayerCardCard), so branching this list itself on
    // DroActiveForDisplay would not track the option correctly. Always declare the full set of vars
    // either branch might need, and choose which ones are actually used at read time instead.
    // Uses the default single-arg PowerVar constructor (name = "StrengthPower", matching vanilla's own
    // SetupStrike.cs) rather than a custom name -- this card only ever declares one Strength var, so
    // there's no name collision to avoid, and the default name is what DynamicVarSet.Strength expects.
    // A custom name is only needed if a single card's two branches must coexist as two independent
    // vars of the same power type.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(5m)
    };

    public CoordinateSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            int cardsInHand = CardPile.GetCards(Owner, PileType.Hand).Count();
            if (cardsInHand > 0)
            {
                await PowerCmd.Apply<CoordinatePowerSolo>(choiceContext, Owner.Creature, cardsInHand, Owner.Creature, this);
            }
        }
        else
        {
            await PowerCmd.Apply<CoordinatePowerSolo>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroWasOnAtCreation)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else
        {
            DynamicVars.Strength.UpgradeValueBy(3m);
        }
    }

    // The card's ".description" loc key is just "{DroEffectText}" (see localization/*/cards.json);
    // this resolves whichever of the two real text bodies (".descriptionRework"/".descriptionXdro")
    // applies right now and feeds it in as that one variable, so both branches stay fully
    // translatable in the normal JSON loc files instead of being hardcoded here.
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
