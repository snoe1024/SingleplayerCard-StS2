using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Enchantments;

public sealed class TrainEnchantmentSolo : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool ShowAmount => true;
    
    protected override string CustomIconPath => $"enchantments/{Id.Entry.ToLowerInvariant()}.png".ImagePath();
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(0m)];

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await PowerCmd.Apply<TrainEnchantmentPowerSolo>(choiceContext, base.Card.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Card.Owner.Creature, cardPlay.Card);
    }

    public override void RecalculateValues()
    {
        base.DynamicVars.Strength.BaseValue = base.Amount;
    }
}
