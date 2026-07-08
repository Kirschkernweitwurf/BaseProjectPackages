namespace Base.CorePackage.EventBus
{
    /// <summary>
    /// Marker interface for every payload dispatched through the <see cref="IEventBus"/>.
    /// Implement on a <c>readonly struct</c> or immutable <c>record</c> to keep events
    /// allocation-free and side-effect-free.
    /// </summary>
    public interface IEvent { }
}