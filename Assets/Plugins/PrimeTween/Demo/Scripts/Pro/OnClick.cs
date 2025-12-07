using UnityEngine;
using UnityEngine.Events;

namespace PrimeTweenDemo {
    // p0 todo replace with AnimateOnClick and serialize TweenAnimation instead? no, use OnMouseDown from TweenAnimationComponent
    // p0 todo create Demo Pro
    public class OnClick : MonoBehaviour {
        [SerializeField] public UnityEvent onClick = new UnityEvent();

        void Update() {
            if (InputController.GetDown()) {
                Vector2 screenPos = InputController.screenPosition;
                var ray = Camera.main.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out var hit) && IsChild(hit.transform, transform)) {
                    Debug.Log("onClick", this);
                    onClick.Invoke();
                }
            }
        }

        private static bool IsChild(Transform t, Transform other) {
            Transform parent = t.parent;
            while (parent != null) {
                if (parent == other) {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }
    }
}
