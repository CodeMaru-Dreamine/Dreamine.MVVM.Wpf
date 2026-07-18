using System;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// \if KO
    /// <para>Dreamine Wpf Options 기능과 관련 상태를 캡슐화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Provides configuration options for the Dreamine WPF runtime bootstrap process.</para>
    /// \endif
    /// </summary>
    public sealed class DreamineWpfOptions
    {
        /// <summary>
        /// \if KO
        /// <para>Enable Global Auto Wire On Loaded 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets a value indicating whether every FrameworkElement Loaded event should try ViewModel auto-wiring.</para>
        /// \endif
        /// </summary>
        public bool EnableGlobalAutoWireOnLoaded { get; set; }

        /// <summary>
        /// \if KO
        /// <para>Register Default Services 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets a value indicating whether Dreamine default WPF services should be registered automatically.</para>
        /// \endif
        /// </summary>
        public bool RegisterDefaultServices { get; set; }

        /// <summary>
        /// \if KO
        /// <para>Enable Auto Navigator Registration 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets a value indicating whether Dreamine should automatically register an INavigator from the default region.</para>
        /// \endif
        /// </summary>
        public bool EnableAutoNavigatorRegistration { get; set; }

        /// <summary>
        /// \if KO
        /// <para>Default Region Name 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets the default region name used for automatic navigator registration.</para>
        /// \endif
        /// </summary>
        public string DefaultRegionName { get; set; } = "SubPage";

        /// <summary>
        /// \if KO
        /// <para>Fallback Window Width 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets the fallback window width used when displaying a UserControl or Page without a region navigator. Defaults to 800.</para>
        /// \endif
        /// </summary>
        public double FallbackWindowWidth { get; set; } = 800;

        /// <summary>
        /// \if KO
        /// <para>Fallback Window Height 값을 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets the fallback window height used when displaying a UserControl or Page without a region navigator. Defaults to 600.</para>
        /// \endif
        /// </summary>
        public double FallbackWindowHeight { get; set; } = 600;

        /// <summary>
        /// \if KO
        /// <para>Default 값을 생성합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the default Dreamine WPF options.</para>
        /// \endif
        /// </summary>
        /// <returns>
        /// \if KO
        /// <para>Create Default 작업에서 생성한 <see cref="DreamineWpfOptions"/> 결과입니다.</para>
        /// \endif
        /// \if EN
        /// <para>A new options instance with safe defaults.</para>
        /// \endif
        /// </returns>
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