using PortaJel_Blazor.Data;
namespace PortaJel_Blazor.Pages
{
    public class HomeCache
    {
        public BaseMusicItem[] recentPlayData { get; set; } = new Album[0];
        public string[] recentPlayPlaceholderImages { get; set; } = new string[0];

        public BaseMusicItem[] favouritesPlayData { get; set; } = new Album[0];
        public string[] favouritesPlaceholderImages { get; set; } = new string[0];

        public BaseMusicItem[] recentAddedData { get; set; } = new Album[0];
        public string[] recentAddedPlaceholderImages { get; set; } = new string[0];

        public BaseMusicItem[] mostPlayData { get; set; } = new Album[0];
        public string[] mostPlayPlaceholderImages { get; set; } = new string[0];

        public static HomeCache Empty {  get { return new HomeCache(); } }
        public bool IsEmpty() 
        {
            if (recentPlayData.Length > 0 &&
                favouritesPlayData.Length > 0 &&
                recentAddedData.Length > 0 &&
                mostPlayData.Length > 0)
            {
                return false;
            } 
            return true;
        }
    }
}
