using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

[Pool(typeof(IroncladCardPool))]
public sealed class OutrageSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "OUTRAGE";

    protected override string OriginalVanillaCardPool => "ironclad";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DamageVar("DamageRemake", 8m, ValueProp.Move),
        new DamageVar("DamageXdro", 9m, ValueProp.Move)
    };

    public OutrageSolo() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override void RefreshDroBranchState()
    {
        DynamicVars.Damage.BaseValue = DroActiveForDisplay
            ? DynamicVars["DamageRemake"].BaseValue
            : DynamicVars["DamageXdro"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (CombatState == null)
        {
            return;
        }

        List<CardModel> sourcesToClone = new List<CardModel> { this };
        if (DroActiveForDisplay)
        {
            sourcesToClone.AddRange(CardPile.GetCards(Owner, PileType.Hand).Where(c => c is OutrageSolo && c != this));
        }
        foreach (CardModel source in sourcesToClone)
        {
            CardModel card = source.CreateCloneForPlayer(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner), 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["DamageRemake"].UpgradeValueBy(3m);
        }
        else
        {
            DynamicVars["DamageXdro"].UpgradeValueBy(4m);
        }
        RefreshDroBranchState();
    }
}
