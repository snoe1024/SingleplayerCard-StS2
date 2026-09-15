using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public class HandOverPowerSolo : SingleplayerCardPower
{
    private CardModel? _handOverCard;
    
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public CardModel? HandOverCard
    {
        get
        {
            return _handOverCard;
        }
        set
        {
            AssertMutable();
            _handOverCard = value;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (HandOverCard == null)
            {
                return [];
            }
            return [HoverTipFactory.FromCard(HandOverCard)];
        }
    }
    
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (HandOverCard != null && dealer == base.Owner && target == HandOverCard.Owner.Creature)
        {
            var combatState = HandOverCard.Owner.Creature.CombatState;
            if (combatState == null)
                return;
            
            var removedProp = typeof(CardModel).GetProperty("HasBeenRemovedFromState", BindingFlags.Public | BindingFlags.Instance);
            removedProp?.SetValue(HandOverCard, false);
            
            var allCardsField = typeof(CombatState).GetField("_allCards", BindingFlags.NonPublic | BindingFlags.Instance);
            var allCards = allCardsField?.GetValue(combatState) as List<CardModel>;
            if (allCards != null && !allCards.Contains(HandOverCard))
            {
                allCards.Add(HandOverCard);
            }

            await CardPileCmd.Add(HandOverCard, PileType.Draw, CardPilePosition.Random);
            await PowerCmd.Remove(this);
        }
    }

    public override Task BeforeDeath(Creature target)
    {
        if (Owner != target || HandOverCard == null)
        {
            return Task.CompletedTask;
        }
        
        var combatState = HandOverCard.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return Task.CompletedTask;
        }
            
        var removedProp = typeof(CardModel).GetProperty("HasBeenRemovedFromState", BindingFlags.Public | BindingFlags.Instance);
        removedProp?.SetValue(HandOverCard, false);
            
        var allCardsField = typeof(CombatState).GetField("_allCards", BindingFlags.NonPublic | BindingFlags.Instance);
        var allCards = allCardsField?.GetValue(combatState) as List<CardModel>;
        if (allCards != null && !allCards.Contains(HandOverCard))
        {
            allCards.Add(HandOverCard);
        }
        
        CardPileCmd.Add(HandOverCard, PileType.Discard, CardPilePosition.Top);
        PowerCmd.Remove(this);

        return Task.CompletedTask;
    }

    public void Take(CardModel card)
    {
        base.Target = card.Owner.Creature;
        HandOverCard = card;
    }
}