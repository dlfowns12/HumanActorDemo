using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Newtonsoft.Json;



public class RecordScreenPix
{
    public int width;
    public int height;

    public int screenW;
    public int screenH;

}

public class CamManager : MonoBehaviour
{

    public Camera m_Camera = null;
    private Transform m_OrigTrans;
 

    private Vector3 camPos;
    private Quaternion camRot;

    public float CanvasInWorldWidth;
    public float CanvasInWorldHeight;


    public float CanvasInWorldHalfWidth;
    public float CanvasInWorldHalfHeight;


    public int refCanvasWidth;
    public int refCanvasHeight;



    void Awake()
    {

        DontDestroyOnLoad(this.gameObject);


       // Screen.SetResolution(1440, 2880, true, 60);


    }

    // Start is called before the first frame update
    void Start()
    {

        m_Camera    = GetComponent<Camera>();
        m_OrigTrans = GetComponent<Transform>();

        camPos = GetComponent<Transform>().localPosition;
        camRot = GetComponent<Transform>().localRotation;



        // Screen.SetResolution(1170, 2532, false);

        // Screen.SetResolution(1080, 1280, false);


        refCanvasWidth = m_Camera.scaledPixelWidth;
        refCanvasHeight = m_Camera.scaledPixelHeight;


        //画布大小应该是 显示在客户端 画布大大小

        calcCanVasInWorldSize();

    }

    // Update is called once per frame
    void Update()
    {
        


    }

    public void calcCanVasInWorldSize()
    {

        Debug.Log("width:" + m_Camera.scaledPixelWidth);
        Debug.Log("height:" + m_Camera.scaledPixelHeight);

        Vector3 worldPos1 = m_Camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldPos2 = m_Camera.ScreenToWorldPoint(new Vector3(m_Camera.scaledPixelWidth, m_Camera.scaledPixelHeight, 0));
        
        //Debug.Log("worldPos1:" + worldPos1);
        //Debug.Log("worldPos2:" + worldPos2);

        CanvasInWorldWidth = Mathf.Abs(worldPos2.x - worldPos1.x);
        CanvasInWorldHeight = Mathf.Abs(worldPos2.y - worldPos1.y);

        //Debug.Log("CanvasInWorldWidth:" + CanvasInWorldWidth);
        //Debug.Log("CanvasInWorldHeight:" + CanvasInWorldHeight);

        CanvasInWorldHalfWidth = CanvasInWorldWidth / 2.0f;
        CanvasInWorldHalfHeight = CanvasInWorldHalfHeight / 2.0f;

    }


    public  void setPositon(float distx,float disty,float distz)
    {
        this.GetComponent<Transform>().position = new Vector3(distx, disty, distz); 
    }

    public void setRotation(float rotx,float roty,float rotz)
    {

        Quaternion qt = Quaternion.Euler(rotx, roty, rotz);
        this.GetComponent<Transform>().rotation = qt;
       
    }

    public void restoreCameraSet()
    {
        this.GetComponent<Transform>().localPosition = camPos;
        this.GetComponent<Transform>().localRotation = camRot;
        this.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        m_Camera.orthographic = false;

        calcCanVasInWorldSize();


    }

    /// <summary>
    /// 设定相机投影为正交投影
    /// </summary>
    public void setCameraToOrthographic() 
    {

        m_Camera.orthographic = true;
        m_Camera.orthographicSize = m_Camera.scaledPixelHeight / 200.0f;

        this.GetComponent<Transform>().localPosition = new Vector3(0, 0, camPos.z);
        Quaternion qt = Quaternion.Euler(0, 180, 0);
        this.GetComponent<Transform>().localRotation = qt;
        this.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);

        calcCanVasInWorldSize();
    }

    //设定相机背景颜色
    public void setCameraBgColor(float r,float g,float b)
    {
        if(m_Camera)
        {
            Color bgc = new Color(r,g,b);
            m_Camera.backgroundColor = bgc;
        }
    }


}
