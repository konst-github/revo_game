using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;

namespace RevoGameSpace
{
    /*
     * Initial constraints:
     * 
     * - The board has a fixed size of 5 squares (assume it is always square). 
     * 
     * - The piece can move around the board in one of four directions ((N)orth, (E)ast, (S)outh and (W)est).
     * 
     * - The piece will start in the bottom left corner of the board facing North.
     * 
     * - The bottom left board square is indicated by the position 0 0, top left corner of the board is 0 4, 
     *   bottom right is 4 0 and top right is therefore 4 4.
     * 
     * - If you try to make a move that would result in the piece moving off the board the move will have no effect. 
     *   For example moving North when you are already at the top of the board.
     *   NOTE K: There are 2 cases here - either drop all remained commands when the end of the board is reached, 
     *   or just ignore single move and continue execution. I will ignore that move.
     * 
     * - You can assume that the input is always correct.
     *   NOTE K: Even though - just a simplest validity check will be performed.
     * 
     */


    /*  
     * = Program structure:
     * 
     * 0. Launch.
     * 
     * 1. Validate pazssed arguments - expect command sequence as a first argument.
     * 
     * 2. Parse input - if command sequence contains any character, other than 'M', 'L', 'R' =>
     *    consider input as invalud and stop execution.
     * 
     * 3. Split input to single commands.
     * 
     * 4. Execute each single command in a loop (break the loop if move is not possible)
     * 
     * 4.1 Check if move is possible: - TRUE => MOVE
     *                                - FALSE => STOP // f.e. reached the edge of the board
     *                                
     */

    public class RevoGame
    {
        static public string ERROR_EMPTY = "ERROR: EMPTY";
        static public string ERROR_INVALID = "ERROR: INVALID";

        static void Main(string[] args)
        {
            Console.Clear();

            string sequence;

            if (args.Length == 0)
            {
                Console.WriteLine("Please enter your movement sequence.\nM - move, L - turn left, R - turn right.");
                sequence = Console.ReadLine();
            }
            else
            {
                sequence = args[0];
            }

            RevoGame revoGame = new RevoGame();
            revoGame.ProcessSequence(sequence);
        }

        public string ProcessSequence(string sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                Console.WriteLine("ERROR: Empty Sequence");
                return ERROR_EMPTY;
            }

            if (!Parser.IsSequenceValid(sequence))
            {
                Console.WriteLine("ERROR: Sequence contains invalid commands.");
                return ERROR_INVALID;
            }

            Console.Clear();
            Board board = new Board();
            return board.ExecuteSequence(sequence);
        }

        private class Board
        {
            private enum Command
            {
                Move  = 'M',
                Left  = 'L',
                Right = 'R'
            }

            // By changing this value it is possible to expand board to any size,
            // but I've tested max 30x30 board.
            // No other changes are required to play on a bigger board.
            static private readonly int BOARD_SIZE = 5;

            private readonly int[,] squares = new int[BOARD_SIZE, BOARD_SIZE];

            private readonly Pawn pawn = new Pawn(BOARD_SIZE);

            private string lastSequence = "";


            internal string ExecuteSequence(string _sequence)
            {
                lastSequence = _sequence;
//                lastSequence = "MRMLMRMLMRMLMRMLMRMLMRMLMRMLMRMLMRML";
                char[] commands = lastSequence.ToCharArray();

                // Show initial position
                UpdatePosition(pawn.position, 1);
                Thread.Sleep(250);

                foreach (char command in commands)
                {
                    // 1. Reset old position
                    UpdatePosition(pawn.position, 0);

                    // 2. Calculate new position
                    switch (command)
                    {
                        case (char)Command.Move:    pawn.Move();        break;
                        case (char)Command.Left:    pawn.TurnLeft();    break;
                        case (char)Command.Right:   pawn.TurnRight();   break;
                        default:                                        break;
                    }

                    // 3. Set new position
                    Console.Clear();
                    UpdatePosition(pawn.position, 1);
                    Thread.Sleep(250);
                }

                // Output more extended info
                Console.WriteLine("EXECUTION DONE!\n" + pawn.Description());
                // but return what is required by specs
                return pawn.position.X + " " + pawn.position.Y + " " + pawn.DirectionCardinal();
            }

            private void UpdatePosition(Point position, int value)
            {
                squares[position.X, position.Y] = value;
                if (value != 0) { OutputState(); }
            }

            private void OutputState()
            {
#if DEBUG
                string boardString = "";

                string avatar = pawn.Avatar();

                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    string line0 = "    ";
                    string line1 = "   ";
                    string line2 = (y > 9 ? "" : " ") + y + " ";
                    for (int x = 0; x < BOARD_SIZE; x++)
                    {
                        line0 += " " + x + (x > 9 ? " " : "  ");
                        int value = squares[x, y];
                        line1 += "+---";
                        line2 += value > 0 ? ("| " + avatar + " ") : "|   ";
                    }
                    line0 += "  X";
                    line1 += "+";
                    line2 += "|";

                    boardString = line2 + "\n" + line1 + "\n" + boardString;

                    if(y == 0)
                    {
                        boardString += line0;
                    }
                    if(y == BOARD_SIZE - 1)
                    {
                        boardString = "EXECUTING SEQUENCE: " + lastSequence + "\n\n" + "Y" + "\n" + line1 + "\n" + boardString;
                    }
                }

                Console.WriteLine(boardString);

                Thread.Sleep(250);

                //Console.WriteLine("Y");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("4 | 04| 14| 24| 34| 44|");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("3 | 03| 13| 23| 33| 43|");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("2 | 02| 12| 22| 32| 42|");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("1 | 01| 11| 21| 31| 41|");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("0 | 00| 10| 20| 30| 40|");
                //Console.WriteLine("  +---+---+---+---+---+");
                //Console.WriteLine("    0   1   2   3   4   X");
#endif
            }


            private class Pawn
            {
                private enum Direction : byte
                {
                    /*
                     * Since Pawn's direction can be rotated left (counter clockwise) 
                     * and right (clockwise), 'circular shift' (bitwise rotation) 
                     * is the ideal solution to avoid tons of nested else-ifs, switches, 
                     * arrays etc.
                     * 
                     * Since in C# 'byte' is the shortest primitive type - 
                     * each of 4 enum values uses 2 assigned bits.
                     */
                    North = 0b_0000_0011,
                    East = 0b_0000_1100,
                    South = 0b_0011_0000,
                    West = 0b_1100_0000,
                    Shift = 2 // 0x00000010 - doesn't interfere with other values
                }

                private Direction direction = Direction.North;

                internal Point position = new Point(0, 0);

                private readonly int boardSize = 0;


                internal Pawn(int _boardSize)
                {
                    this.boardSize = Math.Max(0, _boardSize);
                }

                internal void Move()
                {
                    switch (direction)
                    {
                        case Direction.North: MoveUp(); break;
                        case Direction.South: MoveDown(); break;
                        case Direction.West: MoveLeft(); break;
                        case Direction.East: MoveRight(); break;
                        default: break;
                    }
                }

                private void MoveUp()
                {
                    position.Y = Math.Min(position.Y + 1, boardSize - 1);
                }

                private void MoveDown()
                {
                    position.Y = Math.Max(position.Y - 1, (byte)0);
                }

                private void MoveLeft()
                {
                    position.X = Math.Max(position.X - 1, (byte)0);
                }

                private void MoveRight()
                {
                    position.X = Math.Min(position.X + 1, boardSize - 1);
                }

                internal void TurnLeft()
                {
                    direction = (Direction)((((byte)direction >> 2) | ((byte)direction << (8 - 2))) & Byte.MaxValue);
                }

                internal void TurnRight()
                {
                    direction = (Direction)((((byte)direction << 2) | ((byte)direction >> (8 - 2))) & Byte.MaxValue);
                }

                internal string Avatar()
                {
                    return direction switch
                    {
                        Direction.North => "A",
                        Direction.South => "V",
                        Direction.West => "<",
                        Direction.East => ">",
                        _ => "X",
                    };
                }

                internal string DirectionCardinal()
                {
                    return direction switch
                    {
                        Direction.North => "N",
                        Direction.South => "S",
                        Direction.East => "E",
                        Direction.West => "W",
                        _ => "U",
                    };
                }

                internal string Description()
                {
                    string description = "Position => X " + position.X + " : Y " + position.Y + " : Direction => " + DirectionCardinal();
                    return description;
                }
            } // private class Pawn

        } // private class Board


        private class Parser
        {
            static internal bool IsSequenceValid(string sequence)
            {
                Regex strPattern = new Regex("^[MLR]+$");
                return strPattern.IsMatch(sequence);
            }
        }
    }
}
