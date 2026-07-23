using System;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Commands {
    public class CommandRegistrar : ICommandRegistrar {
        private CommandHandler _commandHandler;
        private CustomComputer _computer;

        public void Initialize() {
            _commandHandler = CommandHandler.Singleton;
            _computer = CustomComputer.Singleton;
            
            RegisterCommands();
        }

        public void RegisterCommands() {
            
            _commandHandler.AddCommand(new Command("setcolor", new[] { typeof(float), typeof(float), typeof(float) }, args => {
                var r = Mathf.Clamp((float)args[0], 0f, 255f);
                var g = Mathf.Clamp((float)args[1], 0f, 255f);
                var b = Mathf.Clamp((float)args[2], 0f, 255f);

                BaseGameInterface.SetColor(r / 255f, g / 255f, b / 255f);
                return $"Updated color:\n\nR: {r}\nG: {g}\nB: {b}\n";
            }));

            
            _commandHandler.AddCommand(new Command("setname", new[] { typeof(string) }, args => {
                var newName = ((string)args[0]).ToUpper();

                var result = BaseGameInterface.SetName(newName);

                return result == EWordCheckResult.Allowed ? $"Updated name: {newName.Replace(" ", "")}" : $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            
            
            _commandHandler.AddCommand(new Command("leave", null, args => {
                if (NetworkSystem.Instance.InRoom) {
                    BaseGameInterface.Disconnect();
                    return "Left room!";
                }
                return "You aren't currently in a room.";
            }));

            
            
            _commandHandler.AddCommand(new Command("join", new[] { typeof(string) }, args => {
                var roomId = (string)args[0];

                roomId = roomId.ToUpper();
                var result = BaseGameInterface.JoinRoom(roomId);

                return result == EWordCheckResult.Allowed ? $"Joining room: {roomId}" : $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            
            
            _commandHandler.AddCommand(new Command("cam", new[] { typeof(string) }, args => {
                
                
                if (GorillaTagger.Instance.thirdPersonCamera == null)
                    return "Error: Could not find camera";
                var camera = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
                if (camera == null)
                    return "Error: Could not find camera";

                var argString = (string)args[0];

                if (argString == "fp" || argString == "tp") {
                    camera.enabled = argString == "tp";
                    return $"Updated camera: {(argString == "tp" ? "Third" : "First")} person";
                }

                return "Invalid syntax! Use fp/tp to use the command";
            }));

            
            
            _commandHandler.AddCommand(new Command("setbg", new[] { typeof(float), typeof(float), typeof(float) }, args => {
                var r = (float)args[0];
                var g = (float)args[1];
                var b = (float)args[2];

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                _computer.SetBG(r, g, b);

                return $"Updated background:\n\nR: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));
            
            
            
            _commandHandler.AddCommand(new Command("resetbg", null, args => {
                _computer.SetBGImage(new ComputerViewChangeBackgroundEventArgs(_computer.GetTexture(_computer.GetScreenBackgroundPath())));
                return "Successfully reset background";
            }));

            _commandHandler.AddCommand(new Command("getname", null, args =>
                $"Name: {BaseGameInterface.GetName()}"));

            _commandHandler.AddCommand(new Command("getcolor", null, args => {
                BaseGameInterface.GetColor(out var r, out var g, out var b);
                return $"R: {Mathf.RoundToInt(r * 255f)}\nG: {Mathf.RoundToInt(g * 255f)}\nB: {Mathf.RoundToInt(b * 255f)}";
            }));

            _commandHandler.AddCommand(new Command("turnmode", new[] { typeof(string) }, args => {
                if (!Enum.TryParse<ETurnMode>((string)args[0], true, out var turnMode))
                    return "Invalid turn mode! Use snap, smooth, or none.";

                BaseGameInterface.SetTurnMode(turnMode);
                return $"Updated turn mode: {turnMode}";
            }));

            _commandHandler.AddCommand(new Command("turnval", new[] { typeof(int) }, args => {
                var value = Mathf.Clamp((int)args[0], 0, 9);
                BaseGameInterface.SetTurnValue(value);
                return $"Updated turn value: {value}";
            }));

            _commandHandler.AddCommand(new Command("mic", new[] { typeof(string) }, args => {
                EPTTMode? pttMode = ((string)args[0]).ToLower() switch {
                    "open" => EPTTMode.OpenMic,
                    "ptt" => EPTTMode.PushToTalk,
                    "ptm" => EPTTMode.PushToMute,
                    _ => null
                };

                if (pttMode == null)
                    return "Invalid mic mode! Use open, ptt, or ptm.";

                BaseGameInterface.SetPttMode(pttMode.Value);
                return $"Updated mic mode: {pttMode}";
            }));

            _commandHandler.AddCommand(new Command("voice", new[] { typeof(string) }, args => {
                var value = ((string)args[0]).ToLower();
                if (value is not ("on" or "off"))
                    return "Invalid value! Use on or off.";

                BaseGameInterface.SetVoiceMode(value == "on");
                return $"Updated voice: {value}";
            }));

            _commandHandler.AddCommand(new Command("items", new[] { typeof(string) }, args => {
                var value = ((string)args[0]).ToLower();
                if (value is not ("on" or "off"))
                    return "Invalid value! Use on or off.";

                BaseGameInterface.SetItemMode(value == "off");
                return $"Updated items: {value}";
            }));

            _commandHandler.AddCommand(new Command("volume", new[] { typeof(int) }, args => {
                var value = Mathf.Clamp((int)args[0], 0, 50);
                BaseGameInterface.SetInstrumentVolume(value);
                return $"Updated volume: {value}";
            }));

            _commandHandler.AddCommand(new Command("getroom", null, args => {
                var room = BaseGameInterface.GetRoomCode();
                return room != null ? $"Room: {room}" : "Not currently in a room.";
            }));

            _commandHandler.AddCommand(new Command("whoami", null, args => {
                BaseGameInterface.GetColor(out var r, out var g, out var b);
                var room = BaseGameInterface.GetRoomCode();

                var sb = new StringBuilder();
                sb.AppendLine($"Name: {BaseGameInterface.GetName()}  Room: {room ?? "none"}");
                sb.AppendLine($"Color: R:{Mathf.RoundToInt(r * 255f)} G:{Mathf.RoundToInt(g * 255f)} B:{Mathf.RoundToInt(b * 255f)}");
                sb.AppendLine($"Turn: {BaseGameInterface.GetTurnMode()} ({BaseGameInterface.GetTurnValue()})  Mic: {BaseGameInterface.GetPttMode()}");
                sb.AppendLine($"Volume: {BaseGameInterface.GetInstrumentVolume() * 50f:0}  Voice: {(BaseGameInterface.GetVoiceMode() ? "on" : "off")}  Items: {(BaseGameInterface.GetItemMode() ? "off" : "on")}");
                sb.Append($"Queue: {BaseGameInterface.GetQueue()}");

                return sb.ToString();
            }));

            _commandHandler.AddCommand(new Command("help", null, args =>
                "type "help" to view the full, paginated command list."));
        }
    }
}