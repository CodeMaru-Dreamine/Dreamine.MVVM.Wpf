using System;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// Provides configuration options for the Dreamine WPF runtime bootstrap process.
    /// </summary>
    public sealed class DreamineWpfOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether every FrameworkElement Loaded event should try ViewModel auto-wiring.
        /// </summary>
        public bool EnableGlobalAutoWireOnLoaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Dreamine default WPF services should be registered automatically.
        /// </summary>
        public bool RegisterDefaultServices { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Dreamine should automatically register an INavigator from the default region.
        /// </summary>
        public bool EnableAutoNavigatorRegistration { get; set; }

        /// <summary>
        /// Gets or sets the default region name used for automatic navigator registration.
        /// </summary>
        public string DefaultRegionName { get; set; } = "SubPage";

        /// <summary>
        /// Gets the default Dreamine WPF options.
        /// </summary>
        /// <returns>A new options instance with safe defaults.</returns>
        public static DreamineWpfOptions CreateDefault()
        {
            return new DreamineWpfOptions
            {
                EnableGlobalAutoWireOnLoaded = true,
                RegisterDefaultServices = true,
                EnableAutoNavigatorRegistration = true,
                DefaultRegionName = "SubPage"
            };
        }
    }
}