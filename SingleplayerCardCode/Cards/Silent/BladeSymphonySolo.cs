using System;
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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: BLADE_SYMPHONY (Uncommon Skill) -- see .claude/loadmap.md
// "ブレイド・シンフォニー" for current numbers. Add Shivs to ALL players' hands.
// Singleplayer rework (see .claude/loadmap.md "ブレイド・シンフォニー"):
// - DRO on (案1): no other players to hand Shivs to, so instead this replays every Shiv-tagged card
//   currently sitting in the Discard or Exhaust pile against a chosen enemy (same AutoPlay pattern as
//   vanilla KNIFE_TRAP, which only reads the Exhaust pile -- this also covers ones that overflowed to
//   Discard). Approximates "Shivs generated this turn" as "Shivs currently off the hand/draw/play
//   piles", since there's no cheap way to timestamp when a specific card instance was generated. No
//   cost reduction on upgrade (kept cheap enough already).
// - DRO off (xDRO): matches the original exactly (minus needing other players) -- add 2 Shivs to
//   hand via Shiv.CreateInHand, same helper vanilla's own OnPlay uses. Cost reduces on upgrade like
//   the original. TargetType stays AnyEnemy (fixed at construction) even though this branch doesn't
//   use the target -- a minor UX mismatch accepted for simplicity.
[Pool(typeof(SilentCardPool))]
public sealed class BladeSymphonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "BLADE_SYMPHONY";

    protected override string OriginalVanillaCardPool => "silent";

    public BladeSymphonySolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            var shivs = PileType.Discard.GetPile(Owner).Cards
                .Concat(PileType.Exhaust.GetPile(Owner).Cards)
                .Where(c => c.Tags.Contains(CardTag.Shiv))
                .ToList();

            bool first = true;
            foreach (CardModel shiv in shivs)
            {
                if (IsUpgraded)
                {
                    CardCmd.Upgrade(shiv, CardPreviewStyle.None);
                }

                await CardCmd.AutoPlay(choiceContext, shiv, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !first);
                first = false;
            }
        }
        else
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (CombatState == null)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                await Shiv.CreateInHand(Owner, CombatState);
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        if (!DroActiveForDisplay)
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
