#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using System;

namespace GSPAWN
{
    public static class TextFieldEx
    {
        public static void focusEx(this TextField textField)
        {
            textField.ElementAt(0).Focus();
        }

        public static void registerDragAndDropCallback(this TextField textField, Action dragPerformAction, DragAndDropVisualMode dragAndDropVisualMode)
        {
            textField.RegisterCallback<DragUpdatedEvent>((p) =>
            { PluginDragAndDrop.visualMode = dragAndDropVisualMode; });
            textField.RegisterCallback<DragPerformEvent>((p) => { dragPerformAction(); });
        }
    }
}
#endif