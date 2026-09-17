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

[Pool(typeof(ColorlessCardPool))]
public sealed class GangUpSolo : SingleplayerCardCard
{
    private readonly List<(CardModel Card, Creature Target)> _attacksThisTurn = new();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "GANG_UP";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar("BonusPerAttack", 5m)
    };

    public GangUpSolo() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
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
        
        decimal totalDamage = DynamicVars.Damage.BaseValue + DynamicVars["BonusPerAttack"].BaseValue * _attacksThisTurn.Count;
        await DamageCmd.Attack(totalDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusPerAttack"].UpgradeValueBy(2m);
    }
}
