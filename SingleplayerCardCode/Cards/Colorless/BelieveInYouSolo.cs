using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: BELIEVE_IN_YOU (Uncommon Skill) -- see .claude/loadmap.md
// "お前を信じる" for current numbers. Another player gains Energy immediately.
// Singleplayer rework (see .claude/loadmap.md "お前を信じる"):
// - DRO on (案1): reflavored as believing in your own tomorrow-self instead of another player --
//   next turn, once you've played an Attack, a Skill, AND a Power, gain Energy (see
//   BelieveInYouPowerSolo).
// - DRO off (xDRO): matches the original -- gain Energy immediately, a flat (different) amount.
[Pool(typeof(ColorlessCardPool))]
public sealed class BelieveInYouSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "BELIEVE_IN_YOU";

    protected override string OriginalVanillaCardPool => "colorless";

    // 1 (the Rework/案1 base) since Rework is this mod's default variant; AfterCloned overwrites
    // this to the xDRO flat amount (2) when that branch is active instead.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new EnergyVar(1) };

    public BelieveInYouSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!DroActiveForDisplay)
        {
            DynamicVars.Energy.BaseValue = 2m;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<BelieveInYouPowerSolo>(choiceContext, Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
        }
        else
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
