﻿using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public abstract class BaseMusicItem
    {
        public Guid id { get; set; } = Guid.Empty;
        public string name { get; set; } = String.Empty;
        public bool isFavourite { get; set; } = false;
        public MusicItemImage image { get; set; } = MusicItemImage.Empty;
        public List<ContextMenuItem> contextMenuItems { get; set; } = new();
    }
}
