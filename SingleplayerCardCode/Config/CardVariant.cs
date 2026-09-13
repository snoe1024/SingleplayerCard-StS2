namespace SingleplayerCard.SingleplayerCardCode.Config;

// Per-card selection for cards that have a genuine, distinct xDRO/Original effect (see loadmap.md).
// Declaration order is dropdown display order.
public enum CardVariant
{
    Rework,
    Original,
    Disabled
}

// For cards whose xDRO effect is either unusable ("破綻") -- see XdroUnavailableReason.Breaks -- there's
// nothing distinct to offer as "Original", so the dropdown only offers two choices.
public enum CardVariantNoOriginal
{
    Rework,
    Disabled
}

// For cards ported completely unchanged: vanilla's own effect never needed adjusting to avoid
// duplicating an existing singleplayer card's effect (see XdroUnavailableReason.Identical), so there's
// no separate "Rework" to offer at all -- the single active choice IS this card's original effect, not
// a rework of it. Labeled Original (not CardVariantNoOriginal's Rework) so the dropdown doesn't
// misleadingly imply something was actually changed.
public enum CardVariantOriginalOnly
{
    Original,
    Disabled
}
