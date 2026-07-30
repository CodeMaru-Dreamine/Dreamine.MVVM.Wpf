using System.Reflection;
using Dreamine.MVVM.Interfaces.DependencyInjection;
using Dreamine.MVVM.Interfaces.Windows;
using Dreamine.MVVM.Locators;
using Dreamine.MVVM.Wpf;
using Xunit;

namespace Dreamine.MVVM.Wpf.Tests;

public sealed class WpfRuntimeTests
{
    [Fact]
    public void DefaultOptions_EnableTheExpectedRuntimeFeatures()
    {
        var options = DreamineWpfOptions.CreateDefault();

        Assert.True(options.EnableGlobalAutoWireOnLoaded);
        Assert.True(options.RegisterDefaultServices);
        Assert.True(options.EnableAutoNavigatorRegistration);
        Assert.Equal("SubPage", options.DefaultRegionName);
        Assert.Equal(800, options.FallbackWindowWidth);
        Assert.Equal(600, options.FallbackWindowHeight);
    }

    [Fact]
    public void Options_AreMutableForApplicationConfiguration()
    {
        var options = new DreamineWpfOptions
        {
            EnableGlobalAutoWireOnLoaded = true,
            RegisterDefaultServices = true,
            EnableAutoNavigatorRegistration = true,
            DefaultRegionName = "Workspace",
            FallbackWindowWidth = 1024,
            FallbackWindowHeight = 720
        };

        Assert.Equal("Workspace", options.DefaultRegionName);
        Assert.Equal(1024, options.FallbackWindowWidth);
        Assert.Equal(720, options.FallbackWindowHeight);
    }

    [Fact]
    public void WindowStateService_TracksStateAndRaisesEvents()
    {
        var service = new WindowStateService();
        var changes = new List<(string Key, bool IsOpen)>();
        service.StateChanged += (_, e) => changes.Add((e.WindowKey, e.IsOpen));

        Assert.False(service.IsOpen("Settings"));
        service.MarkOpened("Settings");
        Assert.True(service.IsOpen("Settings"));
        service.MarkClosed("Settings");

        Assert.False(service.IsOpen("Settings"));
        Assert.Equal([("Settings", true), ("Settings", false)], changes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WindowStateService_RejectsEmptyKeys(string key)
    {
        var service = new WindowStateService();

        Assert.Throws<ArgumentException>(() => service.IsOpen(key));
        Assert.Throws<ArgumentException>(() => service.MarkOpened(key));
        Assert.Throws<ArgumentException>(() => service.MarkClosed(key));
    }

    [Theory]
    [InlineData("MainViewModel", "Main")]
    [InlineData("MainWindow", "Main")]
    [InlineData("MainView", "Main")]
    [InlineData("MainPage", "Main")]
    [InlineData("Main", "Main")]
    public void ViewManager_NormalizesViewKeys(string typeName, string expected)
    {
        var method = typeof(ViewManager).GetMethod(
            "GetViewKey",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var type = CreateNamedType(typeName);
        var actual = method.Invoke(null, [type]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Builder_RejectsNullArgumentsAndInvalidRegions()
    {
        Assert.Throws<ArgumentNullException>(
            () => DreamineAppBuilder.Initialize(null!));
        Assert.Throws<ArgumentNullException>(
            () => DreamineAppBuilder.Initialize(typeof(WpfRuntimeTests).Assembly, null!));
        Assert.Throws<ArgumentException>(
            () => DreamineAppBuilder.RegisterAutoNavigatorHandlerOnce(""));
        Assert.Throws<ArgumentNullException>(
            () => DreamineAppBuilder.RegisterNavigatorFromWindow(null!, "Main"));
    }

    [Fact]
    public void ViewManager_RejectsNullInputs()
    {
        var manager = new ViewManager(new FakeResolver());

        Assert.Throws<ArgumentNullException>(() => manager.RegisterDisplayStrategy(null!));
        Assert.Throws<ArgumentNullException>(() => manager.Show((Type)null!));
        Assert.Throws<ArgumentNullException>(() => manager.Navigate(null!));
        Assert.Throws<ArgumentNullException>(() => new ViewManager(null!));
    }

    [Fact]
    public void Show_ResolvesAndDisplaysRegisteredViewThroughCustomStrategy()
    {
        ViewModelLocator.Reset();
        ViewModelLocator.Register(typeof(Test), typeof(TestViewModel));
        var viewModel = new TestViewModel();
        var resolver = new FakeResolver(viewModel);
        var strategy = new RecordingStrategy();
        var manager = new ViewManager(resolver);
        manager.RegisterDisplayStrategy(strategy);

        manager.Show<TestViewModel>();

        Assert.Equal(typeof(TestViewModel), resolver.LastRequestedType);
        Assert.IsType<Test>(strategy.View);
        Assert.Same(viewModel, strategy.ViewModel);
        Assert.Equal(typeof(TestViewModel), strategy.ViewModelType);
        Assert.True(strategy.UseRegionNavigator);
        ViewModelLocator.Reset();
    }

    [Fact]
    public void Navigate_DisplaysProvidedViewModelWithoutResolvingIt()
    {
        ViewModelLocator.Reset();
        ViewModelLocator.Register(typeof(Test), typeof(TestViewModel));
        var resolver = new FakeResolver();
        var strategy = new RecordingStrategy();
        var manager = new ViewManager(resolver);
        manager.RegisterDisplayStrategy(strategy);
        var viewModel = new TestViewModel();

        manager.Navigate(viewModel);

        Assert.Null(resolver.LastRequestedType);
        Assert.IsType<Test>(strategy.View);
        Assert.Same(viewModel, strategy.ViewModel);
        Assert.False(strategy.UseRegionNavigator);
        ViewModelLocator.Reset();
    }

    [Fact]
    public void Show_ReturnsWithoutDisplayWhenNoViewIsRegistered()
    {
        ViewModelLocator.Reset();
        var strategy = new RecordingStrategy();
        var manager = new ViewManager(new FakeResolver(new UnmappedViewModel()));
        manager.RegisterDisplayStrategy(strategy);

        manager.Show<UnmappedViewModel>();

        Assert.Null(strategy.View);
    }

    private static Type CreateNamedType(string name)
    {
        var assemblyName = new AssemblyName($"DynamicTests_{Guid.NewGuid():N}");
        var assembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            System.Reflection.Emit.AssemblyBuilderAccess.Run);
        return assembly.DefineDynamicModule("Tests").DefineType(name).CreateType()!;
    }

    public sealed class Test;

    public sealed class TestViewModel;

    public sealed class UnmappedViewModel;

    private sealed class FakeResolver(object? instance = null) : IServiceResolver
    {
        public Type? LastRequestedType { get; private set; }

        public TService Resolve<TService>() where TService : class =>
            (TService)Resolve(typeof(TService));

        public object Resolve(Type serviceType)
        {
            LastRequestedType = serviceType;
            return instance ?? Activator.CreateInstance(serviceType)!;
        }

        public bool TryResolve<TService>(out TService? result) where TService : class
        {
            result = instance as TService;
            return result is not null;
        }
    }

    private sealed class RecordingStrategy : IViewDisplayStrategy
    {
        public object? View { get; private set; }
        public object? ViewModel { get; private set; }
        public Type? ViewModelType { get; private set; }
        public bool UseRegionNavigator { get; private set; }

        public bool CanHandle(object view) => true;

        public void Display(
            object view,
            object viewModel,
            Type viewModelType,
            bool useRegionNavigator)
        {
            View = view;
            ViewModel = viewModel;
            ViewModelType = viewModelType;
            UseRegionNavigator = useRegionNavigator;
        }
    }
}
