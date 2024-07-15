using UnityEngine;

namespace Tables.Editor.Utilities
{
    public static class KeyCodeUtility
    {
        public static bool IsNumericKey(this KeyCode keyCode)
        {
            return (KeyCode.Alpha0 <= keyCode && keyCode <= KeyCode.Alpha9) ||
                   (KeyCode.Keypad0 <= keyCode && keyCode <= KeyCode.Keypad9);
        }

        public static int GetNumericValue(this KeyCode keyCode)
        {
            if (KeyCode.Alpha0 <= keyCode && keyCode <= KeyCode.Alpha9) return keyCode - KeyCode.Alpha0;
            if (KeyCode.Keypad0 <= keyCode && keyCode <= KeyCode.Keypad9) return keyCode - KeyCode.Keypad0;
            return -1;
        }

        public static string KeyCodeToString(this KeyCode keyCode, bool isShiftPressed)
        {
            switch (keyCode)
            {
                // Letters
                case KeyCode.A: return isShiftPressed ? "A" : "a";
                case KeyCode.B: return isShiftPressed ? "B" : "b";
                case KeyCode.C: return isShiftPressed ? "C" : "c";
                case KeyCode.D: return isShiftPressed ? "D" : "d";
                case KeyCode.E: return isShiftPressed ? "E" : "e";
                case KeyCode.F: return isShiftPressed ? "F" : "f";
                case KeyCode.G: return isShiftPressed ? "G" : "g";
                case KeyCode.H: return isShiftPressed ? "H" : "h";
                case KeyCode.I: return isShiftPressed ? "I" : "i";
                case KeyCode.J: return isShiftPressed ? "J" : "j";
                case KeyCode.K: return isShiftPressed ? "K" : "k";
                case KeyCode.L: return isShiftPressed ? "L" : "l";
                case KeyCode.M: return isShiftPressed ? "M" : "m";
                case KeyCode.N: return isShiftPressed ? "N" : "n";
                case KeyCode.O: return isShiftPressed ? "O" : "o";
                case KeyCode.P: return isShiftPressed ? "P" : "p";
                case KeyCode.Q: return isShiftPressed ? "Q" : "q";
                case KeyCode.R: return isShiftPressed ? "R" : "r";
                case KeyCode.S: return isShiftPressed ? "S" : "s";
                case KeyCode.T: return isShiftPressed ? "T" : "t";
                case KeyCode.U: return isShiftPressed ? "U" : "u";
                case KeyCode.V: return isShiftPressed ? "V" : "v";
                case KeyCode.W: return isShiftPressed ? "W" : "w";
                case KeyCode.X: return isShiftPressed ? "X" : "x";
                case KeyCode.Y: return isShiftPressed ? "Y" : "y";
                case KeyCode.Z: return isShiftPressed ? "Z" : "z";

                // Numbers (Top Row)
                case KeyCode.Alpha0: return "0";
                case KeyCode.Alpha1: return "1";
                case KeyCode.Alpha2: return "2";
                case KeyCode.Alpha3: return "3";
                case KeyCode.Alpha4: return "4";
                case KeyCode.Alpha5: return "5";
                case KeyCode.Alpha6: return "6";
                case KeyCode.Alpha7: return "7";
                case KeyCode.Alpha8: return "8";
                case KeyCode.Alpha9: return "9";

                // Numbers (Keypad)
                case KeyCode.Keypad0: return "0";
                case KeyCode.Keypad1: return "1";
                case KeyCode.Keypad2: return "2";
                case KeyCode.Keypad3: return "3";
                case KeyCode.Keypad4: return "4";
                case KeyCode.Keypad5: return "5";
                case KeyCode.Keypad6: return "6";
                case KeyCode.Keypad7: return "7";
                case KeyCode.Keypad8: return "8";
                case KeyCode.Keypad9: return "9";

                // Special Characters
                case KeyCode.Space: return " ";
                case KeyCode.Period: return ".";
                case KeyCode.Comma: return ",";
                case KeyCode.Semicolon: return isShiftPressed ? ":" : ";";
                case KeyCode.Slash: return isShiftPressed ? "?" : "/";
                case KeyCode.Backslash: return isShiftPressed ? "|" : "\\";
                case KeyCode.LeftBracket: return isShiftPressed ? "{" : "[";
                case KeyCode.RightBracket: return isShiftPressed ? "}" : "]";
                case KeyCode.Quote: return isShiftPressed ? "\"" : "'";
                case KeyCode.Minus: return isShiftPressed ? "_" : "-";
                case KeyCode.Equals: return isShiftPressed ? "+" : "=";

                // Handle all other KeyCodes
                default: return string.Empty;
            }
        }
    }
}
