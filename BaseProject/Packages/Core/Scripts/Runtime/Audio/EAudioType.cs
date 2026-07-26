namespace Base.CorePackage.Audio
{
    /// <summary>
    /// Enumeration for different audio types. The values are serialized, so existing entries keep their number
    /// and a new entry gets the next free one.
    /// </summary>
    public enum EAudioType : byte
    {
        Sfx2D = 0,
        Sfx3D = 1,
        Music = 2,
        Ui = 3
    }
}
