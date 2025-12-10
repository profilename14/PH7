#if UNITY_EDITOR
namespace GSPAWN
{
    public interface IGridView
    {
        int             dragAndDropInitiatorId  { get; }
        System.Object   dragAndDropData         { get; }
    }
}
#endif