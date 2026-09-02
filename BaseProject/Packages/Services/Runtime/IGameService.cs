namespace Base.ServicesPackage
{
    /// <summary>
    /// Marker interface for game services that can be resolved through the <see cref="ServiceLocator"/>.
    /// It carries no members: it exists to constrain the locator's generics and to type its registry.
    /// </summary>
    /// <remarks>
    /// The interface does not register anything by itself. <see cref="GameServiceBehaviour"/> registers
    /// and deregisters in its Unity callbacks, so deriving from it is the usual route. Any other
    /// implementation has to do it itself, through <see cref="ServiceLocator.Register{T}(T)"/> and
    /// <see cref="ServiceLocator.Deregister{T}(T)"/>, or through the
    /// <see cref="ServiceLocator.Register(System.Type, IGameService)"/> overloads when the key is only
    /// known at runtime. Register under the type callers will ask for, which is the interface when the
    /// service is meant to be swappable.
    /// </remarks>
    public interface IGameService { }
}