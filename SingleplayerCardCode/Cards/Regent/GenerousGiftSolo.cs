using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Regent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

[Pool(typeof(RegentCardPool))]
public sealed class GenerousGiftSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "LARGESSE";

    protected override string OriginalVanillaCardPool => "regent";

    public GenerousGiftSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (DroActiveForDisplay)
        {
            if (IsUpgraded)
            {
                await PowerCmd.Apply<NextTurnCardGenerationPlusPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
            else
            {
                await PowerCmd.Apply<NextTurnCardGenerationPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
        }
        else
        {
            CardModel? card = CardFactory.GetDistinctForCombat(Owner, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint), 1, Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
            if (card != null)
            {
                if (IsUpgraded)
                {
                    CardCmd.Upgrade(card);
                }

                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
            }
        }
    }
}
