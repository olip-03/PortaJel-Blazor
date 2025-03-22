namespace Portajel
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                window.TitleBar = new TitleBar
                {
                    HeightRequest = 48,
                    Icon = "titlebar_icon.png",
                    Title = "PortaJel",
                    Subtitle = "Demo",
                    Content = new SearchBar
                    {
                        PlaceholderColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        WidthRequest = 300,
                        HeightRequest = 32,
                        Placeholder = "Search"
                    },
                    TrailingContent = new ImageButton
                    {
                        HeightRequest = 36,
                        WidthRequest = 36,
                        Source = new FontImageSource
                        { 
                            Size = 16,
                            Glyph = "&#xE713;",
                            FontFamily = "SegoeMDL2"
                        }
                    }
                };
            }
            return window;
        }
    }
}
