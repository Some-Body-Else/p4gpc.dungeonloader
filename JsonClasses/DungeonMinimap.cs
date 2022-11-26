﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4gpc.dungeonloader.JsonClasses
{
    public class DungeonMinimap
    {
        public int roomID { get; set; }
        public bool multipleNames { get; set; }
        public string name { get; set; }
        public List<string> names { get; set; }
        public List<byte> uVarsSingle { get; set; }
        public List<List<byte>> uVarsMulti { get; set; }
    }
}
