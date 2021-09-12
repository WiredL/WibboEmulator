using Butterfly.HabboHotel.GameClients;using System;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd{    internal class LoadVideo : IChatCommand    {        public string PermissionRequired
        {
            get { return ""; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return ""; }
        }        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)        {            if (Params.Length != 2)            {                UserRoom.LoaderVideoId = "";                return;            }            string Url = Params[1];            if (string.IsNullOrEmpty(Url) || (!Url.Contains("?v=") && !Url.Contains("youtu.be/"))) //https://youtu.be/_mNig3ZxYbM
            {                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.loadvideo.incorrect", Session.Langue));                UserRoom.LoaderVideoId = "";                return;            }            string Split = "";            if (Url.Contains("?v="))            {                Split = Url.Split(new string[] { "?v=" }, StringSplitOptions.None)[1];            }            else if (Url.Contains("youtu.be/"))            {                Split = Url.Split(new string[] { "youtu.be/" }, StringSplitOptions.None)[1];            }            if (Split.Length < 11)            {                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.loadvideo.incorrect", Session.Langue));                return;            }            UserRoom.LoaderVideoId = Split.Substring(0, 11);            UserRoom.SendWhisperChat(string.Format(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.loadvideo", Session.Langue), UserRoom.LoaderVideoId));        }    }}