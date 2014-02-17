namespace Griffin
{
    /// <summary>
    /// Delegate returned by <see cref="ConstructorExtensions.CreateFactory"/>.
    /// </summary>
    /// <param name="args">Constructor arguments</param>
    /// <returns>Created object</returns>
    public delegate object InstanceFactory(params object[] args);
}