using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: INTERCEPT (Uncommon Skill) -- see .claude/loadmap.md
// "インターセプト" for current numbers. Gain Block; redirect all incoming attacks meant for
// another player to yourself this turn -- meaningless when you're already the only target.
// Singleplayer rework (see .claude/loadmap.md "インターセプト" -- note this section of loadmap.md
// itself is stale as of this writing; the numbers/description below instead match the more recent
// settings_ui.json hover text, which reflects the author's later verbal revision that xDRO should be
// a plain, valid (if unexciting) Block card rather than "実質破綻"):
// - DRO on (案1): raises Block and adds Weak on yourself in place of the (now meaningless) tanking
//   utility.
// - DRO off (xDRO): a plain, slightly lower Block gain with no Weak -- the original's "redirect
//   attacks" utility is simply dropped rather than treated as game-breaking, since existing purely as
//   a middling Block card is a valid (if bland) outcome.
[Pool(typeof(ColorlessCardPool))]
public sealed class InterceptSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "INTERCEPT";

    protected override string OriginalVanillaCardPool => "colorless";

    public override bool GainsBlock => true;

    // 13m (the Rework/案1 base) since Rework is this mod's default variant -- see MidnightSolo's
    // AfterCloned comment for why CanonicalVars can only ever hold one representative number and the
    // Card Library preview won't reflect live DRO toggling.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new BlockVar(13m, ValueProp.Move) };

    public InterceptSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        DynamicVars.Block.BaseValue = DroActiveForDisplay ? 13m : 9m;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(DroActiveForDisplay ? 6m : 4m);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
