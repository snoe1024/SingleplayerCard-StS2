using System;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Records which vanilla card (by OriginalVanillaCardId) a SingleplayerCardConfig dropdown property
// controls. DuplicateReworkManager reflects over SingleplayerCardConfig's properties once, keyed by
// this, so resolving a card's variant never needs per-card code -- see SingleplayerCardCard.
[AttributeUsage(AttributeTargets.Property)]
public sealed class CardVariantForAttribute(string vanillaCardId) : Attribute
{
    public string VanillaCardId { get; } = vanillaCardId;
}
