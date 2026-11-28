using System;
using System.Collections.Generic;
using ChessGame.Core.Models.Pieces;

namespace ChessGame.Core.Models.Board;

public class ChessBoard : IBoardState
{
    private readonly Piece?[,] _cells = new Piece?[8, 8];

    public PieceColor ActiveColor { get; set; } = PieceColor.White;
    public PieceColor UpwardColor { get; set; } = PieceColor.White;

    public bool WhiteCanCastleShort { get; set; } = true;
    public bool WhiteCanCastleLong { get; set; } = true;
    public bool BlackCanCastleShort { get; set; } = true;
    public bool BlackCanCastleLong { get; set; } = true;

    public Position? EnPassantTarget { get; set; }
    public int HalfMoveClock { get; set; }
    public int FullMoveNumber { get; set; } = 1;

    public Piece? this[int row, int col]
    {
        get
        {
            if (IsInside(row, col))
                return _cells[row, col];
            return null;
        }
        set
        {
            if (IsInside(row, col))
                _cells[row, col] = value;
        }
    }

    public Piece? this[Position pos]
    {
        get => this[pos.Row, pos.Col];
        set => this[pos.Row, pos.Col] = value;
    }

    public Piece? GetPieceAt(Position position)
    {
        return this[position];
    }

    public static bool IsInside(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    public static bool IsInside(Position pos)
    {
        return IsInside(pos.Row, pos.Col);
    }

    public void SetupStandardPosition(PieceColor upwardColor = PieceColor.White)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                _cells[r, c] = null;
            }
        }

        UpwardColor = upwardColor;
        ActiveColor = PieceColor.White;
        WhiteCanCastleShort = true;
        WhiteCanCastleLong = true;
        BlackCanCastleShort = true;
        BlackCanCastleLong = true;
        EnPassantTarget = null;
        HalfMoveClock = 0;
        FullMoveNumber = 1;

        PieceColor bottomColor = upwardColor;
        PieceColor topColor = upwardColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

        SetupMainRow(7, bottomColor);
        SetupPawnsRow(6, bottomColor);

        SetupMainRow(0, topColor);
        SetupPawnsRow(1, topColor);
    }

    private void SetupMainRow(int row, PieceColor color)
    {
        bool flipped = UpwardColor == PieceColor.Black;
        int queenCol = flipped ? 4 : 3;
        int kingCol = flipped ? 3 : 4;

        _cells[row, 0] = new Rook(color, new Position(row, 0));
        _cells[row, 1] = new Knight(color, new Position(row, 1));
        _cells[row, 2] = new Bishop(color, new Position(row, 2));
        _cells[row, queenCol] = new Queen(color, new Position(row, queenCol));
        _cells[row, kingCol] = new King(color, new Position(row, kingCol));
        _cells[row, 5] = new Bishop(color, new Position(row, 5));
        _cells[row, 6] = new Knight(color, new Position(row, 6));
        _cells[row, 7] = new Rook(color, new Position(row, 7));
    }

    private void SetupPawnsRow(int row, PieceColor color)
    {
        for (int col = 0; col < 8; col++)
        {
            _cells[row, col] = new Pawn(color, new Position(row, col));
        }
    }

    public void ApplyMove(Move move)
    {
        Piece? piece = this[move.From];
        if (piece == null)
            throw new Exception("На клетке нет фигуры");

        Piece? targetPiece = this[move.To];
        bool isPawn = piece is Pawn;
        bool isCapture = targetPiece != null;

        if (isPawn && EnPassantTarget != null && move.To == EnPassantTarget.Value && targetPiece == null)
        {
            this[move.From.Row, move.To.Col] = null;
            isCapture = true;
        }

        this[move.From] = null;

        Piece placedPiece = piece;
        if (isPawn && (move.To.Row == 0 || move.To.Row == 7))
        {
            placedPiece = new Queen(piece.Color, move.To);
        }

        this[move.To] = placedPiece;

        if (piece is King && Math.Abs(move.To.Col - move.From.Col) == 2)
        {
            bool movedRight = move.To.Col > move.From.Col;
            int rookOldCol = movedRight ? 7 : 0;
            int rookNewCol = movedRight ? move.To.Col - 1 : move.To.Col + 1;

            Piece? rook = this[move.From.Row, rookOldCol];
            if (rook != null)
            {
                this[move.From.Row, rookOldCol] = null;
                this[move.From.Row, rookNewCol] = new Rook(rook.Color, new Position(move.From.Row, rookNewCol));
            }
        }

        if (isPawn && Math.Abs(move.To.Row - move.From.Row) == 2)
        {
            int middleRow = (move.From.Row + move.To.Row) / 2;
            EnPassantTarget = new Position(middleRow, move.From.Col);
        }
        else
        {
            EnPassantTarget = null;
        }

        UpdateCastlingFlags(piece, move.From, targetPiece, move.To);

        if (isPawn || isCapture)
            HalfMoveClock = 0;
        else
            HalfMoveClock++;

        if (ActiveColor == PieceColor.Black)
            FullMoveNumber++;

        ActiveColor = ActiveColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private void UpdateCastlingFlags(Piece piece, Position from, Piece? captured, Position to)
    {
        if (piece is King)
        {
            if (piece.Color == PieceColor.White)
            {
                WhiteCanCastleShort = false;
                WhiteCanCastleLong = false;
            }
            else
            {
                BlackCanCastleShort = false;
                BlackCanCastleLong = false;
            }
        }

        CheckRookMovedOrCaptured(from);
        if (captured is Rook)
        {
            CheckRookMovedOrCaptured(to);
        }
    }

    private void CheckRookMovedOrCaptured(Position pos)
    {
        int whiteRow = UpwardColor == PieceColor.White ? 7 : 0;
        int blackRow = UpwardColor == PieceColor.White ? 0 : 7;
        int shortCol = UpwardColor == PieceColor.White ? 7 : 0;
        int longCol = UpwardColor == PieceColor.White ? 0 : 7;

        if (pos.Row == whiteRow && pos.Col == shortCol)
            WhiteCanCastleShort = false;
        if (pos.Row == whiteRow && pos.Col == longCol)
            WhiteCanCastleLong = false;
        if (pos.Row == blackRow && pos.Col == shortCol)
            BlackCanCastleShort = false;
        if (pos.Row == blackRow && pos.Col == longCol)
            BlackCanCastleLong = false;
    }

    public Position? FindKing(PieceColor color)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (_cells[r, c] is King && _cells[r, c]!.Color == color)
                {
                    return new Position(r, c);
                }
            }
        }
        return null;
    }

    public List<(Position Pos, Piece Piece)> GetPieces(PieceColor? color = null)
    {
        var list = new List<(Position Pos, Piece Piece)>();

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Piece? piece = _cells[r, c];
                if (piece != null)
                {
                    if (color == null || piece.Color == color.Value)
                    {
                        list.Add((new Position(r, c), piece));
                    }
                }
            }
        }

        return list;
    }

    public ChessBoard Clone()
    {
        var copy = new ChessBoard();
        copy.ActiveColor = ActiveColor;
        copy.UpwardColor = UpwardColor;
        copy.WhiteCanCastleShort = WhiteCanCastleShort;
        copy.WhiteCanCastleLong = WhiteCanCastleLong;
        copy.BlackCanCastleShort = BlackCanCastleShort;
        copy.BlackCanCastleLong = BlackCanCastleLong;
        copy.EnPassantTarget = EnPassantTarget;
        copy.HalfMoveClock = HalfMoveClock;
        copy.FullMoveNumber = FullMoveNumber;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Piece? piece = _cells[r, c];
                if (piece != null)
                {
                    copy._cells[r, c] = piece.Clone();
                }
            }
        }

        return copy;
    }
}