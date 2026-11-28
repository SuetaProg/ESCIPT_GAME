using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Core.Models;

public enum PieceColor {White,Black}

public enum PieceType {Pawn,Rook,Knight,Bishop,Queen,King}

public enum GameStatus {InProgress,Check,Checkmate,Stalemate,Draw}

public static class PieceColorExtensions
{
    public static PieceColor Opponent(this PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;
}

public readonly record struct Position(int Row, int Col)
{
    public bool IsValid => Row is >= 0 and < 8 && Col is >= 0 and < 8;

    public string ToAlgebraic()
    {
        if (!IsValid)
            return string.Empty;

        char file = (char)('A' + Col);
        char rank = (char)('8' - Row);
        return $"{file}{rank}";
    }

    public static bool TryFromAlgebraic(string? notation, out Position position)
    {
        position = default;

        if (string.IsNullOrWhiteSpace(notation) || notation.Length != 2)
            return false;

        char file = char.ToUpperInvariant(notation[0]);
        char rank = notation[1];

        if (file is < 'A' or > 'H' || rank is < '1' or > '8')
            return false;

        int col = file - 'A';
        int row = '8' - rank;

        position = new Position(row, col);
        return true;
    }

    public static Position FromAlgebraic(string notation)
    {
        if (!TryFromAlgebraic(notation, out Position position))
            throw new ArgumentException($"Некорректная шахматная координата: '{notation}'.", nameof(notation));

        return position;
    }
}