public class Voxel {
  public float Data { get; set; }

  public virtual float GetValue() {
    return Data;
  }

  public Voxel() {
  }

  public virtual Voxel Clone() {
    Voxel v = new Voxel();
    v.Data = this.Data;
    return v;
  }
}
