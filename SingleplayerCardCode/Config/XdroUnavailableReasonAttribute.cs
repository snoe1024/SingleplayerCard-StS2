using System;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Why a CardVariantNoOriginal card has no Original choice -- see .claude/loadmap.md, which
// distinguishes these explicitly per card rather than treating them the same:
public enum XdroUnavailableReason
{
    // The xDRO effect doesn't function correctly in singleplayer at all (e.g. Tank's xDRO becomes a
    // do-nothing or purely-negative power with no other player to target).
    Breaks,

    // The xDRO effect is word-for-word identical to the Rework effect anyway (loadmap.md notes this
    // explicitly, e.g. "案1 = xDRO"), so there's nothing distinct to offer as a second choice.
    Identical
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class XdroUnavailableReasonAttribute(XdroUnavailableReason reason) : Attribute
{
    public XdroUnavailableReason Reason { get; } = reason;
}
