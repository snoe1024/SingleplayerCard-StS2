using System;
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
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

[Pool(typeof(DefectCardPool))]
public sealed class ImitationLearningSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "IMITATION_LEARNING";

    protected override string OriginalVanillaCardPool => "defect";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new CardsVar("ImitationRemake", 2),
        new CardsVar("ImitationXdro", 2)
    ];

    public ImitationLearningSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<ImitationLearningPowerSolo>(choiceContext, Owner.Creature, DynamicVars["ImitationRemake"].IntValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<SignalBoostPower>(choiceContext, Owner.Creature, DynamicVars["ImitationXdro"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["ImitationRemake"].UpgradeValueBy(1m);
        }
        else
        {
            DynamicVars["ImitationXdro"].UpgradeValueBy(1m);
        }
    }
}
