using UnityEngine;
using System.Collections;

public class FramesPerSecond : MonoBehaviour
{

    // Attach this to a GUIText to make a _frames/second indicator.
    //
    // It calculates _frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting _frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 _frames.

    public float updateInterval = 0.5F;

    private float _accum = 0; // FPS accumulated over the interval
    private long _frames = 0; // Frames drawn over the interval
    private float _secsleft; // Left time (seconds) for current interval
    private GUIStyle _style;
    private Rect _position;
    private readonly GUIContent _content = new GUIContent("");


    void Start()
    {
        enabled = true;

        _secsleft = updateInterval;

        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        texture.Apply();
        _style = new GUIStyle { fontSize = 20, normal = new GUIStyleState { textColor = Color.white, background = texture } };

    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint || _content.text == null) return;
        GUI.Label(_position, _content, _style);

    }

    void Update()
    {
        _secsleft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;

        // Interval ended - update GUI text and start new interval
        if (_secsleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            var fps = _accum / _frames;
            var text = System.String.Format("{0:F2} FPS " + Plane.InfoStr, fps);
            _content.text = text;

            var size = _style.CalcSize(_content);

            _position = new Rect(0, 0, size.x, size.y);

            _style.normal.textColor =
                fps < 25
                    ? Color.red
                    : fps < 30
                        ? Color.magenta
                        : Color.black;

            _secsleft = updateInterval;
            _accum = 0.0F;
            _frames = 0;


        }
    }
}