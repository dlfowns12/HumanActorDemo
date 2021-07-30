using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using UnityEngine.Video;


using System.IO;
public class BackGroundManager : MonoBehaviour
{

    private GameObject bgGo;
    private RawImage m_RawImg;

    private CanvasScaler  m_CansCalar;
    private RectTransform m_RctTrans;


    private VideoPlayer videoPlayer;



    //画布大小  客户端界面上显示区域的宽度 高度
    public int bgCanvasWidth;
    public int bgCanvasHeight;

    //canvasWidth  = m_Camera.scaledPixelWidth;
    //canvasHeight = m_Camera.scaledPixelHeight;


    //true:播放中,false:空闲中
    private bool videoState = false;
    RenderTexture targetTexture;

    public void Start()
    {
        m_RawImg = this.GetComponent<RawImage>();

        restoreBackGroundTexture();

        m_CansCalar = this.GetComponent<CanvasScaler>();
        m_CansCalar.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        videoPlayer = gameObject.GetComponent<VideoPlayer>();

       

       

     
    }


    public void Update()
    {
        

    }

    /// <summary>
    /// 恢复默认背景纹理
    /// </summary>
    public void restoreBackGroundTexture()
    {
        stopVideoPlay();
        Texture2D tex = Resources.Load<Texture2D>("Textures/background");
        m_RawImg.texture = tex;
    }

    public void setBackGroundTexture(Texture2D tex)
    {
        stopVideoPlay();
        m_RawImg.texture = tex;


    }

    private void stopVideoPlay()
    {

        if (videoState)
        {
            videoPlayer.Stop();

            if (targetTexture)
                targetTexture.Release();

            videoState = false;

        }

    }

    public void setBackGroundTextureFromVideo(string videoPath,int width,int height)
    {
        if (videoState)
            videoPlayer.Stop();
     
        videoPlayer.url = videoPath;
        videoPlayer.Play();


        if (targetTexture)
            targetTexture.Release();


        targetTexture = RenderTexture.GetTemporary(width, height, 16);
        videoPlayer.targetTexture = targetTexture;
        m_RawImg.texture = targetTexture;

       

        videoState = true;


    }
    

    public void saveBgTexture(string strFile)
    {

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);//读像素
        tex.Apply();

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);


        byte[] byt = tex.EncodeToPNG();

        File.WriteAllBytes(strFile, byt);   //保存到  安卓手机的  DCIM/下的Camera   文件夹下
        ScreenCapture.CaptureScreenshot(strFile+".png");



        return;

    }





}
