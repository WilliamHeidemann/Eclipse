using UnityEngine;

namespace Game.Presentation;

public class CameraBackground : MonoBehaviour
{
    private const string ResourcePath = "Backgrounds/GameBackground";
    private const float Distance = 50f;

    private Camera _camera;
    private SpriteRenderer _renderer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        var sprite = Resources.Load<Sprite>(ResourcePath);
        if (sprite == null) return;
        var go = new GameObject("Camera Background");
        DontDestroyOnLoad(go);
        var bg = go.AddComponent<CameraBackground>();
        bg._renderer = go.AddComponent<SpriteRenderer>();
        bg._renderer.sprite = sprite;
        bg._renderer.sortingOrder = short.MinValue;
    }

    private void LateUpdate()
    {
        if (_camera == null || !_camera.isActiveAndEnabled)
        {
            _camera = Camera.main;
            if (_camera == null) return;
        }

        var t = _camera.transform;
        transform.SetPositionAndRotation(t.position + t.forward * Distance, t.rotation);

        float height, width;
        if (_camera.orthographic)
        {
            height = _camera.orthographicSize * 2f;
            width = height * _camera.aspect;
        }
        else
        {
            height = 2f * Distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            width = height * _camera.aspect;
        }

        var size = _renderer.sprite.bounds.size;
        float scale = Mathf.Max(width / size.x, height / size.y);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}