﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Chess.Models.PiecesChess;
using Chess.Models.PiecesChess.Base;
using Chess.Models.PiecesChess.DifferentPiece;

namespace Chess.Models.Boards.Base
{
    internal class ChessBoard:Board
    {
        public event Action<MoveInfo>? ChessBoardMovedEvent;
        public event Action<List<Piece>,Point>? ChoiceReplacementPieceEvent;
        public event Action<TeamEnum?>? EndGameEvent; 

        private MoveInfo? _moveInfoForReplacePiece;
        public ChessBoard()
        {
            
        }
        public ChessBoard(Piece?[,] arrayBoard) :base(arrayBoard)
        {
            
        }
        public static Dictionary<(Point, Point), MoveInfo> GetMovesForPiece(Point? startPoint, Board board)
        {
            var goodMoves = new Dictionary<(Point, Point), MoveInfo>();
            if (startPoint is { X: <= 7 and >= 0, Y: <= 7 and >= 0 } startP &&
                board[startP.X, startP.Y] is { } piece && piece.Team == board.WhoseMove)
            {
                var moves = piece.GetMoves(startP, board);
                foreach (var move in moves)
                {
                    if (board.Clone() is Board boardClone)
                    {
                        Board.Move(move.Value,boardClone);
                        if (!IsCheck(boardClone,board.WhoseMove))
                        {
                            goodMoves.Add(move.Key,move.Value);
                        }
                    }
                }
            }

            return goodMoves;
        }
        public static bool IsCheck(Board board,TeamEnum team)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] is King king && king.Team == team)
                    {
                        return IsCellForKill(new Point(i, j), king.Team,board);
                    }
                }
            }

            return false;
        }
        public static bool IsNoMoves(Board board)
        {
            for (byte i = 0; i < 8; i++)
            {
                for (byte j = 0; j < 8; j++)
                {
                    var movesPiece = ChessBoard.GetMovesForPiece(new Point(i, j), board);
                    if (movesPiece.Count > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void CheckEndGame()
        {
            if (IsNoMoves(this))
            {
                if (IsCheck(this,WhoseMove))
                {
                    EndGameEvent?.Invoke(WhoseMove);
                }
                else
                {
                    EndGameEvent?.Invoke(null);
                }
            }
        }
        public MoveInfo GetMoveInfo(Point startPoint, Point endPoint)
        {
            var movesForPiece = GetMovesForPiece(startPoint,this);
            if (movesForPiece.ContainsKey((startPoint, endPoint)))
            {
                return movesForPiece[(startPoint, endPoint)];
            }
            return new MoveInfo
            {
                IsMoved = false,
                Move = new ChangePosition(startPoint, endPoint)
            };
        }
        public void Moved(MoveInfo moveInfo)
        {
            Task.Run(() => ChessBoardMovedEvent?.Invoke(moveInfo));
            CheckEndGame();
        }
        public void SetReplasementPiece(Piece? piece)
        {
            if (_moveInfoForReplacePiece is { ReplaceImg: { } replaceImg } moveInfo)
            {
                if (piece is { })
                {
                    _moveInfoForReplacePiece = null;
                    moveInfo.ReplaceImg = (replaceImg.Item1, piece);
                    Board.Move(moveInfo, this);
                    Moved(moveInfo);
                }
                else
                {
                    _moveInfoForReplacePiece = null;
                    var nullMoveInfo = new MoveInfo
                    {
                        IsMoved = false,
                        Move = moveInfo.Move,
                    };
                    Board.Move(nullMoveInfo, this);
                    Moved(nullMoveInfo);
                }
            }

        }
        public void Move(Point startPoint, Point endPoint)
        {
            var moveInfo = GetMoveInfo(startPoint, endPoint);
            if (moveInfo.IsReplacePiece && moveInfo.ReplaceImg is {Item2:null} replaceImg &&
                ArrayBoard[startPoint.X, startPoint.Y] is {} piece)
            {
                ChoiceReplacementPieceEvent?.Invoke(piece.ReplacementPieces, replaceImg.Item1);
                _moveInfoForReplacePiece = moveInfo;
            }
            else
            {
                Board.Move(moveInfo, this);
                Moved(moveInfo);
            }
           
            
        }
         
        public override object Clone()
        {
            return new ChessBoard((Piece?[,])ArrayBoard.Clone()) 
            {
                WhoseMove = WhoseMove,
                LastMoveInfo = LastMoveInfo,
                ChessBoardMovedEvent = ChessBoardMovedEvent,
                Price = Price,
                ChoiceReplacementPieceEvent = ChoiceReplacementPieceEvent,
                EndGameEvent = EndGameEvent
                
            };
        }
    }


    public class Board:ICloneable
    {
        protected readonly Piece?[,] ArrayBoard;
        public double Price { get; set; }
        public Piece? this[int i, int j]
        {
            get => ArrayBoard[i,j];
            protected set => ArrayBoard[i,j] = value;
        }
        public TeamEnum WhoseMove { get;protected set; } = TeamEnum.WhiteTeam;
        public MoveInfo LastMoveInfo { get; set; } = new MoveInfo();
        public Board()
        {
            ArrayBoard = GetNewClassicBoard();
        }
        public Board(Piece?[,] arrayBoard)
        {
            ArrayBoard = arrayBoard;
        }
        private static Piece?[,] GetNewClassicBoard()
        {
            Piece?[,] board = new Piece?[8,8];

            #region Создание белой команды

            /*for (int i = 0; i < 8; i++)
            {
                board[1, i] = new WhitePawn(PawnDirection.Up);
            }

            board[0, 0] = new WhiteRook();
            board[0, 7] = new WhiteRook();
            board[0, 1] = new WhiteKnight();
            board[0, 6] = new WhiteKnight();
            board[0, 2] = new WhiteBishop();
            board[0, 5] = new WhiteBishop();
            board[0, 3] = new WhiteKing();
            board[0, 4] = new WhiteQueen();*/

            for (int i = 0; i < 8; i++)
            {
                board[6, i] = new WhitePawn(PawnDirection.Down);
            }

            board[7, 0] = new WhiteRook();
            board[7, 7] = new WhiteRook();
            board[7, 1] = new WhiteKnight();
            board[7, 6] = new WhiteKnight();
            board[7, 2] = new WhiteBishop();
            board[7, 5] = new WhiteBishop();
            board[7, 4] = new WhiteKing();
            board[7, 3] = new WhiteQueen();

            #endregion

            #region Создание черной команды

            /* for (int i = 0; i < 8; i++)
             {
                 board[6, i] = new BlackPawn(PawnDirection.Down);
             }

             board[7, 0] = new BlackRook();
             board[7, 7] = new BlackRook();
             board[7, 1] = new BlackKnight();
             board[7, 6] = new BlackKnight();
             board[7, 2] = new BlackBishop();
             board[7, 5] = new BlackBishop();
             board[7, 4] = new BlackQueen();
             board[7, 3] = new BlackKing();*/

            for (int i = 0; i < 8; i++)
            {
                board[1, i] = new BlackPawn(PawnDirection.Up);
            }

            board[0, 0] = new BlackRook();
            board[0, 7] = new BlackRook();
            board[0, 1] = new BlackKnight();
            board[0, 6] = new BlackKnight();
            board[0, 2] = new BlackBishop();
            board[0, 5] = new BlackBishop();
            board[0, 3] = new BlackQueen();
            board[0, 4] = new BlackKing();

            #endregion

            return board;
        }
        public bool CheckIsEmptySells(IEnumerable<Point>? points)
        {
            if (points is { })
            {
                foreach (var point in points)
                {
                    if (ArrayBoard[point.X, point.Y] is not null)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }

        }
        public static bool IsCellForKill(Point checkPoint,TeamEnum team,Board board)
        {
            
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (board[i, j] is { } piece && piece.Team !=team)
                        {
                            var movesForPiece = board[i, j]?.GetMoves(new Point(i, j), board);
                            if (movesForPiece != null)
                            {
                                foreach (var (_, moveInfoPiece) in movesForPiece)
                                {
                                    if (moveInfoPiece.KillPoint is { } killPoint && killPoint == checkPoint)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
        }
        public static void Move(MoveInfo moveInfo, Board board)
        {
            if (moveInfo.IsMoved)
            {
                if (moveInfo.KillPoint is { } killPoint)
                {
                    #region Пересчет цены доски при убийстве

                    if (board[killPoint.X, killPoint.Y] is { } pieceKill)
                    {
                        board.Price -= pieceKill.Price;
                        board.Price -= pieceKill.PieceEval[killPoint.X, killPoint.Y];
                    }

                    #endregion

                    board[killPoint.X, killPoint.Y] = null;
                }

                if (moveInfo.ChangePositions is { } changePositions)
                {
                    foreach (var (startP, endP) in changePositions)
                    {

                        #region Пересчет цены доски при перемещении

                        if (board[startP.X, startP.Y] is { } pieceMove)
                        {
                            board.Price -= pieceMove.PieceEval[startP.X, startP.Y];
                            board.Price += pieceMove.PieceEval[endP.X, endP.Y];
                        }

                        #endregion

                        board[endP.X, endP.Y] = board[startP.X, startP.Y];
                        board[startP.X, startP.Y] = null;
                        
                        if (board[endP.X, endP.Y] is {IsFirstMove:true } piece)
                        {
                            board[endP.X, endP.Y] = FactoryPiece.GetMovedPiece(piece);
                        }

                        board.LastMoveInfo = moveInfo;
                    }
                }

                if (moveInfo.ReplaceImg is { Item2:{}} replaceImg)
                {
                    #region Пересчет цены доски при замене

                    if (board[replaceImg.Item1.X, replaceImg.Item1.Y] is { } piece)
                    {
                        board.Price -= piece.Price;
                        board.Price -= piece.PieceEval[replaceImg.Item1.X, replaceImg.Item1.Y];
                        board.Price += replaceImg.Item2.Price;
                        board.Price += replaceImg.Item2.PieceEval[replaceImg.Item1.X, replaceImg.Item1.Y];
                    }

                    #endregion

                    board[replaceImg.Item1.X, replaceImg.Item1.Y] = FactoryPiece.GetMovedPiece(replaceImg.Item2) ;
                }
                board.WhoseMove = board.WhoseMove == TeamEnum.WhiteTeam ? TeamEnum.BlackTeam : TeamEnum.WhiteTeam;
            }
        }
        public IHaveIcon?[,] GetIcons()
        {
            return (IHaveIcon?[,])ArrayBoard.Clone();
        }
        public virtual object Clone()
        {
            return new Board((Piece?[,])ArrayBoard.Clone()){LastMoveInfo = LastMoveInfo,Price = Price};
        }

    }

}
