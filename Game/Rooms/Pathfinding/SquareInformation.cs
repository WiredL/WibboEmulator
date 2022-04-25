﻿namespace Butterfly.Game.Rooms.PathFinding
{
    public struct SquareInformation
    {
        private readonly SquarePoint[] _pos;
        private readonly SquarePoint _point;

        public SquareInformation(int pX, int pY, SquarePoint pTarget, ModelInfo pMap, bool pUserOverride, bool CalculateDiagonal, bool pAllowWalkthrough, bool DisableOblique)
        {
            this._point = new SquarePoint(pX, pY, pTarget.X, pTarget.Y, pMap.GetState(pX, pY), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX, pY));
            this._pos = new SquarePoint[8];

            if (CalculateDiagonal || pUserOverride)
            {
                this._pos[1] = new SquarePoint(pX - 1, pY - 1, pTarget.X, pTarget.Y, pMap.GetState(pX - 1, pY - 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX - 1, pY - 1));
                this._pos[3] = new SquarePoint(pX - 1, pY + 1, pTarget.X, pTarget.Y, pMap.GetState(pX - 1, pY + 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX - 1, pY + 1));
                this._pos[5] = new SquarePoint(pX + 1, pY + 1, pTarget.X, pTarget.Y, pMap.GetState(pX + 1, pY + 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX + 1, pY + 1));
                this._pos[7] = new SquarePoint(pX + 1, pY - 1, pTarget.X, pTarget.Y, pMap.GetState(pX + 1, pY - 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX + 1, pY - 1));
            }

            if (DisableOblique || pUserOverride)
            {
                this._pos[0] = new SquarePoint(pX, pY - 1, pTarget.X, pTarget.Y, pMap.GetState(pX, pY - 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX, pY - 1));
                this._pos[2] = new SquarePoint(pX - 1, pY, pTarget.X, pTarget.Y, pMap.GetState(pX - 1, pY), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX - 1, pY));
                this._pos[4] = new SquarePoint(pX, pY + 1, pTarget.X, pTarget.Y, pMap.GetState(pX, pY + 1), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX, pY + 1));
                this._pos[6] = new SquarePoint(pX + 1, pY, pTarget.X, pTarget.Y, pMap.GetState(pX + 1, pY), pUserOverride, pAllowWalkthrough, pMap.GetStateUser(pX + 1, pY));
            }
        }

        public SquarePoint Point => this._point;

        public SquarePoint Pos(int val)
        {
            return this._pos[val];
        }
    }
}
