namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// The condition of one subscriber in an event bus invocation list, as the window reports it.
    /// </summary>
    internal enum EHandlerState : byte
    {
        /// <summary>The Unity object the handler runs on was destroyed, so the subscription leaked.</summary>
        Destroyed = 0,

        /// <summary>The handler runs on a Unity object that is still alive.</summary>
        Live = 1,

        /// <summary>The handler runs on a plain C# object, whose lifetime Unity does not manage.</summary>
        Plain = 2,

        /// <summary>The handler is a static method, so it has no instance to outlive.</summary>
        Static = 3
    }
}