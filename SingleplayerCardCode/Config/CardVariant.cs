namespace SingleplayerCard.SingleplayerCardCode.Config;

// Per-card selection for cards that have a genuine, distinct xDRO/Original effect (see loadmap.md).
// Declaration order is dropdown display order.
public enum CardVariant
{
    Rework,
    Original,
    Disabled
}

// For cards whose xDRO effect is either unusable ("破綻") or identical to the Rework effect anyway
// (loadmap.md notes this explicitly per-card) -- there's nothing distinct to offer as "Original", so
// the dropdown only offers two choices.
public enum CardVariantNoOriginal
{
    Rework,
    Disabled
}
