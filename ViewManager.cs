using Dreamine.MVVM.Core;
using Dreamine.MVVM.Interfaces.DependencyInjection;
using Dreamine.MVVM.Interfaces.Navigation;
using Dreamine.MVVM.Interfaces.Windows;
using Dreamine.MVVM.Locators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// \if KO
    /// <para>View Manager 기능과 관련 상태를 캡슐화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resolves and displays WPF Views based on ViewModel types. Custom view types are supported by registering <see cref="IViewDisplayStrategy"/> implementations via <see cref="RegisterDisplayStrategy"/>.</para>
    /// \endif
    /// </summary>
    public sealed class ViewManager : IViewManager
    {
        /// <summary>
        /// \if KO
        /// <para>resolver 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the resolver value.</para>
        /// \endif
        /// </summary>
        private readonly IServiceResolver _resolver;
        /// <summary>
        /// \if KO
        /// <para>custom Strategies 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the custom strategies value.</para>
        /// \endif
        /// </summary>
        private readonly List<IViewDisplayStrategy> _customStrategies = new();
        /// <summary>
        /// \if KO
        /// <para>fallback Window Width 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the fallback window width value.</para>
        /// \endif
        /// </summary>
        private readonly double _fallbackWindowWidth;
        /// <summary>
        /// \if KO
        /// <para>fallback Window Height 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the fallback window height value.</para>
        /// \endif
        /// </summary>
        private readonly double _fallbackWindowHeight;

        /// <summary>
        /// \if KO
        /// <para>지정한 설정으로 <see cref="ViewManager"/> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of <see cref="ViewManager"/> with an explicit resolver and optional fallback window dimensions for UserControl/Page views. Prefer this constructor in tests and production code to avoid the global DMContainer dependency.</para>
        /// \endif
        /// </summary>
        /// <param name="resolver">
        /// \if KO
        /// <para>resolver에 사용할 <see cref="IServiceResolver"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The service resolver used to obtain ViewModel instances.</para>
        /// \endif
        /// </param>
        /// <param name="fallbackWindowWidth">
        /// \if KO
        /// <para>fallback Window Width에 사용할 <see cref="double"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>Width of the fallback window. Defaults to 800.</para>
        /// \endif
        /// </param>
        /// <param name="fallbackWindowHeight">
        /// \if KO
        /// <para>fallback Window Height에 사용할 <see cref="double"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>Height of the fallback window. Defaults to 600.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para>필수 입력 인자 중 하나가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when a required input argument is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public ViewManager(IServiceResolver resolver, double fallbackWindowWidth = 800, double fallbackWindowHeight = 600)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _fallbackWindowWidth = fallbackWindowWidth > 0 ? fallbackWindowWidth : 800;
            _fallbackWindowHeight = fallbackWindowHeight > 0 ? fallbackWindowHeight : 600;
        }

        /// <summary>
        /// \if KO
        /// <para>지정한 설정으로 <see cref="ViewManager"/> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of <see cref="ViewManager"/> backed by the global DMContainer.</para>
        /// \endif
        /// </summary>
        public ViewManager() : this(DMContainer.GetResolver())
        {
        }

        /// <summary>
        /// \if KO
        /// <para>Register Display Strategy 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Registers a custom display strategy that handles view types not natively supported (Window, UserControl, Page). Strategies are evaluated in registration order before the built-in switch, so registered strategies take precedence.</para>
        /// \endif
        /// </summary>
        /// <param name="strategy">
        /// \if KO
        /// <para>strategy에 사용할 <see cref="IViewDisplayStrategy"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The strategy to register.</para>
        /// \endif
        /// </param>
        public void RegisterDisplayStrategy(IViewDisplayStrategy strategy)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            _customStrategies.Add(strategy);
        }

        /// <summary>
        /// \if KO
        /// <para>Show 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show operation.</para>
        /// \endif
        /// </summary>
        /// <typeparam name="TViewModel">
        /// \if KO
        /// <para>TViewModel 형식 매개변수입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The TViewModel type parameter.</para>
        /// \endif
        /// </typeparam>
        public void Show<TViewModel>() where TViewModel : class
        {
            Show(typeof(TViewModel));
        }

        /// <summary>
        /// \if KO
        /// <para>Show 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show operation.</para>
        /// \endif
        /// </summary>
        /// <param name="viewModelType">
        /// \if KO
        /// <para>view Model Type에 사용할 <see cref="Type"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Type"/> value used for view model type.</para>
        /// \endif
        /// </param>
        public void Show(Type viewModelType)
        {
            ArgumentNullException.ThrowIfNull(viewModelType);

            object viewModel = _resolver.Resolve(viewModelType);
            object? view = ViewModelLocator.ResolveView(viewModelType);
            DisplayResolvedView(view, viewModel, viewModelType, useRegionNavigator: true);
        }

        /// <summary>
        /// \if KO
        /// <para>Navigate 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the navigate operation.</para>
        /// \endif
        /// </summary>
        /// <param name="viewModel">
        /// \if KO
        /// <para>view Model에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view model.</para>
        /// \endif
        /// </param>
        public void Navigate(object viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            Type viewModelType = viewModel.GetType();
            object? view = ViewModelLocator.ResolveView(viewModelType);
            DisplayResolvedView(view, viewModel, viewModelType, useRegionNavigator: false);
        }

        /// <summary>
        /// \if KO
        /// <para>Display Resolved View 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the display resolved view operation.</para>
        /// \endif
        /// </summary>
        /// <param name="view">
        /// \if KO
        /// <para>view에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view.</para>
        /// \endif
        /// </param>
        /// <param name="viewModel">
        /// \if KO
        /// <para>view Model에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view model.</para>
        /// \endif
        /// </param>
        /// <param name="viewModelType">
        /// \if KO
        /// <para>view Model Type에 사용할 <see cref="Type"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Type"/> value used for view model type.</para>
        /// \endif
        /// </param>
        /// <param name="useRegionNavigator">
        /// \if KO
        /// <para>use Region Navigator에 사용할 <see cref="bool"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="bool"/> value used for use region navigator.</para>
        /// \endif
        /// </param>
        private void DisplayResolvedView(
            object? view,
            object viewModel,
            Type viewModelType,
            bool useRegionNavigator)
        {
            if (view is null)
            {
                Debug.WriteLine(
                    $"[ViewManager] No view found for {viewModelType.FullName}. " +
                    "Ensure the view is registered via ViewModelLocator.Register<TView, TViewModel>().");
                return;
            }

            // Custom strategies take precedence — checked before built-in WPF types.
            foreach (var strategy in _customStrategies)
            {
                if (strategy.CanHandle(view))
                {
                    strategy.Display(view, viewModel, viewModelType, useRegionNavigator);
                    return;
                }
            }

            // Built-in WPF type handling.
            switch (view)
            {
                case Window window:
                    ShowWindow(window, viewModel, viewModelType);
                    break;

                case UserControl userControl:
                    ShowUserControl(userControl, viewModel, useRegionNavigator);
                    break;

                case Page page:
                    ShowPage(page, viewModel, useRegionNavigator);
                    break;

                default:
                    Debug.WriteLine(
                        $"[ViewManager] No display strategy found for view type {view.GetType().FullName}. " +
                        "Register a custom IViewDisplayStrategy via RegisterDisplayStrategy().");
                    break;
            }
        }

        /// <summary>
        /// \if KO
        /// <para>Show Window 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show window operation.</para>
        /// \endif
        /// </summary>
        /// <param name="window">
        /// \if KO
        /// <para>window에 사용할 <see cref="Window"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Window"/> value used for window.</para>
        /// \endif
        /// </param>
        /// <param name="viewModel">
        /// \if KO
        /// <para>view Model에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view model.</para>
        /// \endif
        /// </param>
        /// <param name="viewModelType">
        /// \if KO
        /// <para>view Model Type에 사용할 <see cref="Type"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Type"/> value used for view model type.</para>
        /// \endif
        /// </param>
        private void ShowWindow(Window window, object viewModel, Type viewModelType)
        {
            string windowKey = GetViewKey(viewModelType);
            IWindowStateService? windowStateService = TryResolve<IWindowStateService>();

            if (windowStateService?.IsOpen(windowKey) == true)
            {
                ActivateExistingWindow(windowKey);
                return;
            }

            window.DataContext = viewModel;

            // 메인 윈도우를 Owner로 지정합니다.
            // WPF는 Owner가 닫힐 때 소유된 모든 창을 자동으로 닫아주므로
            // 메인 창 종료 시 팝업이 남아있는 문제를 방지합니다.
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow is not null && mainWindow != window && mainWindow.IsLoaded)
            {
                window.Owner = mainWindow;
            }

            windowStateService?.MarkOpened(windowKey);

            window.Closed += (_, _) =>
            {
                windowStateService?.MarkClosed(windowKey);
            };

            window.Show();
        }

        /// <summary>
        /// \if KO
        /// <para>Show User Control 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show user control operation.</para>
        /// \endif
        /// </summary>
        /// <param name="userControl">
        /// \if KO
        /// <para>user Control에 사용할 <see cref="UserControl"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="UserControl"/> value used for user control.</para>
        /// \endif
        /// </param>
        /// <param name="viewModel">
        /// \if KO
        /// <para>view Model에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view model.</para>
        /// \endif
        /// </param>
        /// <param name="useRegionNavigator">
        /// \if KO
        /// <para>use Region Navigator에 사용할 <see cref="bool"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="bool"/> value used for use region navigator.</para>
        /// \endif
        /// </param>
        private void ShowUserControl(UserControl userControl, object viewModel, bool useRegionNavigator)
        {
            userControl.DataContext = viewModel;

            INavigator? navigator = useRegionNavigator ? TryResolve<INavigator>() : null;
            if (navigator is not null && navigator is not ViewManager)
            {
                navigator.Navigate(viewModel);
                return;
            }

            new Window
            {
                Content = userControl,
                Width = _fallbackWindowWidth,
                Height = _fallbackWindowHeight
            }.Show();
        }

        /// <summary>
        /// \if KO
        /// <para>Show Page 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the show page operation.</para>
        /// \endif
        /// </summary>
        /// <param name="page">
        /// \if KO
        /// <para>page에 사용할 <see cref="Page"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Page"/> value used for page.</para>
        /// \endif
        /// </param>
        /// <param name="viewModel">
        /// \if KO
        /// <para>view Model에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for view model.</para>
        /// \endif
        /// </param>
        /// <param name="useRegionNavigator">
        /// \if KO
        /// <para>use Region Navigator에 사용할 <see cref="bool"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="bool"/> value used for use region navigator.</para>
        /// \endif
        /// </param>
        private void ShowPage(Page page, object viewModel, bool useRegionNavigator)
        {
            page.DataContext = viewModel;

            INavigator? navigator = useRegionNavigator ? TryResolve<INavigator>() : null;
            if (navigator is not null && navigator is not ViewManager)
            {
                navigator.Navigate(viewModel);
                return;
            }

            new Window
            {
                Content = page,
                Width = _fallbackWindowWidth,
                Height = _fallbackWindowHeight
            }.Show();
        }

        /// <summary>
        /// \if KO
        /// <para>Activate Existing Window 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the activate existing window operation.</para>
        /// \endif
        /// </summary>
        /// <param name="windowKey">
        /// \if KO
        /// <para>window Key에 사용할 <see cref="string"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for window key.</para>
        /// \endif
        /// </param>
        private static void ActivateExistingWindow(string windowKey)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (GetViewKey(window.GetType()) == windowKey)
                {
                    window.Activate();
                    return;
                }

                if (window.DataContext is not null &&
                    GetViewKey(window.DataContext.GetType()) == windowKey)
                {
                    window.Activate();
                    return;
                }
            }
        }

        /// <summary>
        /// \if KO
        /// <para>View Key 값을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the view key value.</para>
        /// \endif
        /// </summary>
        /// <param name="type">
        /// \if KO
        /// <para>type에 사용할 <see cref="Type"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="Type"/> value used for type.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Get View Key 작업에서 생성한 <see cref="string"/> 결과입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> result produced by the get view key operation.</para>
        /// \endif
        /// </returns>
        private static string GetViewKey(Type type)
        {
            string name = type.Name;

            if (name.EndsWith("ViewModel", StringComparison.Ordinal))
                return name[..^"ViewModel".Length];

            if (name.EndsWith("Window", StringComparison.Ordinal))
                return name[..^"Window".Length];

            if (name.EndsWith("View", StringComparison.Ordinal))
                return name[..^"View".Length];

            if (name.EndsWith("Page", StringComparison.Ordinal))
                return name[..^"Page".Length];

            return name;
        }

        /// <summary>
        /// \if KO
        /// <para>Resolve 작업을 시도하고 성공 여부를 반환합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Attempts to resolve and returns whether the operation succeeds.</para>
        /// \endif
        /// </summary>
        /// <typeparam name="T">
        /// \if KO
        /// <para>T 형식 매개변수입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The T type parameter.</para>
        /// \endif
        /// </typeparam>
        /// <returns>
        /// \if KO
        /// <para>Try Resolve 작업에서 생성한 <typeparamref name="T"/> 결과입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <typeparamref name="T"/> result produced by the try resolve operation.</para>
        /// \endif
        /// </returns>
        private T? TryResolve<T>() where T : class
        {
            _resolver.TryResolve<T>(out var result);
            return result;
        }
    }
}
