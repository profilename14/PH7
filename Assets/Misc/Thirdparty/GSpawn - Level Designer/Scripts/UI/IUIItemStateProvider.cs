#if UNITY_EDITOR
namespace GSPAWN
{
    public interface IUIItemStateProvider
    {
        bool            uiSelected      { get; set; }
        CopyPasteMode   uiCopyPasteMode { get; set; }
        PluginGuid      guid            { get; }
    }
}
#endif