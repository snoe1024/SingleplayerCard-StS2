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

// Original multiplayer card: DEMONIC_SHIELD (Uncommon Skill, Exhaust [removed on upgrade]) -- see
// .claude/loadmap.md "悪魔の盾" for current numbers. Lose HP. Give another player Block equal to
// your own Block. Exhaust.
// Singleplayer rework (see .claude/loadmap.md "悪魔の盾"):
// - DRO on (案1): "other player" has no singleplayer equivalent, so instead of granting block to
//   someone else, this accumulates the HP lost during the whole combat and pays it back as Block
//   when played. CalculatedVar's Calculate() is BaseVar.BaseValue + ExtraVar.BaseValue * multiplier
//   (see sts2_dev_knowledge/topics/dynamic-vars.md) -- CalculationBase/CalculationExtra are declared
//   as 0/1 so Calculate() reduces to exactly the multiplier lambda's own summed value.
// - DRO off (xDRO): matches the original's own-Block-doubling shape (no other-player equivalent
//   needed here since it targets itself), just without the multiplayer distribution.
[Pool(typeof(IroncladCardPool))]
public sealed class DemonicShieldSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "DEMONIC_SHIELD";

    protected override string OriginalVanillaCardPool => "ironclad";

    public override bool GainsBlock => true;

    // Only consumed by the Rework branch. CalculationBase/CalculationExtra must be declared here too
    // -- CalculatedVar.Calculate() unconditionally reads DynamicVars.CalculationBase/CalculationExtra
    // (both hard-cast named accessors), so omitting them throws a KeyNotFoundException the moment
    // Calculate() runs, including during the card's own preview rendering.
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

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
