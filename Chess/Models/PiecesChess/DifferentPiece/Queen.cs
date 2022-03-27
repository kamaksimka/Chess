﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Chess.Models.PiecesChess.Base;

namespace Chess.Models.PiecesChess.DifferentPiece
{
    internal abstract class Queen:Piece
    {
        protected Queen(string icon, TeamEnum team) : base(icon, team)
        {
        }

        private MoveInfo? IsMove(Point startPoint, Point endPoint,Board board)
        {
            var xChange = endPoint.X - startPoint.X;
            var yChange = endPoint.Y - startPoint.Y;

            if (((xChange == 0 && yChange != 0 ) ||
                (xChange != 0 && yChange == 0) ||
                (Math.Abs(xChange) == Math.Abs(yChange))) && 
                board[endPoint.X, endPoint.Y]?.Team != Team &&
                board.CheckIsEmptySells(MovePieces.GetStraightTrajectory(startPoint, endPoint)))
            {
                MoveInfo moveInfo = new MoveInfo
                {
                    ChangePositions = new List<ChangePosition> { new ChangePosition(startPoint, endPoint) }
                };
                if (board[endPoint.X, endPoint.Y] != null)
                {
                    moveInfo.KillPoint = endPoint;
                }
                return moveInfo;
            }
            
            return null;
        }

        public override MoveInfo? Move(Point startPoint, Point endPoint, Board board)
        {
            if (IsMove(startPoint, endPoint,board) is {} moveInfoIsMove)
            {
                return moveInfoIsMove;
            }
            return null;
        }
    }
}
