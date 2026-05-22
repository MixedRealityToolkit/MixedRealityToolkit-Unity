// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Theming
{
    public interface IBinder
    {
        void Subscribe(ThemeDataSource themeDataSource);
        void Unsubscribe(ThemeDataSource themeDataSource);

        string ThemeDefinitionItemName { get; }
    }
}
