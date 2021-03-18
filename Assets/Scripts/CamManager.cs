using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Newtonsoft.Json;


public class CamManager : MonoBehaviour
{
    public Camera m_Camera = null;

    public float CanvasInWorldWidth;
    public float CanvasInWorldHeight;

    public float CanvasInWorldHalfWidth;
    public float CanvasInWorldHalfHeight;

    public int refCanvasWidth;
    public int refCanvasHeight;


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Camera    = GetComponent<Camera>();

        refCanvasWidth = m_Camera.scaledPixelWidth;
        refCanvasHeight = m_Camera.scaledPixelHeight;

        Vector3 worldPos1 = m_Camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldPos2 = m_Camera.ScreenToWorldPoint(new Vector3(m_Camera.scaledPixelWidth, m_Camera.scaledPixelHeight, 0));

        CanvasInWorldWidth = Mathf.Abs(worldPos2.x - worldPos1.x);
        CanvasInWorldHeight = Mathf.Abs(worldPos2.y - worldPos1.y);

        CanvasInWorldHalfWidth = CanvasInWorldWidth / 2.0f;
        CanvasInWorldHalfHeight = CanvasInWorldHalfHeight / 2.0f;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
