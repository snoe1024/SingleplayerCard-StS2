using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Keywords;

public static class SingleplayerCardKeywords
{
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword HandOver;

    public static bool IsHandOver(this CardModel card)
    {
        return card.Keywords.Contains(HandOver);
    }
    
    public static TooltipSource CreateForHandOver()
    {
        var title = new LocString("card_keywords", $"SINGLEPLAYERCARD-HAND_OVER.title");
        var desc = new LocString("card_keywords", $"SINGLEPLAYERCARD-HAND_OVER.description");
        
        // TODO: insert icon(Texture2D) as 3rd arguments
        return new TooltipSource(_ => new HoverTip(title, desc.GetFormattedText()));
    }
}