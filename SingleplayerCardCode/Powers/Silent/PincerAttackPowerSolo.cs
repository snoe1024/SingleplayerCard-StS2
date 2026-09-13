using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

// Backs PincerAttackSolo (see .claude/loadmap.md "挟撃" 案1). Vanilla FLANKING_POWER doubles
// damage the target takes from Attacks except the applier's own. Singleplayer has only one
// attacker, so instead this excludes Attack cards that were already in hand at the moment this
// was cast -- only Attacks drawn/generated afterward this turn get doubled.
public sealed class PincerAttackPowerSolo : SingleplayerCardPower
{
    private readonly HashSet<CardModel> _excludedCards = new();

    protected override string? OriginalVanillaPowerId => "FLANKING_POWER";

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public void SetExcludedCards(IEnumerable<CardModel> cards)
    {
        _excludedCards.Clear();
        foreach (var card in cards)
        {
            _excludedCards.Add(card);
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }

        if (cardSource != null && _excludedCards.Contains(cardSource))
        {
            return 1m;
        }

        return Amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
