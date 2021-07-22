namespace Kebapi
{ 
    /// <summary>
    /// Indicates a Type with settings that require parsing.
    /// </summary>
    /// <typeparam name="T">The Type with settings to parse.</typeparam>
    interface IParsableSettings<T>
    {
        // Takes T (possibly with scones) and returns a parsed T (possibly well
        // satisfied.)
        T ParseSettings(T unparsed);
    }
}
