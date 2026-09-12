using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: DEMONIC_SHIELD (Uncommon, 0 cost, Skill, Exhaust [removed on upgrade]).
// Lose 1 HP. Give another player Block equal to your own Block. Exhaust.
// Singleplayer rework (see .claude/loadmap.md "悪魔の盾" 案1): "other player" has no singleplayer
// equivalent, so instead of granting block to someone else, this accumulates the HP lost during the
// whole combat and pays it back as Block when played.
[Pool(typeof(IroncladCardPool))]
public sealed class DemonicShieldSolo : SingleplayerCardCard
{
    private decimal _hpLostThisCombat;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "DEMONIC_SHIELD";

    protected override string OriginalVanillaCardPool => "ironclad";

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public DemonicShieldSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override Task BeforeCombatStart()
    {
        _hpLostThisCombat = 0m;
        return base.BeforeCombatStart();
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (delta < 0m && creature == Owner.Creature)
        {
            _hpLostThisCombat += -delta;
        }

        return base.AfterCurrentHpChanged(creature, delta);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        await CreatureCmd.GainBlock(Owner.Creature, _hpLostThisCombat, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
