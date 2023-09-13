using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot {

    private const int MaxDepth = 5;
    private readonly double[] _pieceValues = {
        0, 1, 3, 3, 5, 9, 0
    };
    private readonly Dictionary<ulong, (double, Move)> _cache = new();


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
        _cache.Clear();

        (double score, Move move) = MinMax(board, timer, 0, long.MaxValue);

        Console.WriteLine("Will get Score " + score + " by playing move: " + move);

        return move;
    }

    private (double, Move) MinMax(Board board, Timer timer, int depth, long maxTime) {
        if (depth == MaxDepth || timer.MillisecondsElapsedThisTurn >= maxTime) {
            return (EvaluateBoard(board), Move.NullMove);
        }

        if (_cache.TryGetValue(board.ZobristKey, out (double, Move) result)) {
            return result;
        }

        Move[] legalMoves = board.GetLegalMoves();
        if (legalMoves.Length == 0) {
            return (board.IsInCheckmate() ? double.NegativeInfinity : 0, Move.NullMove);
        }

        double bestScore = double.NegativeInfinity;
        Move bestMove = Move.NullMove;

        Array.Sort(legalMoves, MoveOrder);

        foreach (Move move in legalMoves) {
            board.MakeMove(move);

            (double score, Move _) = MinMax(board, timer, depth + 1, maxTime);
            score = -score; // TODO Refactor This

            if (bestScore < score) {
                bestScore = score;
                bestMove = move;
            }

            board.UndoMove(move);
        }

        _cache[board.ZobristKey] = (bestScore, bestMove);

        return (bestScore, bestMove);
    }

    private double EvaluateBoard(Board board) {
        double eval = 0;

        foreach (PieceList list in board.GetAllPieceLists()) {
            eval += (list.IsWhitePieceList ? 1 : -1) * (_pieceValues[(int) list.TypeOfPieceInList] * list.Count);
        }

        // TODO: Checkmate and Checks
        return (board.IsWhiteToMove ? 1 : -1) * eval;
    }

    private int MoveOrder(Move a, Move b) {
        return (-1) * a.GetHashCode().CompareTo(b.GetHashCode());
    }
}