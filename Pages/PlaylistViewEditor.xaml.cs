using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Pages;

// Access this page by using
// await Navigation.PushModalAsync(new PlaylistViewEditor());

public partial class PlaylistViewEditor : ContentPage
{
    private Playlist playlist = Playlist.Empty;
    private Guid playlistId = Guid.Empty;

    private List<Song> unorderedList = new();
    private List<Song> songList = new();

    private bool isLoading = false;

    public PlaylistViewEditor()
	{
        InitializeComponent();
        Initalize();

    }
    public PlaylistViewEditor(Guid setPlaylistId)
    {
        playlistId = setPlaylistId;
        InitializeComponent();
        Initalize();
    }
    public PlaylistViewEditor(Playlist setPlaylist)
    {
        playlistId = setPlaylist.Id;
        playlist = setPlaylist;

        InitializeComponent();
        Initalize();
    }
    
    #region Methods
    private async void Initalize()
    {
        if(playlist == Playlist.Empty)
        {
            // fetch the playlist MEEEEOWWW
            isLoading = true;
            loadingScreen.IsVisible = isLoading;
            // Neccesary to stop the page from lagging when you open it
            await Task.Run(() => { Thread.Sleep(200); });
            Playlist? getPlaylist = await MauiProgram.Server.Playlist.GetPlaylistAsync(playlistId);
            if(getPlaylist != null)
            {
                playlist = getPlaylist;
            }
        }

        songList = playlist.Songs.Select(song => new Song(song)).ToList();
        unorderedList = playlist.Songs.Select(song => new Song(song)).ToList();
        img_main.Source = playlist.ImgSource;
        txt_playlistName.Text = playlist.Name;

        Playlist_List.ItemsSource = songList;
        save_btn.IsVisible = true;
        save_activityIndicator.IsVisible = false;

        if (isLoading)
        {
            await loadingScreen.FadeTo(0, 500, Easing.SinIn);
            isLoading = false;
        }
        
        loadingScreen.IsVisible = isLoading;
    }
    private async void ClosePage()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
    private async void SavePage()
    {
        save_activityIndicator.IsVisible = true;
        save_btn.IsVisible = false;

        // load playlist page from saved data
        // MauiProgram.mainLayout.NavigatePlaylist(playlist.id);

        await Task.Run(async () =>
        {
            //Remove all songs that have also been removed from the local copy 
            // Index, Song
            // List<Tuple<int, Song>> removedSongs = new List<Tuple<int, Song>>();
            int index = 0;
            foreach (Song song in unorderedList.ToList())
            {
                if (!songList.Contains(song))
                { // This song has been removed
                    // removedSongs.Add(new Tuple<int, Song>(index, song));
                    if (song.PlaylistId != null)
                    {
                        await MauiProgram.Server.Playlist.RemovePlaylistItemAsync(playlist.Id, Guid.Parse(song.PlaylistId));
                    }
                    unorderedList.Remove(song);
                }
                index++;
            }

            // Reshuffle server items to match the one in the plalist
            // This might be sketch. Gonna wait for this to break before I can be fucked fixing it 

            // playlist.songs should be the original playlist without changes made to it
            // unlike the songList which will the updated list we want to apply
            bool reordered = false;
            while (!reordered)
            {
                bool completedLoop = true;
                for (int i = 0; i < unorderedList.Count; i++)
                {
                    if (unorderedList[i].Id != songList[i].Id)
                    {
                        int newIndex = songList.IndexOf(unorderedList[i]);
                        Song item = unorderedList[i];

                        // Needs to be moved
                        unorderedList.RemoveAt(i);
                        unorderedList.Insert(newIndex, item);

                        bool passed = await MauiProgram.Server.Playlist.MovePlaylistItem(playlist.Id, Guid.Parse(item.PlaylistId), newIndex);
                        if (!passed)
                        {
                            // wah wah
                        }

                        // Forefit this loop, leaves completedLoop false
                        completedLoop = false;
                        break;
                    }
                }
                if (!completedLoop)
                {
                    break;
                }
                reordered = completedLoop;
            }
        });

        // Close this modal 
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
    #endregion

    #region Interactions
    private void ImageButton_Close_Clicked(object sender, EventArgs e)
    {
        ClosePage();
    }
    private void ImageButton_Save_Clicked(object sender, EventArgs e)
    {
        SavePage();
    }
    private void ImageButton_PlaylistItem_Remove(object sender, EventArgs e)
    {
        ImageButton? button = sender as ImageButton;
        if(button == null) { return; }
        Song? song = button.BindingContext as Song;
        if (song == null) { return; }
    }
    #endregion
}