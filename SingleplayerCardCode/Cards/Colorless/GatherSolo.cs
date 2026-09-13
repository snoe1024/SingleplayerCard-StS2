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
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: RALLY (Rare Skill) -- see .claude/loadmap.md "結集" for current
// numbers. ALL players gain Block immediately.
// Singleplayer rework (see .claude/loadmap.md "結集"):
// - DRO on (案1): no other players, so instead gain Block equal to the sum of every other hand
//   card's printed Block value. A CalculatedVar ("HandBlockTotal") mirrors this exact computation so
//   the card's own description can show a live preview of how much Block it would currently grant
//   (same technique as DemonicShieldSolo's "HP lost this combat" preview) -- CalculationBase/
//   CalculationExtra are declared as 0/1 so Calculate() reduces to exactly the multiplier lambda's
//   own summed value (see sts2_dev_knowledge/topics/dynamic-vars.md). OnPlay calls the SAME
//   CalculatedVar rather than recomputing the sum separately, so the preview can never drift from
//   the actual effect. The multiplier lambda reads DynamicVars.Values.OfType<BlockVar>() (a
//   type-filtered scan of the OTHER card's own var dictionary) rather than the named .Block accessor
//   -- see sts2_dev_knowledge/topics/dynamic-vars.md for why that accessor specifically must never be
//   used for a foreign card (it hard-casts and can throw); this type-filtered enumeration sidesteps
//   that entirely and is safe.
// - DRO off (xDRO): matches the original -- a flat Block gain.
[Pool(typeof(ColorlessCardPool))]
public sealed class GatherSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "RALLY";

    protected override string OriginalVanillaCardPool => "colorless";

    public override bool GainsBlock => true;

    // BlockVar(12m) only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment
    // for why this is still declared unconditionally. CalculationBase/CalculationExtra/HandBlockTotal
    // are only consumed by the Rework branch, for the same reason.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(12m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("HandBlockTotal").WithMultiplier((card, _) => CardPile.GetCards(card.Owner, PileType.Hand).Where(c => c != card).SelectMany(c => c.DynamicVars.Values.OfType<BlockVar>()).Sum(v => v.BaseValue))
    };

    public GatherSolo() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            decimal total = ((CalculatedVar)DynamicVars["HandBlockTotal"]).Calculate(null);
            if (total > 0m)
            {
                await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay);
            }
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        if (!DroActiveForDisplay)
        {
            DynamicVars.Block.UpgradeValueBy(5m);
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
