﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class QueueViewModel : BindableObject
    {
        public static readonly BindableProperty SongQueueProperty = BindableProperty.Create(nameof(SongQueue), typeof(double), typeof(QueueViewModel), default(SongGroupCollection));
        public SongGroupCollection? SongQueue
        {
            get => (SongGroupCollection?)GetValue(SongQueueProperty);
            set => SetValue(SongQueueProperty, value);

        }

        public static readonly BindableProperty PlaybackPercentageProperty = BindableProperty.Create(nameof(PlaybackValue), typeof(double), typeof(QueueViewModel), default(double));
        public double? PlaybackValue
        {
            get => (double?)GetValue(PlaybackPercentageProperty);
            set => SetValue(PlaybackPercentageProperty, value);

        }

        public static readonly BindableProperty PlayButtonSourceProperty = BindableProperty.Create(nameof(PlayButtonSource), typeof(string), typeof(QueueViewModel), default(string));
        public string? PlayButtonSource
        {
            get => (string?)GetValue(PlayButtonSourceProperty);
            set => SetValue(PlayButtonSourceProperty, value);
        }

        public static readonly BindableProperty PlayingFromAlbumProperty = BindableProperty.Create(nameof(PlayingFromAlbum), typeof(string), typeof(QueueViewModel), default(string));
        public string? PlayingFromAlbum
        {
            get => (string?)GetValue(PlayingFromAlbumProperty);
            set => SetValue(PlayingFromAlbumProperty, value);
        }
    }
}
