using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class LayerTexture : MonoBehaviour {
  private Image image;
  private RectTransform rectTransform;
  private Texture2D currentTexture;

  private VoxelLayer<Voxel> layer;
  public VoxelLayer<Voxel> Layer {
    get {
      return layer;
    }
    set {
      layer = value;
      calculateTexture();
    }
  }
  public float threshold = 0.5f;
  private bool outline = true;
  public bool Outline { 
    get {
      return outline;
    }
    set {
      outline = value;
      calculateTexture();
    } 
  }
  private bool drawColors = true;
  public bool DrawColors {
    get {
      return drawColors;
    }
    set {
      drawColors = value;
      calculateTexture();
    } 
  }

  private Action<Voxel> voxelSelectionDelegate = voxel => {};
  private bool mouseInside = false;

  private Vector2 highlightPosition = Vector2.zero;
  private Color previousColor;
  private float highlightBrightness = 0.5f;

  private int scaleFactor = 5;

  public void Start() {
    image = GetComponent<Image>();
    rectTransform = GetComponent<RectTransform>();
    currentTexture = new Texture2D(1, 1);
  }

  public void OnMouseEnter() {
    mouseInside = true;
  }

  public void OnMouseLeave() {
    mouseInside = false;
  }

  public void addVoxelSelectionDelegate(Action<Voxel> del) {
    voxelSelectionDelegate += del;
  }

  public void removeVoxelSelectionDelegate(Action<Voxel> del) {
    voxelSelectionDelegate -= del;
  }

  public void Update() {
    if (mouseInside && layer != null) {
      Vector2 mousePosition;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out mousePosition);
      mousePosition = new Vector2(rectTransform.rect.width + mousePosition.x, mousePosition.y);
      mousePosition = mousePosition / rectTransform.rect.width * layer.Width;
      unhighlightPixel(highlightPosition);
      highlightPosition = mousePosition;
      highlightPixel();
      redraw();
    }
  }

  private void highlightPixel() {
    int x = (int)highlightPosition.x;
    int y = (int)highlightPosition.y;
    Color color = GetPixel(x, y);
    previousColor = color;
    color += new Color(highlightBrightness, highlightBrightness, highlightBrightness);
    SetPixel(x, y, color);
    voxelSelectionDelegate(Layer[x, y]);
  }

  private void unhighlightPixel(Vector2 position) {
    int x = (int)highlightPosition.x;
    int y = (int)highlightPosition.y;
    SetPixel(x, y, previousColor);
  }

  private void redraw() {
    currentTexture.Apply();
    image.sprite = Sprite.Create(currentTexture, new Rect(0.0f, 0.0f, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f));
  }

  private void calculateTexture() {
    if (currentTexture.width != layer.Width*scaleFactor || currentTexture.height != layer.Length*scaleFactor) {
      currentTexture = new Texture2D(layer.Width*scaleFactor, layer.Length*scaleFactor);
    }

    Color[] pixels = Enumerable.Repeat(Color.white, layer.Width*layer.Length).ToArray();

    for(int x = 0; x < layer.Width; x++) {
      for(int y = 0; y < layer.Length; y++) {
	int idx = x + y * layer.Width;
	float brightness = layer[x,y].GetValue();
	if (Outline) {
	  if (y < layer.Length - 1) {
	    float brightnessBottom = layer[x,y+1].GetValue();
	    drawOutline(pixels, idx, brightness, idx+layer.Length, brightnessBottom, threshold);
	  }
	  if (x < layer.Width - 1) {
	    float brightnessRight = layer[x+1,y].GetValue();
	    drawOutline(pixels, idx, brightness, idx+1, brightnessRight, threshold);
	  }
	}

	if (DrawColors) {
	  if (brightness < 0.5)  {
	    pixels[idx] *= new Color(brightness, brightness, 1.5f*brightness);
	  } else {
	    pixels[idx] *= new Color(brightness, 1.5f*brightness, brightness);
	  }
	} else {
	  pixels[idx] *= new Color(brightness, brightness, brightness);
	}
      }
    }

    pixels = scalePixelArray(pixels);

    currentTexture.SetPixels(pixels);
    highlightPixel();
    redraw();
  }

  private void drawOutline(Color[] pixels, int idx1, float brightness1, int idx2, float brightness2, float threshold) {
    if (brightness1 >= threshold && brightness2 < threshold ||
	brightness1 < threshold && brightness2 >= threshold) {
      float interpolationFactor = (brightness1 - threshold)/(brightness1 - brightness2);
      if (brightness1 > brightness2) {
	interpolationFactor = 1 - interpolationFactor;
      }
      pixels[idx1] *= new Color(1.0f, interpolationFactor, interpolationFactor);
      pixels[idx2] *= new Color(1.0f, 1-interpolationFactor, 1-interpolationFactor);
    }
  }

  private Color[] scalePixelArray(Color[] pixel) {
    int currentTextureWidth = currentTexture.width;
    Color[] scaledArray = new Color[pixel.Length*scaleFactor*scaleFactor];
    int oldWidth = currentTextureWidth/scaleFactor;
    for(int i = 0; i < pixel.Length; i++) {
      int row = (i/oldWidth) * scaleFactor;
      int col = (i%oldWidth) * scaleFactor;
      for(int x = 0; x < scaleFactor; x++) {
	for(int y = 0; y < scaleFactor; y++) {
	  int idx = col + row*currentTextureWidth + x + y*currentTextureWidth;
	  scaledArray[idx] = pixel[i];
	}
      }
    }
    return scaledArray;
  }

  private void SetPixel(int x, int y, Color color) {
    for(int sx = 0; sx < scaleFactor; sx++) {
      for(int sy = 0; sy < scaleFactor; sy++) {
	currentTexture.SetPixel(x*scaleFactor + sx, y*scaleFactor + sy, color);
      }
    }
  }

  private Color GetPixel(int x, int y) {
    return currentTexture.GetPixel(x*scaleFactor, y*scaleFactor);
  }
}

