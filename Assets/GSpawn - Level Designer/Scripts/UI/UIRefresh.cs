#if UNITY_EDITOR
namespace GSPAWN
{
    public static class UIRefresh
    {
        public static void refreshShortcutToolTips()
        {
            PluginInspectorUI.instance.refresh();
        }
    }
}
#endif