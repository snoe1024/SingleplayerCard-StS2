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
    
    public override PowerType Type => PowerType.Debuff;

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
            // The card was fully removed from combat (no Pile, no live NCard on the table) before
            // being held here, so the normal move-tween that CardPileCmd.Add's visual flow expects to
            // fire the pile's UI count label event (NCombatCardPile listens for CardAddFinished, not
            // ContentsChanged) never finds a card to animate and silently skips it -- leaving the
            // Draw Pile's displayed count stale even though Cards.Count itself is correct. Fire it
            // explicitly rather than relying on the tween path.
            HandOverCard.Pile?.InvokeCardAddFinished();
            await PowerCmd.Remove(this);
        }
    }

    public override async Task BeforeDeath(Creature target)
    {
        if (Owner != target || HandOverCard == null)
        {
            return;
        }

        var combatState = HandOverCard.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        var removedProp = typeof(CardModel).GetProperty("HasBeenRemovedFromState", BindingFlags.Public | BindingFlags.Instance);
        removedProp?.SetValue(HandOverCard, false);

        var allCardsField = typeof(CombatState).GetField("_allCards", BindingFlags.NonPublic | BindingFlags.Instance);
        var allCards = allCardsField?.GetValue(combatState) as List<CardModel>;
        if (allCards != null && !allCards.Contains(HandOverCard))
        {
            allCards.Add(HandOverCard);
        }

        await CardPileCmd.Add(HandOverCard, PileType.Discard, CardPilePosition.Top);
        // See the matching comment in AfterDamageGiven above.
        HandOverCard.Pile?.InvokeCardAddFinished();
        await PowerCmd.Remove(this);
    }

    public void Take(CardModel card)
    {
        base.Target = card.Owner.Creature;
        HandOverCard = card;
    }
}