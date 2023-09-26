using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessChallenge.API;

public class MyBot : IChessBot {

    private const int StartDepth = 3;
    private const int TimeUsage = 10;
    private readonly int[] _pieceValues = { 0, 1, 3, 3, 5, 9, 999 };
    private readonly Dictionary<ulong, (double, Move)> _cache = new();

    /*
     * PAWNS
     * 0000000000000000000000000000000000000000010100101001010010100101
     * 0010100101001010000100001000100001100011000100000100001000001000
     * 0100010001010010100010000010000100000000000000000100001000000000
     * 0000000000001100011001000000000001001010001000010000100010000101
     * 0100101000001000010000010000000000000000000000000000000000000000
     *
     * KNIGHTS
     * 1101011000101101011010110101101100011010110001010000000000000000
     * 0000001010011000101100000000010000110001100010000001011010110000
     * 0100011001000010000011000011011010110000000001100100001000001100
     * 0001011010110000010001000011000110001000001101101100010100000000
     * 0001000010000010100110001101011000101101011010110101101100011010
     *
     * BISHOPS
     * 1010010010100101001010010100101001010100100100000000000000000000
     * 0000000000010010100100000000001000100001000001000001001010010000
     * 0100001000100001000001000011001010010000000001000010000100001000
     * 0001001010010000100001000010000100001000010100101001000001000000
     * 0000000000000000001100101010010010100101001010010100101001010100
     *
     * ROOKS
     * 0000000000000000000000000000000000000000000010001000010000100001
     * 0000100001000001100010000000000000000000000000000001000110001000
     * 0000000000000000000000000001000110001000000000000000000000000000
     * 0001000110001000000000000000000000000000000100011000100000000000
     * 0000000000000000000100010000000000000000000100001000000000000000
     *
     * QUEENS
     * 1010010010100101000110001100101001010100100100000000000000000000
     * 0000000000010010100100000000001000010000100001000001001010001000
     * 0000001000010000100001000001000100000000000000100001000010000100
     * 0001000110010000010000100001000010000100000100101001000000000010
     * 0000000000000000000100101010010010100101000110001100101001010100
     *
     * KINGS
     * 1111111110111101111011110111101111011111111001110011100111001110
     * 0111001110011100110001101011010111001110011010110101100010110110
     * 0011000110101101011000110001011010100101101011011000110001011010
     * 1101010010010101001010010100101001010010100100100010000100100011
     * 0001100011000100100001000010000110000100000000000000100011000100
     */

    private readonly long[]?[] _positionTable = {
        null,
        new []{ 5412005, 2975208681795240456, 4923147017984688640, 3487652126984325, 5334585226876157952 },
        new []{ -3013634536605679616, 187092916593235632, 5062622360138826252, 1634881644264867072, 1189681297231665946 },
        new []{ -6582809881109069824, 5224888399041168, 4765094495945761032, 1337714366737453120, 55682726971988 },
        new []{ 558113, 594906159370998152, 75296145408, 1263259695478573056, 18691698753536 },
        new []{ -6582828023050928128, 5224888122217096, 148763996252672132, 1265584133993828354, 20498353801812 },
        new []{ -18595508123125298, 8330751894883096758, 3579626227749456986, -3128548966949314269, 1784696631828940996 }
    };

    // TODO Rewrite Caching (fun) abc-bca kompilieren, donn geat des. hon i gheart.. hot mo a bekonnto.. a vögelein getschwitzert ~ Maxi

    public Move Think(Board board, Timer timer) {
        GetPositionValue((int)PieceType.Pawn, 1, 0, true);
        return (board.IsWhiteToMove ? WhiteOpening(board, out Move move) : BlackOpening(board, out move)) ?
            move : ActualWorkingChessEngineLoL(board, timer);
    }


    // Fried Liver
    private bool WhiteOpening(Board board, out Move move) {
        Dictionary<ulong, String> moves = new() {
            { 13227872743731781434, "e2e4" },
            { 17664629573069429839, "g1f3" },
            { 4392852280258111003 , "f1c4" },
            { 10907350113667057435, "f3g5" },
            { 12094595613019000261, "e4d5" },
            { 6149061219614480062 , "g5f7" },
            { 15765041912239346957, "d1f3" }
        };

        move = moves.TryGetValue(board.ZobristKey, out String? m) ? new Move(m, board) : Move.NullMove;
        return move != Move.NullMove;
    }

    // King Indians Defense
    private bool BlackOpening(Board board, out Move move) {
        Dictionary<ulong, String> moves = new() {
            { 15607329186585411972, "g8f6" },
            { 13920910881790336478, "g8f6" },
            { 3743624616017431465 , "f6e4" },
            { 8500792386727910725 , "g7g6" }
        };

        move = moves.TryGetValue(board.ZobristKey, out String? m) ? new Move(m, board) : Move.NullMove;
        return move != Move.NullMove;
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
                timer.MillisecondsRemaining / TimeUsage,
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
            score = -score;
            board.UndoMove(move);

            if (score >= beta) {
                return (beta, move);
            }

            bestMove = alpha < score ? move : bestMove;
            alpha = alpha < score ? score : alpha;
        }

        _cache[board.ZobristKey] = (alpha, bestMove);

        return (alpha, bestMove);
    }

    private double EvaluateBoard(Board board) {
        double eval = 0;

        foreach (PieceList list in board.GetAllPieceLists()) {
            eval += (list.IsWhitePieceList ? 1 : -1) * (_pieceValues[(int) list.TypeOfPieceInList] * list.Count);

            foreach (Piece piece in list) {
                eval += GetPositionValue((int)piece.PieceType, piece.Square.Rank, piece.Square.File, piece.IsWhite);
            }
        }

        // TODO + Bit tables for positioning

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
        // TODO Positional awareness
        return -weight; // Invert because of ascending order
    }

    private int GetPositionValue(int pieceType, int rank, int file, bool isWhite) {
        Console.WriteLine("Value Of Piece on square: " + ChessChallenge.Chess.BoardHelper.SquareNameFromIndex(rank * 8 + file));
        long[]? values = _positionTable[pieceType];
        rank = isWhite ? 7 - rank : rank; // Number
        file = isWhite ? 7 - file : file; // Character

        int index = (rank / 2);

        PrintTable(pieceType);
        Console.WriteLine(index);

        Console.WriteLine(values[index] + " - " + Convert.ToString(values[index], 2));

        return (int) values[0];
    }

    private void PrintTable(int index) {// #DEBUG
        int counter = 0;// #DEBUG

        foreach (ulong l in _positionTable[index]) {// #DEBUG
            ulong actualValue = l;// #DEBUG

            for (int i = 0; i < 64; i++) {// #DEBUG
                ulong bit = (actualValue & 0x8000000000000000) >> 63;// #DEBUG
                actualValue <<= 1;// #DEBUG

                Console.Write(Convert.ToString((long)bit, 2));// #DEBUG

                if (++counter % 5 == 0) {// #DEBUG
                    Console.Write(" ");// #DEBUG
                }// #DEBUG
                if (counter % 40 == 0) {// #DEBUG
                    Console.WriteLine();// #DEBUG
                }// #DEBUG
            }// #DEBUG
        }// #DEBUG
    }// #DEBUG

    private void ConvertTable(int[] table) { // #DEBUG
        StringBuilder builder = new();// #DEBUG

        foreach (int value in table) {// #DEBUG
            builder.Append(Convert.ToString((value < 0 ? 16 : 0) + (Math.Abs(value) / 5), 2).PadLeft(5, '0'));// #DEBUG
        }// #DEBUG

        builder.Insert(256, "\n     * ");// #DEBUG
        builder.Insert(192, "\n     * ");// #DEBUG
        builder.Insert(128, "\n     * ");// #DEBUG
        builder.Insert(64,  "\n     * ");// #DEBUG

        String[] numStrings = builder.ToString().Split("\n     * ");// #DEBUG
        builder.Insert(0,   "     * ");// #DEBUG

        Console.WriteLine(builder.ToString());// #DEBUG
        numStrings.ToList().ForEach(s => Console.Write(Convert.ToInt64(s, 2) + ", "));// #DEBUG
    } // #DEBUG
}