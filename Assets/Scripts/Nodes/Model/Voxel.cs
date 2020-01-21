public class Voxel
{
    public float Data;

    public virtual float GetValue()
    {
        return Data;
    }

    public Voxel()
    {
    }

    public virtual Voxel Clone()
    {
        Voxel v = new Voxel();
        v.Data = this.Data;
        return v;
    }
}
