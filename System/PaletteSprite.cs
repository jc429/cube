using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PaletteIndex{
	Black,
	Red,
	Green,
	Blue
}

public class PaletteSprite : MonoBehaviour
{
	SpriteRenderer _spriteRenderer{
		get { return GetComponent<SpriteRenderer>(); }
	}
	Texture2D paletteTex;
	Color[] mSpriteColors;

	const float flashDuration = 0.1f;
	protected Timer flashTimer = new Timer();

    // Start is called before the first frame update
    void Awake() {
		InitPaletteTex();
		InitializeFlashTimer();
    }

    // Update is called once per frame
    void Update() {
		UpdateFlashTimer();
        
    }

	public void InitPaletteTex(){
		Texture2D colorSwapTex;
		colorSwapTex = (Texture2D)_spriteRenderer.material.GetTexture("_SwapTex");
		if(colorSwapTex == null){
			colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
			colorSwapTex.filterMode = FilterMode.Point;
			/*
			*/	
			_spriteRenderer.material.SetTexture("_SwapTex", colorSwapTex);
		}
	
	
		mSpriteColors = new Color[colorSwapTex.width];
		paletteTex = colorSwapTex;
	}

	public void SetPalette(PaletteIndex index){
		Texture2D colorSwapTex;
		colorSwapTex = (Texture2D)_spriteRenderer.material.GetTexture("_SwapTex");
		for (int i = 0; i < colorSwapTex.width; ++i){
			Color c = colorSwapTex.GetPixel(i,(8 * (int)index) + 4);
			colorSwapTex.SetPixel(i, 0, c);
		}
		colorSwapTex.Apply();
	}

	public void SwapColor(int index, Color color){
		mSpriteColors[index] = color;
		paletteTex.SetPixel(index, 0, color);
	}

	void FlashColor(Color color){
		for (int i = 0; i < paletteTex.width; ++i){
			mSpriteColors[i] = paletteTex.GetPixel(i,0);
			paletteTex.SetPixel(i, 0, color);
		}
		paletteTex.Apply();
	}

	public void ResetColors(){
		for (int i = 0; i < paletteTex.width; ++i){
			paletteTex.SetPixel(i, 0, mSpriteColors[i]);
		}
		paletteTex.Apply();
	}

	

	protected void InitializeFlashTimer(){
		flashTimer = new Timer();
		flashTimer.duration = flashDuration;
	}

	protected void UpdateFlashTimer(){
		if(flashTimer.IsActive){
			flashTimer.AdvanceTimer(Time.deltaTime);
			float f = Mathf.Lerp(1.0f,0.0f,flashTimer.CompletionPercentage);
			_spriteRenderer.material.SetFloat("_FlashAmount", f);
			if(flashTimer.IsFinished){
				FinishFlash();
				flashTimer.Reset();
			}
		}
	}
	
	public virtual void StartFlash(){
		if(_spriteRenderer == null){
			Debug.Log("No Sprite Renderer :(");
			return;
		}
		_spriteRenderer.material.SetFloat("_FlashAmount", 1);
		flashTimer.Reset();
		flashTimer.SetActive(true);
	}
	
	public virtual void FinishFlash(){
	}
}
