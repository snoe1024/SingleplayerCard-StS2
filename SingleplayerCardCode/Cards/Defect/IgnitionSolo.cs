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
