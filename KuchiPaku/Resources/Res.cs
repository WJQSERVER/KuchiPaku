using System;
using System.Resources;
using System.Globalization;

namespace KuchiPaku.Resources
{
    public class Res
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager {
            get {
                if (ReferenceEquals(resourceMan, null)) {
                    resourceMan = new ResourceManager("KuchiPaku.Resources.Resources", typeof(Res).Assembly);
                }
                return resourceMan;
            }
        }

        public static CultureInfo Culture {
            get => resourceCulture;
            set => resourceCulture = value;
        }

        public static string OpenYmmpLabel => ResourceManager.GetString("OpenYmmpLabel", resourceCulture) ?? string.Empty;
        public static string SaveYmmpLabel => ResourceManager.GetString("SaveYmmpLabel", resourceCulture) ?? string.Empty;
        public static string OverwriteLabel => ResourceManager.GetString("OverwriteLabel", resourceCulture) ?? string.Empty;
        public static string BackupSaveLabel => ResourceManager.GetString("BackupSaveLabel", resourceCulture) ?? string.Empty;
        public static string OpenWithSaveLabel => ResourceManager.GetString("OpenWithSaveLabel", resourceCulture) ?? string.Empty;
        public static string YmmpPlaceholder => ResourceManager.GetString("YmmpPlaceholder", resourceCulture) ?? string.Empty;
        public static string CharaLipSyncSettingsHeader => ResourceManager.GetString("CharaLipSyncSettingsHeader", resourceCulture) ?? string.Empty;
        public static string ExportToggleToolTip => ResourceManager.GetString("ExportToggleToolTip", resourceCulture) ?? string.Empty;
        public static string LipSyncSetting1 => ResourceManager.GetString("LipSyncSetting1", resourceCulture) ?? string.Empty;
        public static string LipSyncSetting2 => ResourceManager.GetString("LipSyncSetting2", resourceCulture) ?? string.Empty;
        public static string AddButtonLabel => ResourceManager.GetString("AddButtonLabel", resourceCulture) ?? string.Empty;
        public static string LipSyncRuleLabel => ResourceManager.GetString("LipSyncRuleLabel", resourceCulture) ?? string.Empty;
        public static string StandardRule => ResourceManager.GetString("StandardRule", resourceCulture) ?? string.Empty;
        public static string StandardRuleToolTip => ResourceManager.GetString("StandardRuleToolTip", resourceCulture) ?? string.Empty;
        public static string LipSyncRule2 => ResourceManager.GetString("LipSyncRule2", resourceCulture) ?? string.Empty;
        public static string OptionsHeader => ResourceManager.GetString("OptionsHeader", resourceCulture) ?? string.Empty;
        public static string EnableVisualLeadLabel => ResourceManager.GetString("EnableVisualLeadLabel", resourceCulture) ?? string.Empty;
        public static string VisualLeadHeader => ResourceManager.GetString("VisualLeadHeader", resourceCulture) ?? string.Empty;
        public static string OthersHeader => ResourceManager.GetString("OthersHeader", resourceCulture) ?? string.Empty;
        public static string LicenseButtonLabel => ResourceManager.GetString("LicenseButtonLabel", resourceCulture) ?? string.Empty;
        public static string KuchiPakuWebsiteLabel => ResourceManager.GetString("KuchiPakuWebsiteLabel", resourceCulture) ?? string.Empty;
        public static string YMM4WebsiteLabel => ResourceManager.GetString("YMM4WebsiteLabel", resourceCulture) ?? string.Empty;
        public static string OpenYmmpDialogTitle => ResourceManager.GetString("OpenYmmpDialogTitle", resourceCulture) ?? string.Empty;
        public static string YmmpFileFilter => ResourceManager.GetString("YmmpFileFilter", resourceCulture) ?? string.Empty;
        public static string YmmpNotLoadedTitle => ResourceManager.GetString("YmmpNotLoadedTitle", resourceCulture) ?? string.Empty;
        public static string YmmpNotLoadedMessage => ResourceManager.GetString("YmmpNotLoadedMessage", resourceCulture) ?? string.Empty;
        public static string SavingTitle => ResourceManager.GetString("SavingTitle", resourceCulture) ?? string.Empty;
        public static string SavingMessage => ResourceManager.GetString("SavingMessage", resourceCulture) ?? string.Empty;
        public static string AnalyzingVoiceItemsMessage => ResourceManager.GetString("AnalyzingVoiceItemsMessage", resourceCulture) ?? string.Empty;
        public static string NoVoiceItemsTitle => ResourceManager.GetString("NoVoiceItemsTitle", resourceCulture) ?? string.Empty;
        public static string NoVoiceItemsMessage => ResourceManager.GetString("NoVoiceItemsMessage", resourceCulture) ?? string.Empty;
        public static string FilteringExportItemsMessage => ResourceManager.GetString("FilteringExportItemsMessage", resourceCulture) ?? string.Empty;
        public static string GeneratingCustomVoiceLipSyncMessage => ResourceManager.GetString("GeneratingCustomVoiceLipSyncMessage", resourceCulture) ?? string.Empty;
        public static string GeneratingApiVoiceLipSyncMessage => ResourceManager.GetString("GeneratingApiVoiceLipSyncMessage", resourceCulture) ?? string.Empty;
        public static string SavingFileMessage => ResourceManager.GetString("SavingFileMessage", resourceCulture) ?? string.Empty;
        public static string SaveYmmpDialogTitle => ResourceManager.GetString("SaveYmmpDialogTitle", resourceCulture) ?? string.Empty;
        public static string SaveSuccessTitle => ResourceManager.GetString("SaveSuccessTitle", resourceCulture) ?? string.Empty;
        public static string SaveSuccessMessage => ResourceManager.GetString("SaveSuccessMessage", resourceCulture) ?? string.Empty;
        public static string FolderNotFoundTitle => ResourceManager.GetString("FolderNotFoundTitle", resourceCulture) ?? string.Empty;
        public static string FolderNotFoundMessage => ResourceManager.GetString("FolderNotFoundMessage", resourceCulture) ?? string.Empty;
        public static string KuchiFolderNotFoundTitle => ResourceManager.GetString("KuchiFolderNotFoundTitle", resourceCulture) ?? string.Empty;
        public static string KuchiFolderNotFoundMessage => ResourceManager.GetString("KuchiFolderNotFoundMessage", resourceCulture) ?? string.Empty;
        public static string ALine => ResourceManager.GetString("ALine", resourceCulture) ?? string.Empty;
        public static string ILine => ResourceManager.GetString("ILine", resourceCulture) ?? string.Empty;
        public static string ULine => ResourceManager.GetString("ULine", resourceCulture) ?? string.Empty;
        public static string ELine => ResourceManager.GetString("ELine", resourceCulture) ?? string.Empty;
        public static string OLine => ResourceManager.GetString("OLine", resourceCulture) ?? string.Empty;
        public static string NLine => ResourceManager.GetString("NLine", resourceCulture) ?? string.Empty;
        public static string ConsonantOption_ALL_N => ResourceManager.GetString("ConsonantOption_ALL_N", resourceCulture) ?? string.Empty;
        public static string ConsonantOption_CONTINUE_BEFORE_VOWEL => ResourceManager.GetString("ConsonantOption_CONTINUE_BEFORE_VOWEL", resourceCulture) ?? string.Empty;
        public static string ConsonantOption_SMALL_MOUSE => ResourceManager.GetString("ConsonantOption_SMALL_MOUSE", resourceCulture) ?? string.Empty;
    }
}
