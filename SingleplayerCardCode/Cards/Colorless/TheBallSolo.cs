using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Keywords;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class TheBallSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "THE_BALL";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [SingleplayerCardKeywords.CreateForHandOver().Tip(this)];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("IncreaseRemake", 5m),
        new DynamicVar("IncreaseXdro", 10m)
    };

    public TheBallSolo() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (DroActiveForDisplay)
        {
            DynamicVars.Damage.BaseValue += DynamicVars["IncreaseRemake"].BaseValue;
            
            var handOverPowers = cardPlay.Target.GetPowerInstances<HandOverPowerSolo>();
            var exists = false;
            foreach (var handOverPower in handOverPowers)
            {
                if (handOverPower.HandOverCard == this)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                var handOverPower = await PowerCmd.Apply<HandOverPowerSolo>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
                handOverPower?.Take(this);

                await CardPileCmd.RemoveFromCombat(this, false);
            }
        }
        else
        {
            DynamicVars.Damage.BaseValue += DynamicVars["IncreaseXdro"].BaseValue;
        }
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation location = base.GetResultLocationForCardPlay();
        
        if (DroActiveForDisplay)
        {
            location.pileType = PileType.None;
            location.position = CardPilePosition.Bottom;
        }
        else if (location.pileType == PileType.Discard)
        {
            location.pileType = PileType.Draw;
            location.position = CardPilePosition.Random;
        }

        return location;
    }

    private static Tween? GetTweenForMoveToCreature(IEnumerable<(NCard, PileType?)> cards, Creature target)
    {
        throw new NotImplementedException();
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["IncreaseRemake"].UpgradeValueBy(5m);
        }
        else
        {
            DynamicVars["IncreaseXdro"].UpgradeValueBy(5m);
        }
    }
}
