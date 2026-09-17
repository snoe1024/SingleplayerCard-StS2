using BaseLib.Abstracts;
using BaseLib.Extensions;
using GamblerMod.GamblerModCode.Patch;
using MegaCrit.Sts2.Core.Combat;
using SingleplayerCard.SingleplayerCardCode.Config;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SingleplayerCard.SingleplayerCardCode.Cards;

public abstract class SingleplayerCardCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target), IDynamicCardDescription
{
    // Setter must stay public -- BaseLib's [SavedProperty] restore path can't reach a non-public setter.
    [SavedProperty]
    public bool DroWasOnAtCreation { get; set; }

    // Not [SavedProperty]: intentionally reset on every real save reload (see sts2_dev_knowledge/
    // model-lifecycle-and-saves.md).
    private bool _droBranchGuessed;

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!_droBranchGuessed)
        {
            DroWasOnAtCreation = DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId);
            _droBranchGuessed = true;
        }
        RefreshDroBranchState();
    }

    internal protected bool DroActiveForDisplay => IsCanonical ? DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId) : DroWasOnAtCreation;

    private string BaseTitleText => OriginalVanillaCardId != null
        ? new LocString("cards", OriginalVanillaCardId + ".title").GetFormattedText()
        : TitleLocString.GetFormattedText();

    private string DroSuffix => (LocManager.Instance.Language == "jpn" ? "" : " ") + (DroActiveForDisplay ? "R" : "S");

    public override string Title
    {
        get
        {
            string title = BaseTitleText + DroSuffix;
            if (IsUpgraded)
            {
                title += MaxUpgradeLevel > 1 ? $"+{CurrentUpgradeLevel}" : "+";
            }
            return title;
        }
    }

    protected virtual bool DroVersionExists => false;

    // Pure function of (DroActiveForDisplay, IsUpgraded/CurrentUpgradeLevel) -- see
    // sts2_dev_knowledge/model-lifecycle-and-saves.md for why this can't be an incremental "+= delta".
    protected virtual void RefreshDroBranchState() { }

    public LocString GetDynamicDescription(LocString original)
    {
        if (IsMutable)
        {
            RefreshDroBranchState();
        }
        if (!DroVersionExists) return original;

        return new LocString("cards", this.Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
    }

    protected virtual string? OriginalVanillaCardId => null;
    protected virtual string? OriginalVanillaCardPool => null;

    internal string? VanillaCardIdForConfig => OriginalVanillaCardId;

    protected void AddCommonDescriptionArgs(LocString target)
    {
        target.Add(new IfUpgradedVar(IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal));
        target.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    private string? VanillaPortraitPath => OriginalVanillaCardId != null && OriginalVanillaCardPool != null
        ? ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{OriginalVanillaCardPool}/{OriginalVanillaCardId.ToLowerInvariant()}.tres")
        : null;

    public override string CustomPortraitPath => VanillaPortraitPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => VanillaPortraitPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => VanillaPortraitPath ?? $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}
