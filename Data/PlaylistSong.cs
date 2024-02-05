using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class PlaylistSong: Song
    {
        public string playlistId = string.Empty;
        public static readonly PlaylistSong Empty = new(setGuid: Guid.Empty, setName: String.Empty, setPlaylistId: String.Empty);
        public PlaylistSong(Guid setGuid, string setName, string setPlaylistId, string? setServerId = null, Guid[]? setArtistIds = null, Guid? setAlbumID = null, string? setStreamUrl = null, int? setDiskNum = 0, bool setIsFavourite = false, string? setFileLocation = null, bool setIsDownloaded = false) : base(setGuid, setName, setServerId, setArtistIds, setAlbumID, setStreamUrl, setDiskNum, setIsFavourite, setFileLocation, setIsDownloaded)
        {
            // Required variables
            id = setGuid;
            name = setName;
            playlistId = setPlaylistId;

            // Rest of em
            if (setArtistIds != null)
            { //Set Artists
                List<Artist> newArtistsIDs = new();
                for (int i = 0; i < setArtistIds.Length; i++)
                {
                    Artist toAdd = new();
                    toAdd.id = setArtistIds[i];
                    newArtistsIDs.Add(toAdd);
                }
                artists = newArtistsIDs.ToArray();
            }
            if (setAlbumID != null)
            { //Set Album
                Album setAlbum = new Album();
                setAlbum.id = (Guid)setAlbumID;
                album = setAlbum;
            }
            if (setStreamUrl != null)
            { //Set Stream Location
                streamUrl = setStreamUrl;
            }
            if (setDiskNum != null)
            { //Set Disk Number
                diskNum = (int)setDiskNum;
            }
            if (setFileLocation != null)
            { //Set File Location
                fileLocation = setFileLocation;
            }
            isFavourite = setIsFavourite;
            isDownloaded = setIsDownloaded;
        }
        public Song ToSong()
        {
            return this;
        }
    }
}
