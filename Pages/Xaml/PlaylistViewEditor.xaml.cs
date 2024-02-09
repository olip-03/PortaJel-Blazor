using Microsoft.Maui.Controls.Platform;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.UI.Drawables;

namespace PortaJel_Blazor.Pages.Xaml;

// Access this page by using
// await Navigation.PushModalAsync(new PlaylistViewEditor());

public partial class PlaylistViewEditor : ContentPage
{
    private Playlist playlist = Playlist.Empty;
    private Guid playlistId = Guid.Empty;

    private List<PlaylistSong> unorderedList = new();
    private List<PlaylistSong> songList = new();

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
        playlistId = setPlaylist.id;
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
            Playlist? getPlaylist = await MauiProgram.servers[0].FetchPlaylistByIDAsync(playlistId);
            if(getPlaylist != null)
            {
                playlist = getPlaylist;
            }
        }

        songList = playlist.songs.ToList();
        unorderedList = playlist.songs.ToList();
        img_main.Source = playlist.image.source;
        txt_playlistName.Text = playlist.name;

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

        // Save data from the modal
        MauiProgram.playlistDictionary[playlist.id].name = txt_playlistName.Text;
        MauiProgram.playlistDictionary[playlist.id].songs = songList.ToArray();
        // load playlist page from saved data
        // MauiProgram.mainLayout.NavigatePlaylist(playlist.id);

        await Task.Run(async () =>
        {
            //Remove all songs that have also been removed from the local copy 
            // Index, Song
            List<Tuple<int, PlaylistSong>> removedSongs = new List<Tuple<int, PlaylistSong>>();
            int index = 0;
            foreach (PlaylistSong song in unorderedList)
            {
                if (!songList.Contains(song))
                { // This song has been removed
                    removedSongs.Add(new Tuple<int, PlaylistSong>(index, song));
                    await MauiProgram.servers[0].RemovePlaylistItem(playlist.id, song.playlistId);
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
                    if (unorderedList[i].id != songList[i].id)
                    {
                        int newIndex = songList.IndexOf(unorderedList[i]);
                        PlaylistSong item = unorderedList[i];

                        // Needs to be moved
                        unorderedList.RemoveAt(i);
                        unorderedList.Insert(newIndex, item);

                        bool passed = MauiProgram.servers[0].MovePlaylistItem(playlist.id, item.playlistId, newIndex).Result;
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
        PlaylistSong? song = button.BindingContext as PlaylistSong;
        if (song == null) { return; }
    }
    #endregion
}