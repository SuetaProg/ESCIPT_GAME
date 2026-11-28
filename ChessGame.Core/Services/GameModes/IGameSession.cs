using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessGame.Core.Models;

namespace ChessGame.Core.Services.GameModes;

public interface IGameSession : IDisposable
{
    PieceColor? LocalPlayerColor { get; }
    bool IsMyTurn { get; }

    event EventHandler<Move>? MoveReceived;
    event EventHandler<string>? ConnectionClosed;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task<bool> TryMakeMoveAsync(Move move, CancellationToken cancellationToken = default);
}
