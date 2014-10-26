using UnityEngine;
using System.Collections;
using com.youvisio;
using System.Threading;

public class Plane : MonoBehaviour {

    public static string InfoStr;

    private GameObject _camera;
    private GameObject _sphere;
    private BackgroundWorker _backgroundWorker;

	// Use this for initialization
	void Start () {
        _camera = UnityEngine.GameObject.Find("Main Camera");
        _sphere = UnityEngine.GameObject.Find("Sphere");
        var text = UnityEngine.GameObject.Find("Text");
        var bc = text.AddComponent<BoxCollider>();
        bc.center = new Vector3(0, 0, 0);
        bc.size = new Vector3(7, 1, 1);
        bc.transform.parent = text.transform;
	}
	
	// Update is called once per frame
	void Update () {

        HandleTouch();
        


        _sphere.transform.position = 
            RotateAbout(
                _sphere.transform.position, 
                new Vector3(0, 1, 0), // rotate about this point
                new Vector3(0, 1, 0), // on a plane with this normal
                30 * Time.smoothDeltaTime);// 30 degrees per second

        if (_backgroundWorker != null) _backgroundWorker.Update();
	}
    private void OnTouched()
    {
        if (_backgroundWorker != null) _backgroundWorker.CancelAsync();
        _backgroundWorker = new BackgroundWorker();

        if (string.IsNullOrEmpty(Thread.CurrentThread.Name)) Thread.CurrentThread.Name = "Main";

        Debug.Log("START " + Thread.CurrentThread.Name);
        Plane.InfoStr += "\nS("+Thread.CurrentThread.Name+");";
        _backgroundWorker.DoWork += (o, a) =>
        {
            Debug.Log("INSIDE1 " + Thread.CurrentThread.Name);
            Plane.InfoStr += "IN1("+Thread.CurrentThread.Name+");"+a.Argument+";";
            for (var i = 0; i < 10000000; ++i)
            {
                if (a.IsCanceled) return;

                var n = 67876 + i / 100f;
                var x = Mathf.Pow(n, 3);
            }
            Debug.Log("INSIDE2 " + Thread.CurrentThread.Name);
            Plane.InfoStr += "IN2("+Thread.CurrentThread.Name+");";

            a.Result = a.Argument+"!";
        };
        _backgroundWorker.RunWorkerCompleted += (o, a) =>
        {
            Debug.Log("END " + Thread.CurrentThread.Name);
            Plane.InfoStr += "E("+Thread.CurrentThread.Name+");"+a.Result+";";
        };
        
        _backgroundWorker.RunWorkerAsync("A1");
    }

    #region utils
    private bool _touching;
    private void HandleTouch()
    {
        var touches = Input.touches;
        var len = touches.Length;
        Vector2 pos2d = new Vector2(0, 0);
        if (len == 0)
        {
            if (Input.GetMouseButton(0))
            {
                pos2d = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                len = 1;
            }
        }
        else
        {
            pos2d = new Vector2(touches[0].position.x, Screen.height - touches[0].position.y);
        }


        if (len > 0)
        {
            if (!_touching)
            {
                var ray = _camera.camera.ViewportPointToRay(new Vector3(pos2d.x / Screen.width, 1f - pos2d.y / Screen.height, 1f));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var hitName = hit.transform.name;
                    if (hitName == "Text")
                    {
                        _touching = true;
                        OnTouched();
                    }
                }
            }            
            return;
        }
        if (_touching)
        {
            _touching = false;
        }
        
    }

    public static Vector3 RotateAbout(Vector3 point, Vector3 pivot, Vector3 axis, float degrees)
    {
        return (Quaternion.AngleAxis(degrees, axis) * (point - pivot)) + pivot;
    }
    #endregion
}
