using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;

public class MyBot : IChessBot {

    private const int MaxDepth = 5;
    private readonly double[] PieceValues = {
        0, 1, 3, 3, 5, 9, 0
    };

    public Move Think(Board board, Timer timer) {
        //return board.GetLegalMoves()[0];
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
        (double score, List<Move> moves) = MinMax(board, board.IsWhiteToMove, timer, 0, long.MaxValue);
        Console.Write("Got Score " + score + " by playing move: ");
        moves.Reverse();
        moves.ForEach(move => Console.Write(move + " "));
        Console.WriteLine();
        return moves[0];
    }

    private (double, List<Move>) MinMax(Board board, bool isWhite, Timer timer, int depth, long maxTime) {
        List<Move> bestLine = new List<Move>();
        double bestScore = double.NaN;

        if (depth >= MaxDepth || timer.MillisecondsElapsedThisTurn >= maxTime) { return (EvaluateBoard(board), new List<Move>()); }

        Move[] legalMoves = board.GetLegalMoves();
        foreach (Move move in legalMoves) {
            board.MakeMove(move);
            (double score, List<Move> line) = MinMax(board, !isWhite, timer, depth + 1, maxTime);

            if (Double.IsNaN(bestScore) || (isWhite && bestScore < score) || (!isWhite && bestScore > score)) {
                bestScore = score;
                bestLine = line;
                bestLine.Add(move);
            }
            board.UndoMove(move);
        }

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