﻿using p4gpc.dungeonframework.Configuration;
using p4gpc.dungeonframework.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using Newtonsoft.Json;

namespace p4gpc.dungeonframework.JsonClasses
{
    public class JsonImporter
    {
        private List<DungeonTemplates> _templates;
        private List<DungeonFloor> _floors;
        private List<DungeonRoom> _rooms;
        private List<DungeonMinimap> _minimap;
        private FieldCompare _fieldCompare;
        private Dictionary<byte, byte> _dungeon_template_dict = new Dictionary<byte, byte>();

        public JsonImporter(Config config, Utilities _utils, string jsonPath = "", string defaultPath="" )
        {
            Dictionary<string, byte> temp;
            StreamReader jsonReader;
            bool hasCustom = Directory.Exists(jsonPath);

            if (File.Exists(jsonPath + "/dungeon_templates.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/dungeon_templates.json");
            }
            else
            {
                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load dungeon_templates.json from Persona 4 Golden mod folder failed, defaulting to vanilla dungeon_templates.json");
                }
                jsonReader = new StreamReader(defaultPath + "/dungeon_templates.json");
            }
            string jsonContents = jsonReader.ReadToEnd();
            _templates = JsonSerializer.Deserialize<List<DungeonTemplates>>(jsonContents)!;

            if (File.Exists(jsonPath + "/dungeon_floors.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/dungeon_floors.json");
            }
            else
            {

                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load dungeon_floors.json from Persona 4 Golden mod folder failed, defaulting to vanilla dungeon_floors.json");
                }
                jsonReader = new StreamReader(defaultPath + "/dungeon_floors.json");
            }
            jsonContents = jsonReader.ReadToEnd();
            _floors = JsonSerializer.Deserialize<List<DungeonFloor>>(jsonContents)!;

            if (File.Exists(jsonPath + "/dungeon_rooms.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/dungeon_rooms.json");
            }
            else
            {
                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load dungeon_minimap.json from Persona 4 Golden mod folder failed, defaulting to vanilla dungeon_rooms.json");
                }
                jsonReader = new StreamReader(defaultPath + "/dungeon_rooms.json");
            }
            jsonContents = jsonReader.ReadToEnd();
            _rooms = JsonSerializer.Deserialize<List<DungeonRoom>>(jsonContents)!;

            if (File.Exists(jsonPath + "/dungeon_minimap.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/dungeon_minimap.json");
            }
            else
            {
                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load dungeon_minimap.json from Persona 4 Golden mod folder failed, defaulting to vanilla dungeon_minimap.json");
                }
                jsonReader = new StreamReader(defaultPath + "/dungeon_minimap.json");
            }
            jsonContents = jsonReader.ReadToEnd();
            _minimap = JsonSerializer.Deserialize<List<DungeonMinimap>>(jsonContents)!;

            /*
            Currently unsupported, reasoning given in Program.cs 

             if (File.Exists(jsonPath + "/field_compares.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/field_compares.json");
            }
            else
            {
                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load field_compares.json from Persona 4 Golden mod folder failed, defaulting to vanilla field_compares.json");
                }
                jsonReader = new StreamReader(defaultPath + "/field_compares.json");
            }
            jsonContents = jsonReader.ReadToEnd();
            _fieldCompare = JsonSerializer.Deserialize<FieldCompare>(jsonContents)!;
             */

            if (File.Exists(jsonPath + "/dungeon_template_dict.json"))
            {

                jsonReader = new StreamReader(jsonPath + "/dungeon_template_dict.json");
            }
            else
            {
                if (hasCustom && !config.suppressWarnErr)
                {
                    _utils.LogError($"Attempt to load dungeon_template_dict.json from Persona 4 Golden mod folder failed, defaulting to vanilla dungeon_template_dict.json");
                }
                jsonReader = new StreamReader(defaultPath + "/dungeon_template_dict.json");
            }
            jsonContents = jsonReader.ReadToEnd();
            temp = JsonSerializer.Deserialize<Dictionary<string, byte>>(jsonContents)!;
            foreach (string key in temp.Keys)
            {
                _dungeon_template_dict.Add(Byte.Parse(key), temp[key]);
            }

            jsonReader.Close();
        }
        public List<DungeonTemplates> GetTemplates()
        {
            return _templates;
        }
        public List<DungeonFloor> GetFloors()
        {
            return _floors;
        }

        public List<DungeonRoom> GetRooms()
        {
            return _rooms;
        }

        public List<DungeonMinimap> GetMinimap()
        {
            return _minimap;
        }

        public FieldCompare GetFieldCompare()
        {
            return _fieldCompare;
        }

        public Dictionary<byte, byte> GetDungeonTemplateDictionary()
        {
            return _dungeon_template_dict;
        }
    }
}
