using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot {

    private const int MaxDepth = 5;
    private readonly int[] _pieceValues = { 0, 1, 3, 3, 5, 9, 0 };
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

        var watch = System.Diagnostics.Stopwatch.StartNew();
        (double score, Move move) = MinMax(board, timer, 0, long.MaxValue, double.NegativeInfinity, double.PositiveInfinity);
        watch.Stop();
        Console.WriteLine("Time " + watch.ElapsedMilliseconds + "ms\tWill get Score " + score + " by playing move: " + move);

        return move;
    }

    private (double, Move) MinMax(Board board, Timer timer, int depth, long maxTime, double alpha, double beta) {
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

        Move bestMove = Move.NullMove;

        Array.Sort(legalMoves, (a, b) => MoveOrder(a, b, board));

        foreach (Move move in legalMoves) {
            board.MakeMove(move);
            (double score, Move debug) = MinMax(board, timer, depth + 1, maxTime, -beta, -alpha);
            score = -score; // TODO Refactor This
            board.UndoMove(move);

            if (score >= beta) {
                return (beta, bestMove);
            }

            bestMove = alpha >= score ? bestMove : move;
            alpha = alpha >= score ? alpha : score;
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