using System;

namespace TrulyQuantumChess.Kernel.Errors {
    public abstract class QuantumChessException : Exception {
        public QuantumChessException(string message)
            : base(message)
        {}
    }

    public class AssertionException : QuantumChessException {
        public AssertionException(string message)
            : base(message)
        {}

        public static void Assert(bool expr, string message) {
            if (!expr) {
                throw new AssertionException(message);
            }
        }
    }

    public class QuantumChessEngineException : QuantumChessException {
        public QuantumChessEngineException(string message)
            : base(message)
        {}

        public static void Throw(string message) {
            throw new QuantumChessEngineException(message);
        }
    }
}
