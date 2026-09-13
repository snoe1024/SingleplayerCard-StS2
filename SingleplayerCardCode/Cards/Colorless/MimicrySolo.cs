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
// that card if it has more than one; ignores calculated/special block like CalculatedBlockVar).
// Passing the result through the normal GainBlock pipeline means Dexterity etc. apply to it exactly
// as they would to that card, on top of whatever Dexterity already did to the printed value itself
// -- an intentional "double-dip" the loadmap calls out explicitly, not a bug.
[Pool(typeof(ColorlessCardPool))]
public sealed class MimicrySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "MIMIC";

    protected override string OriginalVanillaCardPool => "colorless";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    public MimicrySolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal best = 0m;
        foreach (CardModel card in CardPile.GetCards(Owner, PileType.Hand))
        {
            if (card == this)
            {
                continue;
            }

            decimal total = card.DynamicVars.Values.OfType<BlockVar>().Sum(v => v.BaseValue);
            if (total > best)
            {
                best = total;
            }
        }

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
