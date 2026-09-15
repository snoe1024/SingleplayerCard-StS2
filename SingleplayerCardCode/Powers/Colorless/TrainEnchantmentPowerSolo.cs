using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Cards.Colorless;
using SingleplayerCard.SingleplayerCardCode.Enchantments;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public class TrainEnchantmentPowerSolo : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Enchantment<TrainEnchantmentSolo>();
    
}