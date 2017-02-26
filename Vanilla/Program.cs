using System;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;
using TrulyQuantumChess.Vanilla.ConsoleIO;

namespace TrulyQuantumChess.Vanilla {
    public static class Program {
        public static void Main(string[] args) {
            int cols = 8;
            if (args.Length >= 1) {
                cols = Convert.ToInt32(args[0]);
            }

            var engine = new QuantumChessEngine();
            for (;;) {
                Output.DisplayQuantumChessboard(engine.QuantumChessboard, cols);
                Console.WriteLine();

                for (;;) {
                    try {
                        QuantumChessMove move = Input.ReadMoveRepeated(engine);
                        engine.Submit(move);
                        break;
                    } catch (MoveParseException e) {
                        Console.WriteLine(e.Message);
                    } catch (MoveProcessException e) {
                        Console.WriteLine(e.Message);
                    }
                }

                switch (engine.GameState) {
                    case GameState.WhiteVictory:
                        Console.WriteLine();
                        Console.WriteLine("White victory!");
                    return;

                    case GameState.BlackVictory:
                        Console.WriteLine();
                        Console.WriteLine("Black victory!");
                    return;

                    case GameState.Tie:
                        Console.WriteLine();
                        Console.WriteLine("Players are tied!");
                    return;
                }
            }
        }
    }
}
