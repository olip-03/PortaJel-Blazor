﻿using Jellyfin.Sdk.Generated.Models;
using SQLite;
using System.Text.Json;
using Portajel.Connections.Data;
using PortaJel_Blazor.Classes;

namespace Portajel.Connections.Database
{
    public class PlaylistData
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public Guid LocalId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsFavourite { get; set; } = false;
        public string ImgSource { get; set; } = string.Empty;
        public string ImgBlurhash { get; set; } = string.Empty;
        public string SongIdsJson { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public bool IsPartial { get; set; } = true;
        public Guid[] GetSongIds()
        {
            Guid[]? artistIds = JsonSerializer.Deserialize<Guid[]>(SongIdsJson);
            if (artistIds == null) return [];
            return artistIds;
        }
        public static PlaylistData Builder(BaseItemDto baseItem, string server, BaseItemDto[]? songData = null)
        {
            if (baseItem.Id == null)
            {
                throw new ArgumentException("Cannot create Playlist without ServerId! Please fix server call flags!");
            }
            if (baseItem.Name == null)
            {
                throw new ArgumentException("Cannot create Playlist without Name! Please fix server call flags!");
            }
            if (baseItem.Path == null)
            {
                throw new ArgumentException("Cannot create Playlist without Path! Please fix server call flags!");
            }
            if(baseItem.UserData == null || baseItem.UserData.IsFavorite == null)
            {
                throw new ArgumentException("Cannot create Playlist without UserData! Please fix server call flags!");
            }
            PlaylistData newPlaylist = new();
            MusicItemImage musicItemImage = MusicItemImage.Builder(baseItem, server);
            newPlaylist.Name = baseItem.Name;
            newPlaylist.Id = (Guid)baseItem.Id;
            newPlaylist.LocalId = GuidHelper.GenerateNewGuidFromHash(newPlaylist.Id, server);
            newPlaylist.IsFavourite = (bool)baseItem.UserData.IsFavorite;
            newPlaylist.Path = baseItem.Path;
            newPlaylist.ServerAddress = server;
            newPlaylist.ImgSource = musicItemImage.Source;
            newPlaylist.ImgBlurhash = musicItemImage.Blurhash;
            if (songData != null)
            {
                newPlaylist.SongIdsJson = JsonSerializer.Serialize(songData.Select(idPair => idPair.Id).ToArray());
            }
            return newPlaylist;
        }
    }
}
