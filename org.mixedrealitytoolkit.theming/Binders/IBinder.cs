// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Interface for a theme binder that subscribes to a <see cref="ThemeDataSource"/> and applies theme data to a target.
    /// </summary>
    public interface IBinder
    {
        /// <summary>
        /// Subscribes to the given theme data source for theme changes.
        /// </summary>
        /// <param name="themeDataSource">The theme data source to subscribe to.</param>
        void Subscribe(ThemeDataSource themeDataSource);

        /// <summary>
        /// Unsubscribes from the given theme data source.
        /// </summary>
        /// <param name="themeDataSource">The theme data source to unsubscribe from.</param>
        void Unsubscribe(ThemeDataSource themeDataSource);

        /// <summary>
        /// The name of the theme definition item this binder targets.
        /// </summary>
        string ThemeDefinitionItemName { get; }
    }
}
