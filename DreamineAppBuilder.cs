using Dreamine.MVVM.Core;
using Dreamine.MVVM.Core.Locators;
using Dreamine.MVVM.Interfaces.Navigation;
using Dreamine.MVVM.Interfaces.Windows;
using Dreamine.MVVM.Locators;
using Dreamine.MVVM.Locators.Wpf;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// \if KO
    /// <para>Dreamine WPF 애플리케이션 초기화를 담당합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Encapsulates dreamine app builder functionality and related state.</para>
    /// \endif
    /// </summary>
    public static class DreamineAppBuilder
    {
        /// <summary>
        /// \if KO
        /// <para>Sync Root 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the sync root value.</para>
        /// \endif
        /// </summary>
        private static readonly object SyncRoot = new();
        /// <summary>
        /// \if KO
        /// <para>global Auto Wire Handler Registered 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the global auto wire handler registered value.</para>
        /// \endif
        /// </summary>
        private static bool _globalAutoWireHandlerRegistered;
        /// <summary>
        /// \if KO
        /// <para>auto Navigator Handler Registered 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the auto navigator handler registered value.</para>
        /// \endif
        /// </summary>
        private static bool _autoNavigatorHandlerRegistered;

        /// <summary>
        /// \if KO
        /// <para>Dreamine MVVM WPF 런타임을 기본 옵션으로 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the initialize operation.</para>
        /// \endif
        /// </summary>
        /// <param name="rootAssembly">
        /// \if KO
        /// <para>View, ViewModel, Model, Event, Manager 등의 자동 등록 및 매핑 대상으로 사용할 루트 어셈블리입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Assembly"/> value used for root assembly.</para>
        /// \endif
        /// </param>
        public static void Initialize(Assembly rootAssembly)
        {
            Initialize(rootAssembly, DreamineWpfOptions.CreateDefault());
        }

        /// <summary>
        /// \if KO
        /// <para>Dreamine MVVM WPF 런타임을 지정한 옵션으로 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the initialize operation.</para>
        /// \endif
        /// </summary>
        /// <param name="rootAssembly">
        /// \if KO
        /// <para>View, ViewModel, Model, Event, Manager 등의 자동 등록 및 매핑 대상으로 사용할 루트 어셈블리입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Assembly"/> value used for root assembly.</para>
        /// \endif
        /// </param>
        /// <param name="options">
        /// \if KO
        /// <para>WPF 런타임 초기화 옵션입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The options that configure the operation.</para>
        /// \endif
        /// </param>
        public static void Initialize(Assembly rootAssembly, DreamineWpfOptions options)
        {
            ArgumentNullException.ThrowIfNull(rootAssembly);
            ArgumentNullException.ThrowIfNull(options);

            if (options.RegisterDefaultServices)
            {
                RegisterDefaultServices(options);
            }

            RegisterDefaultViewModelResolver();

            // ① DI 자동 등록
            DreamineAutoRegistrar.RegisterAll(rootAssembly);

            // ② View ↔ ViewModel 자동 매핑
            ViewModelLocator.RegisterAll(rootAssembly);

            // ③ 선택적 Loaded 기반 ViewModel 자동 주입
            if (options.EnableGlobalAutoWireOnLoaded)
            {
                RegisterGlobalAutoWireHandlerOnce();
            }

            // ④ 선택적 Region 기반 INavigator 자동 등록
            if (options.EnableAutoNavigatorRegistration)
            {
                RegisterAutoNavigatorHandlerOnce(options.DefaultRegionName);
            }
        }

        /// <summary>
        /// \if KO
        /// <para>Register Default View Model Resolver 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Registers the default ViewModel resolver used by ViewModelLocator.</para>
        /// \endif
        /// </summary>
        private static void RegisterDefaultViewModelResolver()
        {
            ViewModelLocator.RegisterResolver(
                new DreamineContainerViewModelResolver());
        }

        /// <summary>
        /// \if KO
        /// <para>Dreamine WPF 기본 서비스를 등록합니다. 이미 등록된 서비스는 덮어쓰지 않습니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the register default services operation.</para>
        /// \endif
        /// </summary>
        /// <param name="options">
        /// \if KO
        /// <para>동작을 구성하는 설정입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The options that configure the operation.</para>
        /// \endif
        /// </param>
        /// <remarks>
        /// \if KO
        /// <para><b>MS.Extensions.DI 혼용 시 주의사항 (Fix 7)</b> Hybrid(Blazor+WPF) 시나리오에서 <c>IServiceCollection.AddDreamineHybridWpf()</c>를 사용하면 Blazor의 <c>IServiceProvider</c>와 Dreamine의 <c>DMContainer</c>가 동시에 활성화됩니다. 이중 등록으로 인한 혼란을 방지하려면: <list type="number"> <item>App.xaml.cs 의 Composition Root에서 <c>DreamineAppBuilder.Initialize()</c>를 먼저 호출하여 DMContainer를 구성한 후, <c>IServiceCollection</c>에 <c>services.AddSingleton&lt;IViewManager&gt;(sp =&gt; DMContainer.Resolve&lt;IViewManager&gt;())</c> 형태로 브리지 등록하십시오.</item> <item>단일 서비스를 두 컨테이너 모두에 직접 등록하지 마십시오. 어느 한쪽에만 등록하고 다른 쪽은 팩토리 위임으로 연결하는 것이 안전합니다.</item> <item>장기적으로는 <c>DMContainer.SetContainer(new MsExtensionsAdapter(serviceProvider))</c> 패턴으로 DMContainer가 MS.Extensions.DI를 백엔드로 사용하도록 통합할 수 있습니다.</item> </list></para>
        /// \endif
        /// \if EN
        /// <para>Describes behavior and usage considerations for this member.</para>
        /// \endif
        /// </remarks>
        public static void RegisterDefaultServices(DreamineWpfOptions? options = null)
        {
            if (!DMContainer.IsRegistered<IWindowStateService>())
            {
                DMContainer.RegisterSingleton<IWindowStateService, WindowStateService>();
            }

            if (!DMContainer.IsRegistered<IViewManager>())
            {
                var vm = options is not null
                    ? new ViewManager(DMContainer.GetResolver(),
                        options.FallbackWindowWidth,
                        options.FallbackWindowHeight)
                    : new ViewManager();
                DMContainer.RegisterSingleton<IViewManager>(vm);
            }

            if (!DMContainer.IsRegistered<IDialogService>())
            {
                DMContainer.RegisterSingleton<IDialogService, WpfDialogService>();
            }
        }

        /// <summary>
        /// \if KO
        /// <para>WPF FrameworkElement Loaded 이벤트의 전역 ViewModel 자동 주입 핸들러를 한 번만 등록합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the register global auto wire handler once operation.</para>
        /// \endif
        /// </summary>
        public static void RegisterGlobalAutoWireHandlerOnce()
        {
            lock (SyncRoot)
            {
                if (_globalAutoWireHandlerRegistered)
                {
                    return;
                }

                EventManager.RegisterClassHandler(
                    typeof(FrameworkElement),
                    FrameworkElement.LoadedEvent,
                    new RoutedEventHandler(AttachViewModelIfExists));

                _globalAutoWireHandlerRegistered = true;
            }
        }

        /// <summary>
        /// \if KO
        /// <para>Window Loaded 시점에 지정된 RegionName을 찾아 INavigator를 자동 등록하는 핸들러를 한 번만 등록합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the register auto navigator handler once operation.</para>
        /// \endif
        /// </summary>
        /// <param name="defaultRegionName">
        /// \if KO
        /// <para>기본 Region 이름입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for default region name.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentException">
        /// \if KO
        /// <para>입력 인자가 유효하지 않은 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when an input argument is invalid.</para>
        /// \endif
        /// </exception>
        public static void RegisterAutoNavigatorHandlerOnce(string defaultRegionName)
        {
            if (string.IsNullOrWhiteSpace(defaultRegionName))
            {
                throw new ArgumentException("Default region name must not be empty.", nameof(defaultRegionName));
            }

            lock (SyncRoot)
            {
                if (_autoNavigatorHandlerRegistered)
                {
                    return;
                }

                EventManager.RegisterClassHandler(
                    typeof(Window),
                    FrameworkElement.LoadedEvent,
                    new RoutedEventHandler((sender, _) =>
                    {
                        if (sender is Window window)
                        {
                            RegisterNavigatorFromWindow(window, defaultRegionName);
                        }
                    }));

                _autoNavigatorHandlerRegistered = true;
            }
        }

        /// <summary>
        /// \if KO
        /// <para>지정한 Window 내부에서 RegionName이 일치하는 ContentControl을 찾아 INavigator를 등록합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the register navigator from window operation.</para>
        /// \endif
        /// </summary>
        /// <param name="window">
        /// \if KO
        /// <para>Region을 찾을 대상 Window입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Window"/> value used for window.</para>
        /// \endif
        /// </param>
        /// <param name="regionName">
        /// \if KO
        /// <para>Region 이름입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for region name.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentException">
        /// \if KO
        /// <para>입력 인자가 유효하지 않은 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when an input argument is invalid.</para>
        /// \endif
        /// </exception>
        public static void RegisterNavigatorFromWindow(Window window, string regionName)
        {
            ArgumentNullException.ThrowIfNull(window);

            if (string.IsNullOrWhiteSpace(regionName))
            {
                throw new ArgumentException("Region name must not be empty.", nameof(regionName));
            }

            ContentControl? region = RegionBinderHelper.FindRegionControl(window, regionName);
            if (region is null)
            {
                return;
            }

            DMContainer.RegisterSingleton<INavigator>(
                new ContentControlNavigator(region));
        }

        /// <summary>
        /// \if KO
        /// <para>WPF FrameworkElement의 Loaded 이벤트에서 호출되는 핸들러입니다. View의 DataContext가 비어 있을 경우, ViewModelLocator를 통해 ViewModel을 자동 연결합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Attaches the behavior to a target object.</para>
        /// \endif
        /// </summary>
        /// <param name="sender">
        /// \if KO
        /// <para>이벤트가 발생한 View 인스턴스입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The object that raised the event.</para>
        /// \endif
        /// </param>
        /// <param name="e">
        /// \if KO
        /// <para>라우팅 이벤트 인자입니다.</para>
        /// \endif
        /// \if EN
        /// <para>Contains data associated with the event.</para>
        /// \endif
        /// </param>
        private static void AttachViewModelIfExists(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement view)
            {
                return;
            }

            if (view.DataContext is not null)
            {
                return;
            }

            if (view is not Window && view is not UserControl && view is not Page)
            {
                return;
            }

            object? viewModel = ViewModelLocator.Resolve(view.GetType());
            if (viewModel is not null)
            {
                view.DataContext = viewModel;
            }
        }
    }
}
