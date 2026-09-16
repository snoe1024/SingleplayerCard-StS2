using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: BLADE_SYMPHONY (Uncommon Skill) -- see .claude/loadmap.md
// "ブレイド・シンフォニー" for current numbers. Add Shivs to ALL players' hands.
// Singleplayer rework (see .claude/loadmap.md "ブレイド・シンフォニー"):
// - DRO on (案1): no other players to hand Shivs to, so instead this plays every Shiv generated
//   THIS TURN, wherever it currently is (hand, hand-overflow discard, reshuffled into the draw pile,
//   or already Exhausted from an earlier play this turn -- replaying an already-played Shiv is
//   intentional here, same combo potential as vanilla KNIFE_TRAP's own unrestricted Exhaust-pile
//   replay, and is a deliberate power source for this branch, not a bug), upgrading each first if
//   this card is upgraded. Base cost raised to 2 (from 1) and upgrade now reduces cost by 1 (from no
//   reduction) to compensate for how strong unrestricted replay is. "Generated this turn" comes from
//   CombatHistory: every Shiv creation already logs a CardGeneratedEntry holding the exact CardModel
//   instance, and CombatHistoryEntry.HappenedThisTurn filters it to the current turn -- see
//   sts2_dev_knowledge/topics/gotchas.md for why this beats a per-instance hook (which would break
//   for a copy of this card generated mid-turn by a Skill Potion).
// - DRO off (xDRO): matches the original exactly (minus needing other players) -- add 2 Shivs to
//   hand via Shiv.CreateInHand, same helper vanilla's own OnPlay uses. Cost stays 1, reduces to 0 on
//   upgrade, matching the original. TargetType stays AnyEnemy (fixed at construction) even though
//   this branch doesn't use the target -- a minor UX mismatch accepted for simplicity.
[Pool(typeof(SilentCardPool))]
public sealed class BladeSymphonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "BLADE_SYMPHONY";

    protected override string OriginalVanillaCardPool => "silent";
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Shiv>()];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];
    
    public BladeSymphonySolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        if (CombatState == null)
        {
            return;
        }
        
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<BladeSymphonyPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                await Shiv.CreateInHand(Owner, CombatState);
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
