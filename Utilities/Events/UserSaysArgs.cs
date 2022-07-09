﻿using Wibbo.Game.Rooms;

namespace Wibbo.Utilities.Events
{
    public class UserSaysArgs : EventArgs
    {
        public readonly RoomUser User;
        public readonly string Message;

        public UserSaysArgs(RoomUser user, string message)
        {
            this.User = user;
            this.Message = message;
        }
    }
}
