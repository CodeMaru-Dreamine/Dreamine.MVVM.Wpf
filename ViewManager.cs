using Dreamine.MVVM.Core;
using Dreamine.MVVM.Interfaces.Navigation;
using Dreamine.MVVM.Interfaces.Windows;
using Dreamine.MVVM.Locators;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dreamine.MVVM.Wpf
{
    /// <summary>
    /// Resolves and displays WPF Views based on ViewModel types.
    /// </summary>
    public sealed class ViewManager : IViewManager
    {
        /// <inheritdoc />
        public void Show<TViewModel>() where TViewModel : class
        {
            Show(typeof(TViewModel));
        }

        /// <inheritdoc />
        public void Show(Type viewModelType)
        {
            ArgumentNullException.ThrowIfNull(viewModelType);

            object viewModel = DMContainer.Resolve(viewModelType);
            object? view = ViewModelLocator.ResolveView(viewModelType);
            DisplayResolvedView(view, viewModel, viewModelType, useRegionNavigator: true);
        }

        /// <inheritdoc />
        public void Navigate(object viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            Type viewModelType = viewModel.GetType();
            object? view = ViewModelLocator.ResolveView(viewModelType);
            DisplayResolvedView(view, viewModel, viewModelType, useRegionNavigator: false);
        }

        private static void DisplayResolvedView(
            object? view,
            object viewModel,
            Type viewModelType,
            bool useRegionNavigator)
        {
            if (view is null)
            {
                return;
            }

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
            }
        }

        private static void ShowWindow(Window window, object viewModel, Type viewModelType)
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

        private static void ShowUserControl(UserControl userControl, object viewModel, bool useRegionNavigator)
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
                Width = 800,
                Height = 600
            }.Show();
        }

        private static void ShowPage(Page page, object viewModel, bool useRegionNavigator)
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
                Width = 800,
                Height = 600
            }.Show();
        }

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

        private static string GetViewKey(Type type)
        {
            string name = type.Name;

            if (name.EndsWith("ViewModel", StringComparison.Ordinal))
            {
                return name[..^"ViewModel".Length];
            }

            if (name.EndsWith("Window", StringComparison.Ordinal))
            {
                return name[..^"Window".Length];
            }

            if (name.EndsWith("View", StringComparison.Ordinal))
            {
                return name[..^"View".Length];
            }

            if (name.EndsWith("Page", StringComparison.Ordinal))
            {
                return name[..^"Page".Length];
            }

            return name;
        }

        private static T? TryResolve<T>() where T : class
        {
            try
            {
                return DMContainer.Resolve<T>();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
