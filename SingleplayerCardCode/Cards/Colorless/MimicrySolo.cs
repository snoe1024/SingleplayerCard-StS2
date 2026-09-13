using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: MIMIC (Rare Skill, Exhaust) -- see .claude/loadmap.md "ものまね" for
// current numbers. Gain Block equal to another player's CURRENT Block stat -- meaningless alone.
// Singleplayer rework (see .claude/loadmap.md "ものまね" 案1): instead, gain Block equal to the
// printed Block value of the single highest-Block card currently in hand (summing all BlockVars on
// that card if it has more than one; ignores calculated/special block like CalculatedBlockVar). Has
// no xDRO branch (loadmap.md marks it as "破綻" -- there's no other player to reference at all), so
// this always uses the Rework behavior; see CardVariantNoOriginal in SingleplayerCardConfig.
// Passing the result through the normal GainBlock pipeline means Dexterity etc. apply to it exactly
// as they would to that card, on top of whatever Dexterity already did to the printed value itself
// -- an intentional "double-dip" the loadmap calls out explicitly, not a bug.
// A CalculatedVar ("BestHandBlock") mirrors the highest-card computation so the card's own
// description can show a live preview of how much Block it would currently grant (same technique as
// DemonicShieldSolo's "HP lost this combat" preview) -- CalculationBase/CalculationExtra are
// declared as 0/1 so Calculate() reduces to exactly the multiplier lambda's own value (see
// sts2_dev_knowledge/topics/dynamic-vars.md). OnPlay calls the SAME CalculatedVar rather than
// recomputing the search separately, so the preview can never drift from the actual effect.
[Pool(typeof(ColorlessCardPool))]
public sealed class MimicrySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "MIMIC";

    protected override string OriginalVanillaCardPool => "colorless";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("BestHandBlock").WithMultiplier((card, _) => CardPile.GetCards(card.Owner, PileType.Hand).Where(c => c != card).Select(c => c.DynamicVars.Values.OfType<BlockVar>().Sum(v => v.BaseValue)).DefaultIfEmpty(0m).Max())
    };

    public MimicrySolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal best = ((CalculatedVar)DynamicVars["BestHandBlock"]).Calculate(null);
        if (best > 0m)
        {
            await CreatureCmd.GainBlock(Owner.Creature, best, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
