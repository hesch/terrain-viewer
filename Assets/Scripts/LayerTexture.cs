using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LayerTexture : MonoBehaviour {
  private Image image;

  private VoxelLayer<Voxel> layer;
  public VoxelLayer<Voxel> Layer {
    get {
      return layer;
    }
    set {
      layer = value;
      calculateTexture(layer);
    }
  }
  public float threshold = 0.5f;
  public bool outline = true;
  public bool drawColors = true;

  public void Start() {
    image = GetComponent<Image>();
  }

  private void calculateTexture(VoxelLayer<Voxel> layer) {
    Texture2D texture = new Texture2D(layer.Width, layer.Length);

    Color[] pixels = Enumerable.Repeat(Color.white, layer.Width*layer.Length).ToArray();

    for(int x = 0; x < layer.Width; x++) {
      for(int y = 0; y < layer.Length; y++) {
	int idx = x + y * layer.Width;
	float brightness = layer[x,y].GetValue();
	if (outline) {
	  if (y < layer.Length - 1) {
	    float brightnessBottom = layer[x,y+1].GetValue();
	    drawOutline(pixels, idx, brightness, idx+layer.Length, brightnessBottom, threshold);
	  }
	  if (x < layer.Width - 1) {
	    float brightnessRight = layer[x+1,y].GetValue();
	    drawOutline(pixels, idx, brightness, idx+1, brightnessRight, threshold);
	  }
	}

	if (drawColors) {
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

    texture.SetPixels(pixels);
    texture.Apply();

    image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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
}

