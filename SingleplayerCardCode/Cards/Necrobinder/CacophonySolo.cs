using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: CACOPHONY (Rare, 2 cost, Power). Every 33 cards drawn by ALL players,
// deal 66(99) damage to a random enemy.
// Singleplayer rework (see .claude/loadmap.md "不協和音" 案1): 33-card windows are far too large
// for a single player to realistically hit, so this lowers the threshold to 12 draws and the
// damage to 16(24), keeping the exact same mechanism (CacophonyPowerSolo).
[Pool(typeof(NecrobinderCardPool))]
public sealed class CacophonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "CACOPHONY";

    protected override string OriginalVanillaCardPool => "necrobinder";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(12),
        new DamageVar(16m, ValueProp.Unpowered)
    };

    public CacophonySolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CacophonyPowerSolo>(choiceContext, Owner.Creature, DynamicVars.Damage.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
