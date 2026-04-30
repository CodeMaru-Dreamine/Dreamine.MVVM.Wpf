using Dreamine.MVVM.Core;
using Dreamine.MVVM.Extensions;
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

            // ① DI 자동 등록
            DMContainer.AutoRegisterAll(rootAssembly);

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
        /// Dreamine WPF 기본 서비스를 등록합니다.
        /// 이미 등록된 서비스는 덮어쓰지 않습니다.
        /// </summary>
        public static void RegisterDefaultServices()
        {
            if (!DMContainer.IsRegistered<IWindowStateService>())
            {
                DMContainer.RegisterSingleton<IWindowStateService>(new WindowStateService());
            }

            if (!DMContainer.IsRegistered<IViewManager>())
            {
                DMContainer.RegisterSingleton<IViewManager>(new ViewManager());
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

            if (DMContainer.IsRegistered<INavigator>())
            {
                return;
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