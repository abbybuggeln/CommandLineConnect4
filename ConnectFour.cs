using System;

namespace ConnectFour
{
    internal class Board
    {
        public enum SpaceState
        {
            Empty,
            Player1,
            Player2,
        }

        public enum MoveType
        {
            Player1,
            Player2,
        }

        public const uint Width = 7;
        public const uint Height = 6;

        public bool ConnectFour { get; private set; } = false;
        public bool FullBoard { get;  set; } = false;

        private readonly SpaceState[,] board = new SpaceState[Height, Width];
        private readonly int[,] directions = new int[8, 2]
        {
            { 1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 },
            { -1, -1 }, { 0, -1 }, { 1, -1 }, { -1, 0 }
        };

        public void EmptyTheBoard()
        {
            ConnectFour = false;
            FullBoard = false;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    board[row, column] = SpaceState.Empty;
                }
            }
        }

        public SpaceState GetSpace(uint row, uint column)
        {
            if (row >= Height || column >= Width)
                throw new ArgumentOutOfRangeException();

            return board[row, column];
        }

        private void SetSpace(uint row, uint column, SpaceState newState)
        {
            if (row >= Height || column >= Width)
                throw new ArgumentOutOfRangeException();

            board[row, column] = newState;
        }

        public uint GetColumnHeight(uint column)
        {
            if (column >= Width)
                throw new ArgumentOutOfRangeException();

            uint height = 0;
            while (height < Height && board[height, column] != SpaceState.Empty)
            {
                height++;
            }
            return height;
        }

        public bool CanMakeMove(uint column)
        {
            return GetColumnHeight(column) < Height;
        }

        public void MakeMove(MoveType move, uint column)
        {
            if (!CanMakeMove(column))
            {
                Console.WriteLine("This column is already full! Pick a new one to keep playing.");
                throw new InvalidOperationException();
            }

            uint currentHeight = GetColumnHeight(column);
            SetSpace(currentHeight, column, ConvertMoveToSpaceState(move));
        }

        private void FindConsecutiveTiles(uint row, uint column, MoveType player, int[] counts)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                counts[i] = 0;
                for (int j = 1; j < 4; j++)
                {
                    int newRow = (int)row + directions[i, 0] * j;
                    int newColumn = (int)column + directions[i, 1] * j;

                    if (newRow < 0 || newRow >= Height || newColumn < 0 || newColumn >= Width ||
                        board[newRow, newColumn] != ConvertMoveToSpaceState(player))
                    {
                        break;
                    }
                    counts[i]++;
                }
            }
        }

        public uint ComputerPlay(uint row, uint column)
        {
            int[] counts = new int[8];
            FindConsecutiveTiles(row, column, MoveType.Player1, counts);

            int maxCount = -1;
            int bestDirection = -1;

            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > maxCount)
                {
                    maxCount = counts[i];
                    bestDirection = i;
                }
            }
            bestDirection = 7 - bestDirection;
            if (bestDirection == -1)
            {
                throw new InvalidOperationException("No valid move found.");
            }

            int newRow = (int)row + directions[bestDirection, 0] * (counts[bestDirection] + 1);
            int newColumn = (int)column + directions[bestDirection, 1] * (counts[bestDirection] + 1);

            while (newColumn < 0 || newColumn >= Width || !CanMakeMove((uint)newColumn))
            {
                newColumn = (int)column + directions[bestDirection, 1] * (counts[bestDirection] - 1);
            }

            MakeMove(MoveType.Player2, (uint)newColumn);
            return (uint)newColumn;
        }

        public SpaceState LookForAWinner(uint row, uint column, MoveType player)
        {
            int[] counts = new int[8];
            FindConsecutiveTiles(row, column, player, counts);

            foreach (var count in counts)
            {
                if (count >= 3)
                {
                    return ConvertMoveToSpaceState(player);
                }
            }
            return SpaceState.Empty;
        }

        private string GetSpaceStateCharacter(SpaceState state)
        {
            return state switch
            {
                SpaceState.Player1 => "X",
                SpaceState.Player2 => "O",
                _ => " ",
            };
        }

        public void PrintBoard()
        {
            for (uint i = 0; i < Width; i++)
            {
                Console.Write("|" + (i + 1));
            }
            Console.WriteLine();

            for (uint i = 0; i < Height; i++)
            {
                for (uint j = 0; j < Width; j++)
                {
                    Console.Write("|" + GetSpaceStateCharacter(GetSpace(Height - i - 1, j)));
                }
                Console.WriteLine();
            }
        }

        private SpaceState ConvertMoveToSpaceState(MoveType move)
        {
            return move == MoveType.Player1 ? SpaceState.Player1 : SpaceState.Player2;
        }
    }

    internal class Program
    {
        static uint? GetRequestedColumn()
        {
            var requestedColumn = Console.ReadLine();
            if (uint.TryParse(requestedColumn, out var column))
            {
                return column - 1;
            }
            return null;
        }

        static void Main(string[] args)
        {
            Board board = new Board();
            board.EmptyTheBoard();
            Console.WriteLine("Welcome! Let's play Connect Four. Our board is empty to start.");
            int turnCount = 0;

            do
            {
                Console.WriteLine("Your turn, choose a column");
                var playerColumn = GetRequestedColumn();
                if (playerColumn == null)
                    return;

                Console.WriteLine("Great choice! Let's check out the game board.");
                board.MakeMove(Board.MoveType.Player1, playerColumn.Value);
                System.Threading.Thread.Sleep(1300);
                board.PrintBoard();
                uint playerRow = board.GetColumnHeight(playerColumn.Value) - 1;

                var winner = board.LookForAWinner(playerRow, playerColumn.Value, Board.MoveType.Player1);
                if (winner == Board.SpaceState.Player1)
                {
                    Console.WriteLine("Congrats! You beat me! Game Over.");
                    break;
                }

                uint computerColumn = board.ComputerPlay(playerRow, playerColumn.Value);
                Console.WriteLine("Now it's my turn, here is my choice.");
                System.Threading.Thread.Sleep(1300);
                board.PrintBoard();
                uint computerRow = board.GetColumnHeight(computerColumn) - 1;

                winner = board.LookForAWinner(computerRow, computerColumn, Board.MoveType.Player2);
                if (winner == Board.SpaceState.Player2)
                {
                    Console.WriteLine("I win! Game Over.");
                    break;
                }

                turnCount += 2;
                if (turnCount >= 7 * 6)
                {
                    Console.WriteLine("Full board! Game Over.");
                    board.FullBoard = true;
                }

            } while (!board.FullBoard);
        }
    }
}
