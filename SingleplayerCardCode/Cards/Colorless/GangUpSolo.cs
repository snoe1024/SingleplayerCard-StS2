using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: GANG_UP (Uncommon Attack) -- see .claude/loadmap.md "総攻撃" for
// current numbers. Deal damage, with more for each time ANOTHER player attacked the enemy this
// turn.
// Singleplayer rework (see .claude/loadmap.md "総攻撃"):
// - DRO on (案1): no other player to count, so instead this deals flat damage and replays every
//   OTHER Attack card played against this same enemy so far this turn. Tracked by this card
//   instance itself listening for AfterCardPlayed while sitting in any pile (all cards receive
//   combat hooks regardless of pile), reset each turn start.
// - DRO off (xDRO): matches the original -- flat base damage, plus a per-Attack-played-this-turn
//   damage bonus (reusing the same tracked list, just counting instead of replaying). Cost lowered
//   to match the original.
[Pool(typeof(ColorlessCardPool))]
public sealed class GangUpSolo : SingleplayerCardCard
{
    private readonly List<(CardModel Card, Creature Target)> _attacksThisTurn = new();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "GANG_UP";

    protected override string OriginalVanillaCardPool => "colorless";

    // Rework/案1 base (10, +4=14 on upgrade) since Rework is this mod's default variant;
    // AfterCloned overwrites this to xDRO's flat base (5, no upgrade scaling) when that branch is
    // active instead. xDRO's separate per-attack BONUS is tracked by its own var, "BonusPerAttack".
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("BonusPerAttack", 5m)
    };

    public GangUpSolo() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!DroActiveForDisplay)
        {
            DynamicVars.Damage.BaseValue = 5m;
            EnergyCost.SetCustomBaseCost(1);
        }
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _attacksThisTurn.Clear();
        }

        return base.AfterPlayerTurnStart(choiceContext, player);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this && cardPlay.Card.Type == CardType.Attack && cardPlay.Target != null)
        {
            _attacksThisTurn.Add((cardPlay.Card, cardPlay.Target));
        }

        return base.AfterCardPlayed(choiceContext, cardPlay);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (DroActiveForDisplay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            var toReplay = _attacksThisTurn.Where(a => a.Target == cardPlay.Target).Select(a => a.Card).ToList();
            bool first = true;
            foreach (CardModel card in toReplay)
            {
                await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !first);
                first = false;
            }
        }
        else
        {
            decimal totalDamage = DynamicVars.Damage.BaseValue + DynamicVars["BonusPerAttack"].BaseValue * _attacksThisTurn.Count;
            await DamageCmd.Attack(totalDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
        else
        {
            DynamicVars["BonusPerAttack"].UpgradeValueBy(2m);
        }
    }
}
