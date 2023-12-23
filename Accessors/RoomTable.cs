﻿using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.Sources;
using Reloaded.Memory;
using Reloaded.Memory.Sigscan;
using Reloaded.Mod.Interfaces;

using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Diagnostics;

using p4gpc.dungeonloader.Exceptions;
using p4gpc.dungeonloader.JsonClasses;
using p4gpc.dungeonloader.Configuration;
using System.Reflection;
using Reloaded.Memory.Pointers;
using System.Data.SqlTypes;

namespace p4gpc.dungeonloader.Accessors
{
    public class RoomTable : Accessor
    {
        /*
        To do:
            -Account for room connection table (Maybe, since it's tile-by-tile basis, it might have all bases covered already)
         */

        private List<DungeonRoom> _rooms;
        private nuint _newRoomTable;

        public RoomTable(IReloadedHooks hooks, Utilities utils, IMemory memory, Config config, JsonImporter jsonImporter)// : base(hooks, utils, memory, config, jsonImporter)
        {
            _rooms = jsonImporter.GetRooms();
            executeAccessor(hooks, utils, memory, config, jsonImporter);
            _utils.LogDebug("Room hooks established.", Config.DebugLevels.AlertConnections);
        }

        protected override void Initialize()
        {
            // Debugger.Launch();
            List<long> functions;
            String address_str_old;
            String search_string = "0F ?? ?? ?? ";
            long address;
            long func;
            uint oldAddress;
            int totalTemplateTableSize = 0;
            byte SIB;
            byte prefixExists;
            AccessorRegister regToZero;

            List<long> _roomTables; 



            foreach (DungeonRoom room in _rooms)
            {
                totalTemplateTableSize += 86;
            }

            _newRoomTable = _memory.Allocate(totalTemplateTableSize);
            _utils.LogDebug($"Address of NewRoomTable: {_newRoomTable.ToString("X8")}", Config.DebugLevels.TableLocations);

            totalTemplateTableSize = 0;
            foreach (DungeonRoom room in _rooms)
            {
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, room.ID);
                totalTemplateTableSize++;
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, room.sizeX);
                totalTemplateTableSize++;
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, room.sizeY);
                totalTemplateTableSize++;
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, (byte)0);
                totalTemplateTableSize++;
                foreach (List<byte> connectionRow in room.connectionPointers)
                {
                    foreach (byte connection in connectionRow)
                    {
                        _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, connection);
                        totalTemplateTableSize++;
                    }
                }
                foreach (List<byte> revealRow in room.revealProperties)
                {
                    foreach (byte reveal in revealRow)
                    {
                        _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, reveal);
                        totalTemplateTableSize++;
                    }
                }
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, room.unknownMasks[0]);
                totalTemplateTableSize++;
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, room.unknownMasks[1]);
                totalTemplateTableSize++;
                foreach (List<byte> row in room.mapRamOutline)
                {
                    foreach (byte value in row)
                    {

                        _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, value);
                        totalTemplateTableSize++;
                    }
                }
                _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, (byte)0);
                totalTemplateTableSize++;
                foreach (List<int> row in room.connectionValues)
                {
                    foreach (int value in row)
                    {
                        _memory.SafeWrite(_newRoomTable+(nuint)totalTemplateTableSize, value);
                        totalTemplateTableSize+=4;
                    }
                }

            }

            _roomTables = _utils.SigScan_FindCount("48 6B C8 56", "RoomTable Reference Function", 4);

            // -86 present since room 0 is unused, so need to have the blank space somewhere
            // Not sure if I'm happy with this workaround, but we'll give it a shot

            foreach (long _roomTable in _roomTables)
            {
                _memory.SafeRead((nuint)_roomTable+4, out prefixExists);
                if (0x40 <= prefixExists && prefixExists <= 0x4F)
                {
                    _memory.SafeRead((nuint)_roomTable+8, out SIB);
                    _memory.SafeRead((nuint)_roomTable+9, out oldAddress);
                    regToZero = (AccessorRegister)((SIB >> 3) & 0x7);
                    regToZero += (prefixExists & 0x2) << 2;
                    ReplaceImul(_roomTable, 13, regToZero);
                    _utils.LogDebug($"Location of [{search_string}]: {_roomTable.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);
                }
                else
                {
                    _memory.SafeRead((nuint)_roomTable+7, out SIB);
                    _memory.SafeRead((nuint)_roomTable+8, out oldAddress);
                    regToZero = (AccessorRegister)((SIB >> 3) & 0x7);
                    ReplaceImul(_roomTable, 12, regToZero);
                    _utils.LogDebug($"Location of [{search_string}]: {_roomTable.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);
                }

                address_str_old = (oldAddress+0x10).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("0F 10 ?? ?? " + address_str_old, $"RoomTable Address Chunk #2 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+4, (Int32)(_newRoomTable+0x10));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

                address_str_old = (oldAddress+0x20).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("0F 10 ?? ?? " + address_str_old, $"RoomTable Address Chunk #3 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+4, (Int32)(_newRoomTable+0x20));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

                address_str_old = (oldAddress+0x30).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("0F 10 ?? ?? " + address_str_old, $"RoomTable Address Chunk #4 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+4, (Int32)(_newRoomTable+0x30));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

                address_str_old = (oldAddress+0x40).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("0F 10 ?? ?? " + address_str_old, $"RoomTable Address Chunk #5 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+4, (Int32)(_newRoomTable+0x40));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

                address_str_old = (oldAddress+0x50).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("8B 84 ?? " + address_str_old, $"RoomTable Address Chunk #6 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+3, (Int32)(_newRoomTable+0x50));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

                address_str_old = (oldAddress+0x54).ToString("X8");
                address_str_old = address_str_old.Substring(6, 2) + " " + address_str_old.Substring(4, 2) + " " + address_str_old.Substring(2, 2) + " " + address_str_old.Substring(0, 2);
                func = _utils.SigScan("0F B7 ?? ?? " + address_str_old, $"RoomTable Address Chunk #7 [{oldAddress.ToString("X8")}]");
                _memory.SafeWrite((nuint)func+4, (Int32)(_newRoomTable+0x54));
                _utils.LogDebug($"Location of [0F 10 ?? ?? {address_str_old}]: {func.ToString("X8")}", Config.DebugLevels.CodeReplacedLocations);

            }
        }
        
        private void ReplaceImul(Int64 functionAddress, int length, AccessorRegister offsetReg)
        {
            AccessorRegister pushReg;
            List<AccessorRegister> usedRegs;
            List<string> instruction_list = new List<string>();
            instruction_list.Add($"use64");
            instruction_list.Add($"imul rcx, rax, 0x56");
            instruction_list.Add($"mov {offsetReg}, 0");
            instruction_list.Add($"movups xmm0, [{_newRoomTable} + rcx]");

            _functionHookList.Add(_hooks.CreateAsmHook(instruction_list.ToArray(), functionAddress, AsmHookBehaviour.DoNotExecuteOriginal, length).Activate());
        }

    }
}
