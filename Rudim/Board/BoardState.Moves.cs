﻿using Rudim.Common;
using System.Collections.Generic;

namespace Rudim.Board
{
    public partial class BoardState
    {
        public void GenerateMoves()
        {
            Moves.Clear();

            GeneratePawnMoves();
            GenerateBishopMoves();
            GenerateKnightMoves();
            GenerateRookMoves();
            GenerateQueenMoves();
            GenerateKingMoves();
        }

        private void GenerateKingMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.King].CreateCopy();
            while (bitboard.Board > 0)
            {
                // Ideally this loop should only run once
                var source = bitboard.GetLsb();
                var attacks = new Bitboard(Bitboard.KingAttacks[source]);

                while (attacks.Board > 0)
                {
                    var target = attacks.GetLsb();

                    if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                    {
                        attacks.ClearBit(target);
                        continue;
                    }

                    AddMoveToMovesList(source, target);

                    attacks.ClearBit(target);
                }

                GenerateCastleMoves();
                bitboard.ClearBit(source);
            }

        }

        public Move FindBestMove(int depth)
        {
            GenerateMoves();

            depth.ToString(); // Placeholder for actual depth usage later

            return PickNextLegalMove();
        }

        private Move PickNextLegalMove()
        {
            for (var i = 0; i < Moves.Count; ++i)
            {
                var move = Moves[i];
                SaveState();
                MakeMove(move);
                if (!IsInCheck(SideToMove.Other()))
                {
                    RestoreState();
                    return move;
                }
                RestoreState();
            }
            return new Move(Square.NoSquare, Square.NoSquare, MoveType.Quiet);
        }

        private void GenerateQueenMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.Queen].CreateCopy();
            while (bitboard.Board > 0)
            {
                var source = bitboard.GetLsb();
                var attacks = Bitboard.GetQueenAttacksFromTable((Square)source, Occupancies[(int)Side.Both]);

                while (attacks.Board > 0)
                {
                    var target = attacks.GetLsb();

                    if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                    {
                        attacks.ClearBit(target);
                        continue;
                    }

                    AddMoveToMovesList(source, target);

                    attacks.ClearBit(target);
                }

                bitboard.ClearBit(source);
            }
        }

        private void GenerateRookMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.Rook].CreateCopy();
            while (bitboard.Board > 0)
            {
                var source = bitboard.GetLsb();
                var attacks = Bitboard.GetRookAttacksFromTable((Square)source, Occupancies[(int)Side.Both]);

                while (attacks.Board > 0)
                {
                    var target = attacks.GetLsb();

                    if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                    {
                        attacks.ClearBit(target);
                        continue;
                    }

                    AddMoveToMovesList(source, target);

                    attacks.ClearBit(target);
                }

                bitboard.ClearBit(source);
            }
        }

        private void GenerateKnightMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.Knight].CreateCopy();
            while (bitboard.Board > 0)
            {
                var source = bitboard.GetLsb();
                var attacks = new Bitboard(Bitboard.KnightAttacks[source]);

                while (attacks.Board > 0)
                {
                    var target = attacks.GetLsb();

                    if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                    {
                        attacks.ClearBit(target);
                        continue;
                    }

                    AddMoveToMovesList(source, target);

                    attacks.ClearBit(target);
                }

                bitboard.ClearBit(source);
            }
        }

        private void GenerateBishopMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.Bishop].CreateCopy();
            while (bitboard.Board > 0)
            {
                var source = bitboard.GetLsb();
                var attacks = Bitboard.GetBishopAttacksFromTable((Square)source, Occupancies[(int)Side.Both]);

                while (attacks.Board > 0)
                {
                    var target = attacks.GetLsb();

                    if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                    {
                        attacks.ClearBit(target);
                        continue;
                    }

                    AddMoveToMovesList(source, target);

                    attacks.ClearBit(target);
                }

                bitboard.ClearBit(source);
            }
        }

        private void GeneratePawnMoves()
        {
            var bitboard = Pieces[(int)SideToMove, (int)Piece.Pawn].CreateCopy();
            while (bitboard.Board > 0)
            {
                var source = bitboard.GetLsb();
                GeneratePawnPushes(source);
                GenerateEnPassants(source);
                GeneratePawnAttacks(source);

                bitboard.ClearBit(source);
            }
        }

        private void GenerateEnPassants(int source)
        {
            if (EnPassantSquare == Square.NoSquare)
                return;

            var attacks = new Bitboard(Bitboard.PawnAttacks[(int)SideToMove, source] & (1ul << (int)EnPassantSquare));
            if (attacks.Board > 0)
            {
                var target = attacks.GetLsb();
                AddPawnMove(source, target, true, false);
            }

        }


        private void GeneratePawnPushes(int source)
        {
            if (SideToMove == Side.Black)
            {
                var oneSquarePush = source + 8;
                if (Occupancies[(int)Side.Both].GetBit(oneSquarePush) != 0) return;
                AddPawnMove(source, oneSquarePush, false, false);
                if (source is <= (int)Square.h7 and >= (int)Square.a7)
                {
                    var twoSquarePush = oneSquarePush + 8;
                    if (Occupancies[(int)Side.Both].GetBit(twoSquarePush) != 0) return;
                    AddPawnMove(source, twoSquarePush, false, true);
                }
            }
            else
            {
                var oneSquarePush = source - 8;
                if (Occupancies[(int)Side.Both].GetBit(oneSquarePush) != 0) return;
                AddPawnMove(source, oneSquarePush, false, false);
                if (source is <= (int)Square.h2 and >= (int)Square.a2)
                {
                    var twoSquarePush = oneSquarePush - 8;
                    if (Occupancies[(int)Side.Both].GetBit(twoSquarePush) != 0) return;
                    AddPawnMove(source, twoSquarePush, false, true);
                }
            }
        }

        private void GeneratePawnAttacks(int source)
        {
            var attacks = new Bitboard(Bitboard.PawnAttacks[(int)SideToMove, source] & Occupancies[(int)SideToMove.Other()].Board);

            while (attacks.Board > 0)
            {
                var target = attacks.GetLsb();

                if (Occupancies[(int)SideToMove].GetBit(target) == 1)
                {
                    attacks.ClearBit(target);
                    continue;
                }

                AddPawnMove(source, target, false, false);

                attacks.ClearBit(target);
            }
        }
        private void GenerateCastleMoves()
        {
            // Squares should be empty and shouldn't castle through check, can avoid checking if landing position is check to do legal move check in make move function

            var occ = Occupancies[(int)Side.Both];
            if (SideToMove == Side.White)
            {
                if (Castle.HasFlag(Castle.WhiteShort))
                {
                    if (occ.GetBit(Square.f1) == 0 && occ.GetBit(Square.g1) == 0 && !IsSquareAttacked(Square.e1, Side.Black) && !IsSquareAttacked(Square.f1, Side.Black))
                        Moves.Add(new Move(Square.e1, Square.g1, MoveType.Castle));
                }
                if (Castle.HasFlag(Castle.WhiteLong))
                {
                    if (occ.GetBit(Square.d1) == 0 && occ.GetBit(Square.c1) == 0 && occ.GetBit(Square.b1) == 0 && !IsSquareAttacked(Square.e1, Side.Black) && !IsSquareAttacked(Square.d1, Side.Black))
                        Moves.Add(new Move(Square.e1, Square.c1, MoveType.Castle));
                }
            }
            else
            {
                if (Castle.HasFlag(Castle.BlackShort))
                {
                    if (occ.GetBit(Square.f8) == 0 && occ.GetBit(Square.g8) == 0 && !IsSquareAttacked(Square.e8, Side.White) && !IsSquareAttacked(Square.f8, Side.White))
                        Moves.Add(new Move(Square.e8, Square.g8, MoveType.Castle));
                }
                if (Castle.HasFlag(Castle.BlackLong))
                {
                    if (occ.GetBit(Square.d8) == 0 && occ.GetBit(Square.c8) == 0 && occ.GetBit(Square.b8) == 0 && !IsSquareAttacked(Square.e8, Side.White) && !IsSquareAttacked(Square.d8, Side.White))
                        Moves.Add(new Move(Square.e8, Square.c8, MoveType.Castle));
                }
            }
        }

        private bool IsSquareAttacked(Square square, Side attackingSide)
        {
            if ((Bitboard.PawnAttacks[(int)attackingSide.Other(), (int)square] & Pieces[(int)attackingSide, (int)Piece.Pawn].Board) != 0)
                return true;
            if ((Bitboard.KnightAttacks[(int)square] & Pieces[(int)attackingSide, (int)Piece.Knight].Board) != 0)
                return true;
            if ((Bitboard.GetBishopAttacksFromTable(square, Occupancies[(int)Side.Both]).Board & Pieces[(int)attackingSide, (int)Piece.Bishop].Board) != 0)
                return true;
            if ((Bitboard.GetRookAttacksFromTable(square, Occupancies[(int)Side.Both]).Board & Pieces[(int)attackingSide, (int)Piece.Rook].Board) != 0)
                return true;
            if ((Bitboard.GetQueenAttacksFromTable(square, Occupancies[(int)Side.Both]).Board & Pieces[(int)attackingSide, (int)Piece.Queen].Board) != 0)
                return true;
            if ((Bitboard.KingAttacks[(int)square] & Pieces[(int)attackingSide, (int)Piece.King].Board) != 0)
                return true;
            return false;
        }

        private void AddPawnMove(int source, int target, bool enpassant, bool doublePush)
        {
            // This assumes all incoming pawn moves are valid
            if (target is >= (int)Square.a1 and <= (int)Square.h1 || target is <= (int)Square.h8 and >= (int)Square.a8)
            {
                var capture = IsSquareCapture(target);

                Moves.Add(new Move((Square)source, (Square)target, capture ? MoveType.KnightPromotionCapture : MoveType.KnightPromotion));
                Moves.Add(new Move((Square)source, (Square)target, capture ? MoveType.BishopPromotionCapture : MoveType.BishopPromotion));
                Moves.Add(new Move((Square)source, (Square)target, capture ? MoveType.RookPromotionCapture : MoveType.RookPromotion));
                Moves.Add(new Move((Square)source, (Square)target, capture ? MoveType.QueenPromotionCapture : MoveType.QueenPromotion));
            }
            else if (enpassant || doublePush)
            {
                Moves.Add(new Move((Square)source, (Square)target, enpassant ? MoveType.EnPassant : MoveType.DoublePush));
            }
            else
            {
                AddMoveToMovesList(source, target);
            }
        }


        private void AddMoveToMovesList(int source, int target)
        {
            // Makes more sense for source and target to come in as Square instead of int, refactor later
            var moveType = IsSquareCapture(target) ? MoveType.Capture : MoveType.Quiet;
            var move = new Move((Square)source, (Square)target, moveType);
            Moves.Add(move);
        }
        private bool IsSquareCapture(int target)
        {
            return Occupancies[(int)SideToMove.Other()].GetBit(target) == 1;
        }
    }
}
