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
    /// Dreamine WPF 애플리케이션 초기화를 담당합니다.
    /// </summary>
    public static class DreamineAppBuilder
    {
        private static readonly object SyncRoot = new();
        private static bool _globalAutoWireHandlerRegistered;
        private static bool _autoNavigatorHandlerRegistered;

        /// <summary>
        /// Dreamine MVVM WPF 런타임을 기본 옵션으로 초기화합니다.
        /// </summary>
        /// <param name="rootAssembly">
        /// View, ViewModel, Model, Event, Manager 등의 자동 등록 및 매핑 대상으로 사용할 루트 어셈블리입니다.
        /// </param>
        public static void Initialize(Assembly rootAssembly)
        {
            Initialize(rootAssembly, DreamineWpfOptions.CreateDefault());
        }

        /// <summary>
        /// Dreamine MVVM WPF 런타임을 지정한 옵션으로 초기화합니다.
        /// </summary>
        /// <param name="rootAssembly">
        /// View, ViewModel, Model, Event, Manager 등의 자동 등록 및 매핑 대상으로 사용할 루트 어셈블리입니다.
        /// </param>
        /// <param name="options">WPF 런타임 초기화 옵션입니다.</param>
        public static void Initialize(Assembly rootAssembly, DreamineWpfOptions options)
        {
            ArgumentNullException.ThrowIfNull(rootAssembly);
            ArgumentNullException.ThrowIfNull(options);

            if (options.RegisterDefaultServices)
            {
                RegisterDefaultServices();
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
        /// Registers the default ViewModel resolver used by ViewModelLocator.
        /// </summary>
        private static void RegisterDefaultViewModelResolver()
        {
            ViewModelLocator.RegisterResolver(
                new DreamineContainerViewModelResolver());
        }

        /// <summary>
        /// Dreamine WPF 기본 서비스를 등록합니다.
        /// 이미 등록된 서비스는 덮어쓰지 않습니다.
        /// </summary>
        /// <remarks>
        /// <para><b>MS.Extensions.DI 혼용 시 주의사항 (Fix 7)</b></para>
        /// <para>
        /// Hybrid(Blazor+WPF) 시나리오에서 <c>IServiceCollection.AddDreamineHybridWpf()</c>를
        /// 사용하면 Blazor의 <c>IServiceProvider</c>와 Dreamine의 <c>DMContainer</c>가
        /// 동시에 활성화됩니다. 이중 등록으로 인한 혼란을 방지하려면:
        /// <list type="number">
        ///   <item>App.xaml.cs 의 Composition Root에서 <c>DreamineAppBuilder.Initialize()</c>를
        ///   먼저 호출하여 DMContainer를 구성한 후, <c>IServiceCollection</c>에
        ///   <c>services.AddSingleton&lt;IViewManager&gt;(sp =&gt; DMContainer.Resolve&lt;IViewManager&gt;())</c>
        ///   형태로 브리지 등록하십시오.</item>
        ///   <item>단일 서비스를 두 컨테이너 모두에 직접 등록하지 마십시오.
        ///   어느 한쪽에만 등록하고 다른 쪽은 팩토리 위임으로 연결하는 것이 안전합니다.</item>
        ///   <item>장기적으로는 <c>DMContainer.SetContainer(new MsExtensionsAdapter(serviceProvider))</c>
        ///   패턴으로 DMContainer가 MS.Extensions.DI를 백엔드로 사용하도록 통합할 수 있습니다.</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static void RegisterDefaultServices()
        {
            if (!DMContainer.IsRegistered<IWindowStateService>())
            {
                DMContainer.RegisterSingleton<IWindowStateService, WindowStateService>();
            }

            if (!DMContainer.IsRegistered<IViewManager>())
            {
                DMContainer.RegisterSingleton<IViewManager, ViewManager>();
            }

            if (!DMContainer.IsRegistered<IDialogService>())
            {
                DMContainer.RegisterSingleton<IDialogService, WpfDialogService>();
            }
        }

        /// <summary>
        /// WPF FrameworkElement Loaded 이벤트의 전역 ViewModel 자동 주입 핸들러를 한 번만 등록합니다.
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
        /// Window Loaded 시점에 지정된 RegionName을 찾아 INavigator를 자동 등록하는 핸들러를 한 번만 등록합니다.
        /// </summary>
        /// <param name="defaultRegionName">기본 Region 이름입니다.</param>
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
        /// 지정한 Window 내부에서 RegionName이 일치하는 ContentControl을 찾아 INavigator를 등록합니다.
        /// </summary>
        /// <param name="window">Region을 찾을 대상 Window입니다.</param>
        /// <param name="regionName">Region 이름입니다.</param>
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
        /// WPF FrameworkElement의 Loaded 이벤트에서 호출되는 핸들러입니다.
        /// View의 DataContext가 비어 있을 경우, ViewModelLocator를 통해 ViewModel을 자동 연결합니다.
        /// </summary>
        /// <param name="sender">이벤트가 발생한 View 인스턴스입니다.</param>
        /// <param name="e">라우팅 이벤트 인자입니다.</param>
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
