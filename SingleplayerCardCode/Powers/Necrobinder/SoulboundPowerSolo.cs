using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

public sealed class SoulboundPowerSolo : SingleplayerCardPower
{
    public bool DroActiveForDisplay { get; private set; } = true;

    private bool _hasTriggeredThisTurn;
    private bool _isAddingSoul;

    protected override string? OriginalVanillaPowerId => "SOULBOUND_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

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

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (DroActiveForDisplay && side == CombatSide.Player)
        {
            _hasTriggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (!DroActiveForDisplay || _hasTriggeredThisTurn || target != Owner || Applier == null || !props.IsPoweredAttack() || result.UnblockedDamage <= 0m)
        {
            return;
        }

        _hasTriggeredThisTurn = true;
        Flash();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Applier.Player, Amount, CombatState), PileType.Draw, Applier.Player, CardPilePosition.Random));
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (DroActiveForDisplay || _isAddingSoul || creator == null || creator.Creature != Applier || card is not Soul)
        {
            return;
        }

        _isAddingSoul = true;
        Flash();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Owner.Player, Amount, CombatState), PileType.Draw, Owner.Player, CardPilePosition.Random));
        _isAddingSoul = false;
    }
}
