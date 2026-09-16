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
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public class HandOverPowerSolo : SingleplayerCardPower
{
    private CardModel? _handOverCard;
    
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // Registers a plain, always-empty StringVar in CanonicalVars and keeps it in sync from this
    // setter -- same pattern vanilla's own ImitationLearningPower.PlayerTarget uses for its
    // "TargetPlayer" var. DynamicVars.AddTo(locString) (see PowerModel.GetHoverTip) adds every
    // registered var to the smartDescription LocString automatically, so {CardName} just needs to
    // exist as a var here; no manual wiring on the description-building side.
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
            ((StringVar)DynamicVars["CardName"]).StringValue = value?.Title ?? "";
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("CardName")];

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
            await ReturnToPile(PileType.Draw, CardPilePosition.Random);
            await PowerCmd.Remove(this);
        }
    }

    public override async Task BeforeDeath(Creature target)
    {
        if (Owner != target || HandOverCard == null)
        {
            return;
        }

        await ReturnToPile(PileType.Discard, CardPilePosition.Bottom);
        await PowerCmd.Remove(this);
    }

    // Re-attaches HandOverCard to the enemy's CombatState (it was fully detached while held, via
    // reflection since neither hook is exposed publicly -- see the comment on the two field/property
    // lookups below), then flies it from the enemy's on-screen position to the destination pile using
    // NCardFlyVfx, the same node vanilla uses for its own card-movement vfx (e.g. Ball's
    // player-to-player hand-off, CardPileCmd.GiveToAnotherPlayer). There is no live NCard for this
    // card at this point (it was fully removed from combat -- see TheBallSolo.FlyToCreature's silent
    // removal -- so no node ever needed cleaning up), so a fresh one is created here purely to animate
    // the return trip, positioned at the enemy's own vfx spawn point as the flight's start position.
    private async Task ReturnToPile(PileType pileType, CardPilePosition position)
    {
        if (HandOverCard == null)
        {
            return;
        }

        ICombatState? combatState = HandOverCard.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        // HasBeenRemovedFromState/CombatState._allCards have no public API for "put a fully-removed
        // card back" (RemoveFromCombat's counterpart, CardPileCmd.Add, expects a card that's merely
        // moving BETWEEN combat piles, not one that was removed from CombatState entirely) -- this
        // mirrors what CardPileCmd.RemoveFromCombat itself would have flipped on removal.
        PropertyInfo? removedProp = typeof(CardModel).GetProperty("HasBeenRemovedFromState", BindingFlags.Public | BindingFlags.Instance);
        removedProp?.SetValue(HandOverCard, false);

        FieldInfo? allCardsField = typeof(CombatState).GetField("_allCards", BindingFlags.NonPublic | BindingFlags.Instance);
        if (allCardsField?.GetValue(combatState) is List<CardModel> allCards && !allCards.Contains(HandOverCard))
        {
            allCards.Add(HandOverCard);
        }

        NCard? cardNode = NCard.Create(HandOverCard);
        Vector2? startPos = Owner.GetCreatureNode()?.VfxSpawnPosition;
        if (cardNode != null && startPos.HasValue && NCombatRoom.Instance != null)
        {
            NCombatRoom.Instance.Ui.AddChildSafely(cardNode);
            cardNode.GlobalPosition = startPos.Value;
        }

        await CardPileCmd.Add(HandOverCard, pileType, position, skipVisuals: true);

        if (cardNode != null && startPos.HasValue)
        {
            NCardFlyVfx? flyVfx = NCardFlyVfx.Create(cardNode, pileType, isAddingToPile: true, HandOverCard.Owner.Character.TrailPath);
            NCombatRoom.Instance?.Ui.AddChildSafely(flyVfx);
        }
        else
        {
            // No live scene to animate in (e.g. TestMode) -- NCardFlyVfx would normally fire this once
            // the flight lands, so without it the pile's displayed count would otherwise go stale (see
            // the CardAddFinished/ContentsChanged gotcha in sts2_dev_knowledge).
            HandOverCard.Pile?.InvokeCardAddFinished();
        }
    }

    public void Take(CardModel card)
    {
        base.Target = card.Owner.Creature;
        HandOverCard = card;
    }
}