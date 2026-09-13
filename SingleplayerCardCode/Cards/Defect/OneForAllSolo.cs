using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: ONE_FOR_ALL (Rare Power) -- see .claude/loadmap.md
// "ワン・フォー・オール" for current numbers. EVERYONE'S 0-cost Attacks deal additional damage.
// Singleplayer rework (see .claude/loadmap.md "ワン・フォー・オール" 案1, identical to its own
// xDRO -- the loadmap notes this one works fine unreworked): simply targets yourself instead of
// ALL allies. Reuses vanilla OneForAllPower as-is since it already has no multiplayer-specific
// logic of its own.
[Pool(typeof(DefectCardPool))]
public sealed class OneForAllSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "ONE_FOR_ALL";

    protected override string OriginalVanillaCardPool => "defect";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<OneForAllPower>(3m) };

    public OneForAllSolo() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<OneForAllPower>(choiceContext, Owner.Creature, DynamicVars["OneForAllPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OneForAllPower"].UpgradeValueBy(1m);
    }
}
