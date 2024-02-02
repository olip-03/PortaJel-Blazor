using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Pages.Xaml;

// Access this page by using
// await Navigation.PushModalAsync(new PlaylistViewEditor());

public partial class PlaylistViewEditor : ContentPage
{
    private Playlist playlist = Playlist.Empty;
    private Guid playlistId = Guid.Empty;

    private List<Song> songList = new();

	public PlaylistViewEditor()
	{
		InitializeComponent();
	}
    public PlaylistViewEditor(Guid setPlaylistId)
    {
        playlistId = setPlaylistId;
        InitializeComponent();

        txt_debug.Text = playlistId.ToString();
    }
    public PlaylistViewEditor(Playlist setPlaylist)
    {
        playlistId = setPlaylist.id;
        playlist = setPlaylist;

        songList = playlist.songs.ToList();

        InitializeComponent();

        img_main.Source = playlist.image.source;
        txt_debug.Text = playlist.name;
        Playlist_List.ItemsSource = songList;
    }
    #region Methods
    private async void ClosePage()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        } 
    }
    #endregion

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        ClosePage();
    }
}