namespace Tot;

public struct PakedFile
{
    public string pakName;
    public string path;
    public string sha;
    public long size;

    public override string ToString()
    {
        return $"{sha} - {path}";
    }
}