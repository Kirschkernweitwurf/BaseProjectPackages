namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Supported languages. Numeric values are persisted, so keep existing values stable when adding
    /// new entries (append rather than reorder).
    /// </summary>
    public enum ELanguage // TODO: See if refactor possible
    {
        English = 0,
        German = 1,
        French = 2,
        Spanish = 3
    }
}