using UnityEngine;

public class TextureHelper
{

	public static Texture2D scale(Texture2D src, float ratio)
	{
		int newWidth = (int)(src.width * ratio);
		int newHeight = (int)(src.height * ratio);
		src.filterMode = FilterMode.Point;
		RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
		rt.filterMode = FilterMode.Point;
		var old = RenderTexture.active;
		RenderTexture.active = rt;
		Graphics.Blit(src, rt);
		Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
		nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
		nTex.Apply();
		RenderTexture.active = old;
		RenderTexture.ReleaseTemporary(rt);
		return nTex;
	}

	public static Texture2D scale(Texture2D src, int width, int height)
	{
		src.filterMode = FilterMode.Point;
		RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		rt.filterMode = FilterMode.Point;
		rt.wrapMode = TextureWrapMode.Clamp;
		var old = RenderTexture.active;
		RenderTexture.active = rt;
		Graphics.Blit(src, rt);
		Texture2D nTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		nTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		nTex.Apply();
		RenderTexture.active = old;
		RenderTexture.ReleaseTemporary(rt);
		return nTex;
	}


	public static Texture2D rotated(Texture2D src, FilterMode mode = FilterMode.Trilinear)
	{
		Rect texR = new Rect(0, 0, src.width, src.height);
		gpu_rotate(src, mode);

		//Get rendered data back to a new texture
		Texture2D result = new Texture2D(src.width, src.height, TextureFormat.RGBA32, true);
		result.ReadPixels(texR, 0, 0, true);
		return result;
	}

	/// <summary>
	/// Scales the texture data of the given texture.
	/// </summary>
	/// <param name="tex">Texure to scale</param>
	/// <param name="width">New width</param>
	/// <param name="height">New height</param>
	/// <param name="mode">Filtering mode</param>
	public static void rotate(Texture2D tex, FilterMode mode = FilterMode.Trilinear)
	{
		Rect texR = new Rect(0, 0, tex.width, tex.height);
		gpu_rotate(tex, mode);

		// Update new texture
		tex.Resize(tex.width, tex.height);
		tex.ReadPixels(texR, 0, 0, true);
		tex.Apply(true);    //Remove this if you hate us applying textures for you :)
	}

	// Internal unility that renders the source texture into the RTT - the scaling method itself.
	static void gpu_rotate(Texture2D src, FilterMode fmode)
	{
		//We need the source texture in VRAM because we render with it
		src.filterMode = fmode;
		src.Apply(true);

		//Using RTT for best quality and performance. Thanks, Unity 5
		RenderTexture rtt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
		rtt.filterMode = fmode;

		//Set the RTT in order to render to it
		Graphics.SetRenderTarget(rtt);

		//Setup 2D matrix in range 0..1, so nobody needs to care about sized
		GL.LoadPixelMatrix(0, 1, 0, 1);

		//Then clear & draw the texture to fill the entire RTT.
		GL.Clear(true, true, new Color(0, 0, 0, 0));
		Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
		RenderTexture.ReleaseTemporary(rtt);
	}

}