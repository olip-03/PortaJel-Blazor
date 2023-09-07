using Microsoft.Maui.Controls;

namespace PortaJel_Blazor;

public partial class App : Application
{
	public App()
<<<<<<< HEAD
	{
		InitializeComponent(); 
        MainPage = new MainPage();
=======
    {
		InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
>>>>>>> 9db61e6d4a4e0f65aa645f4f807a0f0e7a31b80f
    }
}
