namespace Base.SettingsPackage.Presets
{
    /// <summary>
    /// The value types a <see cref="SettingsPresetEntry"/> can carry. Picks which of the entry's
    /// serialized values is the meaningful one and which setting type it is written to.
    /// </summary>
    public enum ESettingValueType : byte
    {
        /// <summary>Written to a <see cref="Core.BoolSetting"/>.</summary>
        Bool = 0,

        /// <summary>Written to a <see cref="Core.FloatSetting"/>.</summary>
        Float = 1,

        /// <summary>Written to an <see cref="Core.IntSetting"/>.</summary>
        Int = 2,

        /// <summary>Written to a <see cref="Core.StringSetting"/>.</summary>
        String = 3
    }
}