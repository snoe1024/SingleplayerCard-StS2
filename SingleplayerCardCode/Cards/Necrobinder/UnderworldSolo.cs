using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: UNDERWORLD (Uncommon Skill, Exhaust) -- see .claude/loadmap.md "冥界"
// for current numbers. This turn, whenever OTHER players deal Attack damage, apply that much Doom.
// Singleplayer rework (see .claude/loadmap.md "冥界"): reworked into UnderworldPowerSolo (see its own
// doc comment for both branches' causality and cost). Rework's cost is lowered from the original;
// xDRO keeps the original's cost. Exhaust removed on upgrade in both branches, matching the original.
[Pool(typeof(NecrobinderCardPool))]
public sealed class UnderworldSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "UNDERWORLD";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<DoomPower>() };

    public UnderworldSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // Pure function of DroActiveForDisplay -- see base class doc comment. Cost never changes with
    // upgrade in either branch (upgrade only removes Exhaust, see OnUpgrade below), so no IsUpgraded
    // term is needed here.
    protected override void RefreshDroBranchState()
    {
        EnergyCost.SetCustomBaseCost(DroActiveForDisplay ? 1 : 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<UnderworldPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
