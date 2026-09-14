using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: IGNITION (Rare Skill, Exhaust) -- see .claude/loadmap.md "イグニッション"
// for current numbers. Another player Channels Plasma.
// Singleplayer rework (see .claude/loadmap.md "イグニッション"):
// - DRO on (案1): no other player, so instead this Channels Plasma AND immediately triggers the
//   Passive of every Plasma orb currently slotted (matches vanilla "Fusion" exactly when only the new
//   orb is counted, but this also retriggers any Plasma orbs already in the queue).
// - DRO off (xDRO): just Channels Plasma, matching the original card exactly aside from not needing
//   another player.
// Exhaust removed on upgrade in both branches, matching the original.
[Pool(typeof(DefectCardPool))]
public sealed class IgnitionSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "IGNITION";

    protected override string OriginalVanillaCardPool => "defect";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<PlasmaOrb>()
    };

    public IgnitionSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await OrbCmd.Channel<PlasmaOrb>(choiceContext, Owner);
        if (DroActiveForDisplay)
        {
            foreach (PlasmaOrb orb in Owner.PlayerCombatState!.OrbQueue.Orbs.OfType<PlasmaOrb>().ToList())
            {
                await OrbCmd.Passive(choiceContext, orb, null);
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
