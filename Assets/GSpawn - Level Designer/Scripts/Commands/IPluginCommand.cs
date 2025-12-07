#if UNITY_EDITOR
namespace GSPAWN
{
    public interface IPluginCommand
    {
        void enter  ();
        void exit   ();
    }
}
#endif