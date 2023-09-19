using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot {

    private const int StartDepth = 3;
    private const int TimeUsage = 10;
    private readonly int[] _pieceValues = { 0, 1, 3, 3, 5, 9, 999 };
    private readonly Dictionary<ulong, (double, Move)> _cache = new();

    // TODO Rewrite Caching (fun) abc-bca kompilieren, donn geat des. hon i gheart.. hot mo a bekonnto.. a vögelein getschwitzert

    public Move Think(Board board, Timer timer) {
        return ActualWorkingChessEngineLoL(board, timer);
    }

    // White Only
    private Move FriedLiverTrap() {
        return Move.NullMove;
    }

    // Black Only
    private Move BuschGassTrap() {
        return Move.NullMove;
    }

    private Move ActualWorkingChessEngineLoL(Board board, Timer timer) {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        (double lastScore, Move lastMove) = (0, Move.NullMove);
        for (int depth = StartDepth; ; depth++) {
            _cache.Clear();

            // TODO Rework Time Control (currently not efficient)
            // TODO - look at time used => enough to calculate next depth?
            // TODO - rewrite MinMax, return boolean for time run out => to get currently best move that got evaluated
            (double score, Move move) = MinMax(board, timer, depth,
                Math.Min(2000, timer.IncrementMilliseconds + (timer.MillisecondsRemaining / TimeUsage)),
                double.NegativeInfinity, double.PositiveInfinity);

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / TimeUsage) {
                watch.Stop();
                Console.WriteLine("[" + (depth - 1) + "]\tTime " + watch.ElapsedMilliseconds + "ms\tWill get Score " + lastScore +
                                  " by playing move: " + lastMove);

                return lastMove;
            }

            lastMove = move;
            lastScore = score;
        }
    }

    private (double, Move) MinMax(Board board, Timer timer, int depth, long maxTime, double alpha, double beta) {
        if (_cache.TryGetValue(board.ZobristKey, out (double, Move) result)) {
            return result;
        }

        Move[] legalMoves = board.GetLegalMoves();
        if (legalMoves.Length == 0) {
            return (board.IsInCheckmate() ? double.NegativeInfinity : 0, Move.NullMove);
        }

        if (depth == 0 || timer.MillisecondsElapsedThisTurn >= maxTime) {
            return (EvaluateBoard(board), Move.NullMove);
        }

        Move bestMove = Move.NullMove;

        Array.Sort(legalMoves, (a, b) => MoveOrder(a, b, board));

        foreach (Move move in legalMoves) {
            board.MakeMove(move);
            (double score, Move _) = MinMax(board, timer, depth - 1, maxTime, -beta, -alpha);
            score = -score; // TODO Refactor This
            board.UndoMove(move);

            if (score >= beta) {
                return (beta, move);
            }

            bestMove = alpha > score ? move : bestMove;
            alpha = alpha > score ? score : alpha;
        }

        _cache[board.ZobristKey] = (alpha, bestMove);

        return (alpha, bestMove);
    }

    private double EvaluateBoard(Board board) {
        double eval = 0;

        foreach (PieceList list in board.GetAllPieceLists()) {
            eval += (list.IsWhitePieceList ? 1 : -1) * (_pieceValues[(int) list.TypeOfPieceInList] * list.Count);
        }

        // TODO: Checkmate and Checks
        return (board.IsWhiteToMove ? 1 : -1) * eval;
    }

    private int MoveOrder(Move a, Move b, Board board) {
        int weight = 0;

        // + Taking moves
        weight += _pieceValues[(int)a.CapturePieceType] - _pieceValues[(int)b.CapturePieceType];

        // + Promote pawn
        weight += _pieceValues[(int)a.PromotionPieceType] - _pieceValues[(int)b.PromotionPieceType];

        // - Gifting piece
        weight -= (board.SquareIsAttackedByOpponent(a.TargetSquare) ? _pieceValues[(int)a.MovePieceType] : 0 )
                  - (board.SquareIsAttackedByOpponent(b.TargetSquare) ? _pieceValues[(int)b.MovePieceType] : 0 );

        // TODO + Check move
        // TODO + Develop piece
        // TODO Positional awareness
        return -weight; // Invert because of ascending order
    }
}