using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Core.Models.Pieces;

public interface IBoardState
{
    PieceColor UpwardColor { get; }
    Piece? GetPieceAt(Position position);
}

public abstract class Piece
{
    protected Piece(PieceType type, PieceColor color, Position position)
    {
        Type = type;
        Color = color;
        Position = position;
    }

    public PieceType Type { get; }
    public PieceColor Color { get; }
    public Position Position { get; set; }
    public bool HasMoved { get; set; }
    public abstract Piece Clone();
    public abstract IEnumerable<Position> GetPseudoLegalMoves(IBoardState board);

    protected IEnumerable<Position> GetRayMoves(IBoardState board, (int dRow, int dCol)[] directions)
    {
        foreach (var (dRow, dCol) in directions)
        {
            int row = Position.Row + dRow;
            int col = Position.Col + dCol;

            while (true)
            {
                var next = new Position(row, col);
                if (!next.IsValid)
                    break;

                var occupant = board.GetPieceAt(next);
                if (occupant is null)
                {
                    yield return next;
                }
                else
                {
                    if (occupant.Color != Color)
                        yield return next;

                    break;
                }

                row += dRow;
                col += dCol;
            }
        }
    }
}

public sealed class Rook : Piece
{
    private static readonly (int dRow, int dCol)[] Directions =
    [
        (-1, 0), (1, 0), (0, -1), (0, 1)
    ];

    public override Piece Clone() => new Rook(Color, Position);
    public Rook(PieceColor color, Position position)
        : base(PieceType.Rook, color, position) { }

    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board) =>
        GetRayMoves(board, Directions);
}

public sealed class Bishop : Piece
{
    private static readonly (int dRow, int dCol)[] Directions =
    [
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];

    public override Piece Clone() => new Bishop(Color, Position);
    public Bishop(PieceColor color, Position position)
        : base(PieceType.Bishop, color, position) { }

    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board) =>
        GetRayMoves(board, Directions);
}

public sealed class Queen : Piece
{
    private static readonly (int dRow, int dCol)[] Directions =
    [
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];
    public override Piece Clone() => new Queen(Color, Position);
    public Queen(PieceColor color, Position position)
        : base(PieceType.Queen, color, position) { }

    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board) =>
        GetRayMoves(board, Directions);
}

public sealed class Knight : Piece
{
    private static readonly (int dRow, int dCol)[] Jumps =
    [
        (-2, -1), (-2, 1), (-1, -2), (-1, 2),
        (1, -2),  (1, 2),  (2, -1),  (2, 1)
    ];
    public override Piece Clone() => new Knight(Color, Position);
    public Knight(PieceColor color, Position position)
        : base(PieceType.Knight, color, position) { }

    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board)
    {
        foreach (var (dRow, dCol) in Jumps)
        {
            var target = new Position(Position.Row + dRow, Position.Col + dCol);
            if (!target.IsValid)
                continue;

            var occupant = board.GetPieceAt(target);
            if (occupant is null || occupant.Color != Color)
                yield return target;
        }
    }
}

public sealed class King : Piece
{
    private static readonly (int dRow, int dCol)[] Steps =
    [
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];
    public override Piece Clone() => new King(Color, Position);
    public King(PieceColor color, Position position)
        : base(PieceType.King, color, position) { }

    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board)
    {
        foreach (var (dRow, dCol) in Steps)
        {
            var target = new Position(Position.Row + dRow, Position.Col + dCol);
            if (!target.IsValid)
                continue;

            var occupant = board.GetPieceAt(target);
            if (occupant is null || occupant.Color != Color)
                yield return target;
        }
    }
}

public sealed class Pawn : Piece
{
    public Pawn(PieceColor color, Position position)
        : base(PieceType.Pawn, color, position) { }

    public override Piece Clone() => new Pawn(Color, Position);
    public override IEnumerable<Position> GetPseudoLegalMoves(IBoardState board)
    {
        bool movesUp = Color == board.UpwardColor;
        int dir = movesUp ? -1 : 1;
        int startRow = movesUp ? 6 : 1;

        var oneStep = new Position(Position.Row + dir, Position.Col);
        if (oneStep.IsValid && board.GetPieceAt(oneStep) is null)
        {
            yield return oneStep;

            if (Position.Row == startRow)
            {
                var twoSteps = new Position(Position.Row + dir * 2, Position.Col);
                if (twoSteps.IsValid && board.GetPieceAt(twoSteps) is null)
                    yield return twoSteps;
            }
        }

        foreach (int dCol in new[] { -1, 1 })
        {
            var diag = new Position(Position.Row + dir, Position.Col + dCol);
            if (!diag.IsValid)
                continue;

            var targetPiece = board.GetPieceAt(diag);
            if (targetPiece is not null && targetPiece.Color != Color)
                yield return diag;
        }
    }
}