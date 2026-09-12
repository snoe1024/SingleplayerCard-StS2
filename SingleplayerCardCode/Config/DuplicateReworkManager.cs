using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Single resolution point for whether the Duplicate Rework Option (DRO, see .claude/loadmap.md) is
// active. Every ported card reads its DRO state through this instead of MainFile.Config directly.
//
// Today this just returns the local mod config's global toggle -- there is neither a per-card
// override UI nor multiplayer support for these cards yet. The cardId/contextPlayer parameters
// exist so that when either of those ships (a per-card override table, or resolving the setting from
// a multiplayer host rather than the local client), only this method needs to change; no card's
// OnPlay/Title logic has to be touched.
public static class DuplicateReworkManager
{
    public static bool IsEnabled(ModelId? cardId = null, Player? contextPlayer = null)
    {
        return MainFile.Config.DuplicateReworkOption;
    }
}
