using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: TUTOR (Rare, 1(0) cost, Skill). Another player chooses a card in
// their Draw Pile to add to their Hand.
// Singleplayer rework (see .claude/loadmap.md "教示" 案1, which is identical to its own xDRO):
// simply targets yourself instead of another player -- the effect and numbers are otherwise
// unchanged, since there was nothing multiplayer-specific left to rework once retargeted.
[Pool(typeof(RegentCardPool))]
public sealed class TutorSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "TUTOR";

    protected override string OriginalVanillaCardPool => "regent";

    public TutorSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? card = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(Owner), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null)).FirstOrDefault();
        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
