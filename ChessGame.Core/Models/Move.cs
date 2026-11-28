using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChessGame.Core.Models;

public readonly record struct Move(
    Position From,
    Position To,
    PieceType? PromotionPiece = null)
{
    public string FromNotation => From.ToAlgebraic();
    public string ToNotation => To.ToAlgebraic();

    public static bool TryFromNotation(
        string? fromNotation,
        string? toNotation,
        out Move move,
        PieceType? promotionPiece = null)
    {
        move = default;

        if (!Position.TryFromAlgebraic(fromNotation, out var from) ||
            !Position.TryFromAlgebraic(toNotation, out var to))
        {
            return false;
        }

        move = new Move(from, to, promotionPiece);
        return true;
    }
}