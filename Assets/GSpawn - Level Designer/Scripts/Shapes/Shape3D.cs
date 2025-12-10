#if UNITY_EDITOR
namespace GSPAWN
{
    public abstract class Shape3D
    {
        public abstract void drawFilled();
        public abstract void drawWire();
    }
}
#endif