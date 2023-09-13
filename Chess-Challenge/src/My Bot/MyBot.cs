using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class MyBot : IChessBot {

    private const int MaxDepth = 5;
    private readonly double[] PieceValues = {
        0, 1, 3, 3, 5, 9, 0
    };
    private Dictionary<ulong, (double, List<Move>)> cache;

    public MyBot() {
        cache = new Dictionary<ulong, (double, List<Move>)>();
    }

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
        (double score, List<Move> moves) = MinMax(board, timer, 0, long.MaxValue);
        Console.Write("Got Score " + score + " by playing move: ");
        moves.Reverse();
        moves.ForEach(move => Console.Write(move + " "));
        Console.WriteLine();
        return moves[0];
    }

    private (double, List<Move>) MinMax(Board board, Timer timer, int depth, long maxTime) {

        if (depth >= MaxDepth || timer.MillisecondsElapsedThisTurn >= maxTime) { 
            return (EvaluateBoard(board), new List<Move>()); 
        }

        bool isWhiteToMove = board.IsWhiteToMove;
        double bestScore = double.NaN;
        Move bestMove = Move.NullMove;
        Move[] legalMoves = board.GetLegalMoves();
        List<Move> bestLine = new List<Move>();

        Array.Sort(legalMoves, (a, b) => {
            return (-1) * a.GetHashCode().CompareTo(b.GetHashCode());
        });

        foreach (Move move in legalMoves) {
            board.MakeMove(move);
            
            (double score, List< Move > line) = this.cache.GetValueOrDefault(board.ZobristKey, (double.NaN, new List<Move>()));
            if (double.IsNaN(score)) {
                (score, line) = MinMax(board, timer, depth + 1, maxTime);
                cache[board.ZobristKey] = (score, line);
            }

            if (Double.IsNaN(bestScore) || (isWhiteToMove && bestScore < score) || (!isWhiteToMove && bestScore > score)) {
                bestScore = score;
                bestLine = line;
                bestMove = move;
            }

            board.UndoMove(move);
        }
        bestLine.Add(bestMove);

        return (bestScore, bestLine);
    }

    private double EvaluateBoard(Board board) {
        double eval = 0;

        foreach (PieceList list in board.GetAllPieceLists()) {
            eval += (list.IsWhitePieceList ? 1 : -1) * (PieceValues[(int) list.TypeOfPieceInList] * list.Count);
        }
        // TODO: Checkmate and Checks
        return board.IsInCheckmate() ? double.MaxValue : eval;
    }
}