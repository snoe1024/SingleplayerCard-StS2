using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

public sealed class StealthPowerSolo : SingleplayerCardPower
{
    public bool DroActiveForDisplay { get; private set; } = true;

    protected override string? OriginalVanillaPowerId => "SNEAKY_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Block) };

    public override LocString Description => new LocString("powers", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));

    protected override string SmartDescriptionLocKey => Id.Entry + (DroActiveForDisplay ? ".smartDescriptionRework" : ".smartDescriptionXdro");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is SingleplayerCardCard soloCard)
        {
            DroActiveForDisplay = soloCard.DroActiveForDisplay;
        }
        return base.AfterApplied(applier, cardSource);
    }
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (DroActiveForDisplay && amount != 0m && power.GetTypeForAmount(amount) == PowerType.Debuff && power.Owner.IsEnemy && applier == base.Owner && !(power is ITemporaryPower))
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!DroActiveForDisplay && cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }
}
