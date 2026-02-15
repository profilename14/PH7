using UnityEngine;
using Random = UnityEngine.Random;

namespace Dustyroom {
public class FloatingMotion : MonoBehaviour {
    public float verticalAmplitude = 1.0f;
    public float horizontalAmplitude = 0.0f;
    public bool startAtRandomOffset = true;

    [Space]
    public float speed = 1.0f;

    [Space, Tooltip("In seconds")]
    public float startDelay = 0;

    [Space]
    public bool worldSpace = false;

    private Vector3 _initialPosition;
    private float _offsetH = 0f;
    private float _offsetV = 0f;
    private bool _isMoving = false;

    private void Start() {
        Invoke(nameof(Initialize), startDelay);
    }

    private void Initialize() {
        _initialPosition = worldSpace ? transform.position : transform.localPosition;

        if (startAtRandomOffset) {
            _offsetH = Random.value * 1000f;
            _offsetV = Random.value * 1000f;
        }

        _isMoving = true;
    }

    private void Update() {
        if (!_isMoving) {
            return;
        }

        var hDirection = new Vector3(Mathf.Sin(Time.timeSinceLevelLoad * speed * 0.5f + _offsetV + 100f), 0f,
            Mathf.Cos(Time.timeSinceLevelLoad * speed + _offsetV + 100f));
        Vector3 offset = Vector3.up * (Mathf.Sin(Time.timeSinceLevelLoad * speed + _offsetH) * verticalAmplitude) +
                         hDirection * (Mathf.Sin(Time.timeSinceLevelLoad * speed + _offsetV) * horizontalAmplitude);
        Vector3 position = _initialPosition + offset * Time.timeScale;
        if (worldSpace) {
            transform.position = position;
        } else {
            transform.localPosition = position;
        }
    }
}
}