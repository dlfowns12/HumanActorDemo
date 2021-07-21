using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
由于加载ab资源包，不能要求内部有同名的对象

如果想加载的ab包的内容不一样 但是名字一样的话

只能加载第一次加载的ab包的内容,所以 ab内的对象名称一定不能重复

否则 会出现问题.

GameObject.destroy()  之后，变量主动赋值为null;

*/

public class AnimControl
{
    private GameObject m_AvatarGo = null;


    Animation anim;
    AnimationClip amClip;


    private List<string>      m_AnimsList;
    private List<AssetBundle> m_AnimsAB;

    private string m_CurAnimName;



    public AnimControl()
    {
        m_CurAnimName = "";
    }


    public void initial(GameObject go)
    {

        m_AvatarGo = go;
        //判断go对象是否含有animation组件，如果没有，则创建动画组件 

        Animation anims = m_AvatarGo.GetComponent<Animation>();
        if (anims == null)
            anim = m_AvatarGo.AddComponent<Animation>();
        else
            anim = anims;


        m_AnimsList = new List<string>();
        m_AnimsAB = new List<AssetBundle>();


    }


    

    private bool animIsExist(List<string> animsCol,string animName)
    {
        for(int i=0;i<animsCol.Count;i++)
        {
            if (animsCol[i] == animName)
                return true;
        }
        return false;
    }

    public void playAnim(string strDir,string animfile,int isloop,float speed)
    {

        
        //step1:判断动画是否已经加载过
        if (!animIsExist(m_AnimsList, animfile))
        {
            m_AnimsList.Add(animfile);
            string strAnimFullFile = strDir + "/" + animfile;

            AssetBundle animAB = AssetBundle.LoadFromFile(strAnimFullFile);
            m_AnimsAB.Add(animAB);

            AnimationClip amClip = animAB.LoadAsset<AnimationClip>(animfile);
            if (amClip == null)
                Debug.Log("anim load empty!");
            else
                anim.AddClip(amClip,animfile);

        }

        //step2:动画设置 
        anim.playAutomatically = true;
        if (isloop == 0)
            anim.wrapMode = WrapMode.Once;
        else
            anim.wrapMode = WrapMode.Loop;

        anim[animfile].speed = 1.0f;
        if (speed>0.0f && speed <=1.0f)
            anim[animfile].speed = speed;

        anim.Play(animfile);

        m_CurAnimName = animfile;



    }

    public void stopAnim()
    {
        if (m_CurAnimName == "")
            return;

        

        AnimationState state = anim[m_CurAnimName];
        anim.Play(m_CurAnimName);
        state.time = 0;
        anim.Sample();
        state.enabled = false;
        m_CurAnimName = "";

      
    
    }





}
