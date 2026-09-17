using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

[Pool(typeof(IroncladCardPool))]
public sealed class DemonicShieldSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "DEMONIC_SHIELD";

    protected override string OriginalVanillaCardPool => "ironclad";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("LoseHPAmount").WithMultiplier((card, _) => CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Where(e => e.Receiver == card.Owner.Creature && e.Result.UnblockedDamage > 0).Sum(e => e.Result.UnblockedDamage))
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public DemonicShieldSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);

        if (DroActiveForDisplay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, ((CalculatedVar)DynamicVars["LoseHPAmount"]).Calculate(cardPlay.Target), ValueProp.Move, cardPlay);
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, Owner.Creature.Block, ValueProp.Unpowered | ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
