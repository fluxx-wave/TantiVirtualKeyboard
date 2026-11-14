using System.Collections.Generic;
using HotVirtualKeyboard.Core;

namespace HotVirtualKeyboard.Layouts
{
    public static class LayoutDefinitions
    {
        /// <summary>
        /// Windows 10 Default Touch Keyboard Layout (Clean 4-row modern touch interface)
        /// </summary>
        public static KeyboardLayout CreateTouchLayout()
        {
            var layout = new KeyboardLayout
            {
                Id = "touch",
                Name = "Windows 10 Touch Keyboard",
                Description = "Clean, modern Windows 10 touch layout with large touch targets"
            };

            // Row 1: QWERTY
            var r1 = new KeyRow();
            r1.Keys.Add(new KeyModel("q", "Q", 0x51, 1.0));
            r1.Keys.Add(new KeyModel("w", "W", 0x57, 1.0));
            r1.Keys.Add(new KeyModel("e", "E", 0x45, 1.0));
            r1.Keys.Add(new KeyModel("r", "R", 0x52, 1.0));
            r1.Keys.Add(new KeyModel("t", "T", 0x54, 1.0));
            r1.Keys.Add(new KeyModel("y", "Y", 0x59, 1.0));
            r1.Keys.Add(new KeyModel("u", "U", 0x55, 1.0));
            r1.Keys.Add(new KeyModel("i", "I", 0x49, 1.0));
            r1.Keys.Add(new KeyModel("o", "O", 0x4F, 1.0));
            r1.Keys.Add(new KeyModel("p", "P", 0x50, 1.0));
            layout.Rows.Add(r1);

            // Row 2: ASDF (Indented like Windows 10)
            var r2 = new KeyRow();
            r2.Keys.Add(new KeyModel("a", "A", 0x41, 1.0));
            r2.Keys.Add(new KeyModel("s", "S", 0x53, 1.0));
            r2.Keys.Add(new KeyModel("d", "D", 0x44, 1.0));
            r2.Keys.Add(new KeyModel("f", "F", 0x46, 1.0));
            r2.Keys.Add(new KeyModel("g", "G", 0x47, 1.0));
            r2.Keys.Add(new KeyModel("h", "H", 0x48, 1.0));
            r2.Keys.Add(new KeyModel("j", "J", 0x4A, 1.0));
            r2.Keys.Add(new KeyModel("k", "K", 0x4B, 1.0));
            r2.Keys.Add(new KeyModel("l", "L", 0x4C, 1.0));
            layout.Rows.Add(r2);

            // Row 3: Shift, ZXCV, Backspace
            var r3 = new KeyRow();
            r3.Keys.Add(new KeyModel("⇧", Win32.VK_SHIFT, 1.4, KeyActionType.Modifier) { Tag = "Shift" });
            r3.Keys.Add(new KeyModel("z", "Z", 0x5A, 1.0));
            r3.Keys.Add(new KeyModel("x", "X", 0x58, 1.0));
            r3.Keys.Add(new KeyModel("c", "C", 0x43, 1.0));
            r3.Keys.Add(new KeyModel("v", "V", 0x56, 1.0));
            r3.Keys.Add(new KeyModel("b", "B", 0x42, 1.0));
            r3.Keys.Add(new KeyModel("n", "N", 0x4E, 1.0));
            r3.Keys.Add(new KeyModel("m", "M", 0x4D, 1.0));
            r3.Keys.Add(new KeyModel("⌫", Win32.VK_BACK, 1.4, KeyActionType.VirtualKey));
            layout.Rows.Add(r3);

            // Row 4: &123, Voice Input, Modifiers, Space, ., Enter
            var r4 = new KeyRow();
            r4.Keys.Add(new KeyModel("&123", 0, 1.2, KeyActionType.Macro) { MacroCommand = "SWITCH_LAYOUT:symbol" });
            r4.Keys.Add(new KeyModel("🎙", 0, 1.0, KeyActionType.Macro) { MacroCommand = "VOICE_INPUT" });
            r4.Keys.Add(new KeyModel("Ctrl", Win32.VK_CONTROL, 1.0, KeyActionType.Modifier) { Tag = "Ctrl" });
            r4.Keys.Add(new KeyModel("Space", Win32.VK_SPACE, 4.1, KeyActionType.VirtualKey));
            r4.Keys.Add(new KeyModel(".", 0xBE, 1.0));
            r4.Keys.Add(new KeyModel("↵ Enter", Win32.VK_RETURN, 1.7, KeyActionType.VirtualKey));
            layout.Rows.Add(r4);

            return layout;
        }

        /// <summary>
        /// Windows 10 Full 5-Row Desktop Layout (With number row, Tab, Esc, Arrows)
        /// </summary>
        public static KeyboardLayout CreateFullLayout()
        {
            var layout = new KeyboardLayout
            {
                Id = "full",
                Name = "Windows 10 Full Keyboard",
                Description = "5-Row desktop layout with number row, Tab, Esc, and arrow keys"
            };

            // Row 0: Esc + Numbers + Backspace
            var r0 = new KeyRow();
            r0.Keys.Add(new KeyModel("Esc", Win32.VK_ESCAPE, 1.0, KeyActionType.VirtualKey));
            r0.Keys.Add(new KeyModel("1", "!", 0x31, 1.0));
            r0.Keys.Add(new KeyModel("2", "@", 0x32, 1.0));
            r0.Keys.Add(new KeyModel("3", "#", 0x33, 1.0));
            r0.Keys.Add(new KeyModel("4", "$", 0x34, 1.0));
            r0.Keys.Add(new KeyModel("5", "%", 0x35, 1.0));
            r0.Keys.Add(new KeyModel("6", "^", 0x36, 1.0));
            r0.Keys.Add(new KeyModel("7", "&", 0x37, 1.0));
            r0.Keys.Add(new KeyModel("8", "*", 0x38, 1.0));
            r0.Keys.Add(new KeyModel("9", "(", 0x39, 1.0));
            r0.Keys.Add(new KeyModel("0", ")", 0x30, 1.0));
            r0.Keys.Add(new KeyModel("-", "_", 0xBD, 1.0));
            r0.Keys.Add(new KeyModel("=", "+", 0xBB, 1.0));
            r0.Keys.Add(new KeyModel("⌫", Win32.VK_BACK, 1.5, KeyActionType.VirtualKey));
            layout.Rows.Add(r0);

            // Row 1: Tab + QWERTY + Brackets
            var r1 = new KeyRow();
            r1.Keys.Add(new KeyModel("Tab", Win32.VK_TAB, 1.4, KeyActionType.VirtualKey));
            r1.Keys.Add(new KeyModel("q", "Q", 0x51, 1.0));
            r1.Keys.Add(new KeyModel("w", "W", 0x57, 1.0));
            r1.Keys.Add(new KeyModel("e", "E", 0x45, 1.0));
            r1.Keys.Add(new KeyModel("r", "R", 0x52, 1.0));
            r1.Keys.Add(new KeyModel("t", "T", 0x54, 1.0));
            r1.Keys.Add(new KeyModel("y", "Y", 0x59, 1.0));
            r1.Keys.Add(new KeyModel("u", "U", 0x55, 1.0));
            r1.Keys.Add(new KeyModel("i", "I", 0x49, 1.0));
            r1.Keys.Add(new KeyModel("o", "O", 0x4F, 1.0));
            r1.Keys.Add(new KeyModel("p", "P", 0x50, 1.0));
            r1.Keys.Add(new KeyModel("[", "{", 0xDB, 1.0));
            r1.Keys.Add(new KeyModel("]", "}", 0xDD, 1.0));
            r1.Keys.Add(new KeyModel("\\", "|", 0xDC, 1.1));
            layout.Rows.Add(r1);

            // Row 2: Caps + ASDF + Enter
            var r2 = new KeyRow();
            r2.Keys.Add(new KeyModel("Caps", Win32.VK_CAPITAL, 1.6, KeyActionType.Modifier) { Tag = "Caps" });
            r2.Keys.Add(new KeyModel("a", "A", 0x41, 1.0));
            r2.Keys.Add(new KeyModel("s", "S", 0x53, 1.0));
            r2.Keys.Add(new KeyModel("d", "D", 0x44, 1.0));
            r2.Keys.Add(new KeyModel("f", "F", 0x46, 1.0));
            r2.Keys.Add(new KeyModel("g", "G", 0x47, 1.0));
            r2.Keys.Add(new KeyModel("h", "H", 0x48, 1.0));
            r2.Keys.Add(new KeyModel("j", "J", 0x4A, 1.0));
            r2.Keys.Add(new KeyModel("k", "K", 0x4B, 1.0));
            r2.Keys.Add(new KeyModel("l", "L", 0x4C, 1.0));
            r2.Keys.Add(new KeyModel(";", ":", 0xBA, 1.0));
            r2.Keys.Add(new KeyModel("'", "\"", 0xDE, 1.0));
            r2.Keys.Add(new KeyModel("↵ Enter", Win32.VK_RETURN, 1.9, KeyActionType.VirtualKey));
            layout.Rows.Add(r2);

            // Row 3: Shift + ZXCV + Arrows
            var r3 = new KeyRow();
            r3.Keys.Add(new KeyModel("⇧ Shift", Win32.VK_SHIFT, 2.0, KeyActionType.Modifier) { Tag = "Shift" });
            r3.Keys.Add(new KeyModel("z", "Z", 0x5A, 1.0));
            r3.Keys.Add(new KeyModel("x", "X", 0x58, 1.0));
            r3.Keys.Add(new KeyModel("c", "C", 0x43, 1.0));
            r3.Keys.Add(new KeyModel("v", "V", 0x56, 1.0));
            r3.Keys.Add(new KeyModel("b", "B", 0x42, 1.0));
            r3.Keys.Add(new KeyModel("n", "N", 0x4E, 1.0));
            r3.Keys.Add(new KeyModel("m", "M", 0x4D, 1.0));
            r3.Keys.Add(new KeyModel(",", "<", 0xBC, 1.0));
            r3.Keys.Add(new KeyModel(".", ">", 0xBE, 1.0));
            r3.Keys.Add(new KeyModel("/", "?", 0xBF, 1.0));
            r3.Keys.Add(new KeyModel("▲", Win32.VK_UP, 1.1, KeyActionType.Navigation));
            r3.Keys.Add(new KeyModel("⇧ Shift", Win32.VK_SHIFT, 1.4, KeyActionType.Modifier) { Tag = "Shift" });
            layout.Rows.Add(r3);

            // Row 4: Modifiers + Voice Input + Spacebar + Arrows
            var r4 = new KeyRow();
            r4.Keys.Add(new KeyModel("Ctrl", Win32.VK_CONTROL, 1.4, KeyActionType.Modifier) { Tag = "Ctrl" });
            r4.Keys.Add(new KeyModel("Win", Win32.VK_LWIN, 1.1, KeyActionType.Modifier) { Tag = "Win" });
            r4.Keys.Add(new KeyModel("Alt", Win32.VK_MENU, 1.1, KeyActionType.Modifier) { Tag = "Alt" });
            r4.Keys.Add(new KeyModel("🎙", 0, 1.0, KeyActionType.Macro) { MacroCommand = "VOICE_INPUT" });
            r4.Keys.Add(new KeyModel("Space", Win32.VK_SPACE, 4.8, KeyActionType.VirtualKey));
            r4.Keys.Add(new KeyModel("Alt", Win32.VK_MENU, 1.1, KeyActionType.Modifier) { Tag = "Alt" });
            r4.Keys.Add(new KeyModel("◀", Win32.VK_LEFT, 1.1, KeyActionType.Navigation));
            r4.Keys.Add(new KeyModel("▼", Win32.VK_DOWN, 1.1, KeyActionType.Navigation));
            r4.Keys.Add(new KeyModel("▶", Win32.VK_RIGHT, 1.1, KeyActionType.Navigation));
            r4.Keys.Add(new KeyModel("Ctrl", Win32.VK_CONTROL, 1.4, KeyActionType.Modifier) { Tag = "Ctrl" });
            layout.Rows.Add(r4);

            return layout;
        }

        /// <summary>
        /// Windows 10 PIN & Numeric Pad (Lock Screen Optimized)
        /// </summary>
        public static KeyboardLayout CreateNumpadPinLayout()
        {
            var layout = new KeyboardLayout
            {
                Id = "numpad",
                Name = "Windows 10 PIN / Numpad",
                Description = "Large keypad designed for quick numeric PIN entry on Lock Screen and POS"
            };

            // Row 1
            var r1 = new KeyRow();
            r1.Keys.Add(new KeyModel("7", 0x37, 1.0));
            r1.Keys.Add(new KeyModel("8", 0x38, 1.0));
            r1.Keys.Add(new KeyModel("9", 0x39, 1.0));
            r1.Keys.Add(new KeyModel("⌫", Win32.VK_BACK, 1.2, KeyActionType.VirtualKey));
            layout.Rows.Add(r1);

            // Row 2
            var r2 = new KeyRow();
            r2.Keys.Add(new KeyModel("4", 0x34, 1.0));
            r2.Keys.Add(new KeyModel("5", 0x35, 1.0));
            r2.Keys.Add(new KeyModel("6", 0x36, 1.0));
            r2.Keys.Add(new KeyModel("Clear", Win32.VK_CLEAR, 1.2, KeyActionType.VirtualKey));
            layout.Rows.Add(r2);

            // Row 3
            var r3 = new KeyRow();
            r3.Keys.Add(new KeyModel("1", 0x31, 1.0));
            r3.Keys.Add(new KeyModel("2", 0x32, 1.0));
            r3.Keys.Add(new KeyModel("3", 0x33, 1.0));
            r3.Keys.Add(new KeyModel("Tab", Win32.VK_TAB, 1.2, KeyActionType.VirtualKey));
            layout.Rows.Add(r3);

            // Row 4
            var r4 = new KeyRow();
            r4.Keys.Add(new KeyModel("ABC", 0, 1.0, KeyActionType.Macro) { MacroCommand = "SWITCH_LAYOUT:touch" });
            r4.Keys.Add(new KeyModel("0", 0x30, 1.0));
            r4.Keys.Add(new KeyModel(".", 0xBE, 1.0));
            r4.Keys.Add(new KeyModel("↵ Enter", Win32.VK_RETURN, 1.2, KeyActionType.VirtualKey));
            layout.Rows.Add(r4);

            return layout;
        }

        /// <summary>
        /// Windows 10 Symbols & Numbers (&123)
        /// </summary>
        public static KeyboardLayout CreateSymbolsLayout()
        {
            var layout = new KeyboardLayout
            {
                Id = "symbol",
                Name = "Windows 10 Symbols (&123)",
                Description = "Numbers, brackets, currency, and punctuation"
            };

            // Row 1: 1 2 3 4 5 6 7 8 9 0
            var r1 = new KeyRow();
            r1.Keys.Add(new KeyModel("1", 0x31, 1.0));
            r1.Keys.Add(new KeyModel("2", 0x32, 1.0));
            r1.Keys.Add(new KeyModel("3", 0x33, 1.0));
            r1.Keys.Add(new KeyModel("4", 0x34, 1.0));
            r1.Keys.Add(new KeyModel("5", 0x35, 1.0));
            r1.Keys.Add(new KeyModel("6", 0x36, 1.0));
            r1.Keys.Add(new KeyModel("7", 0x37, 1.0));
            r1.Keys.Add(new KeyModel("8", 0x38, 1.0));
            r1.Keys.Add(new KeyModel("9", 0x39, 1.0));
            r1.Keys.Add(new KeyModel("0", 0x30, 1.0));
            layout.Rows.Add(r1);

            // Row 2: @ # $ % & * - + ( )
            var r2 = new KeyRow();
            r2.Keys.Add(new KeyModel("@", 0x32, 1.0, KeyActionType.Character) { PrimaryLabel = "@" });
            r2.Keys.Add(new KeyModel("#", 0x33, 1.0, KeyActionType.Character) { PrimaryLabel = "#" });
            r2.Keys.Add(new KeyModel("$", 0x34, 1.0, KeyActionType.Character) { PrimaryLabel = "$" });
            r2.Keys.Add(new KeyModel("%", 0x35, 1.0, KeyActionType.Character) { PrimaryLabel = "%" });
            r2.Keys.Add(new KeyModel("&", 0x37, 1.0, KeyActionType.Character) { PrimaryLabel = "&" });
            r2.Keys.Add(new KeyModel("*", 0x38, 1.0, KeyActionType.Character) { PrimaryLabel = "*" });
            r2.Keys.Add(new KeyModel("-", 0xBD, 1.0, KeyActionType.Character) { PrimaryLabel = "-" });
            r2.Keys.Add(new KeyModel("+", 0xBB, 1.0, KeyActionType.Character) { PrimaryLabel = "+" });
            r2.Keys.Add(new KeyModel("(", 0x39, 1.0, KeyActionType.Character) { PrimaryLabel = "(" });
            r2.Keys.Add(new KeyModel(")", 0x30, 1.0, KeyActionType.Character) { PrimaryLabel = ")" });
            layout.Rows.Add(r2);

            // Row 3: ! " ' : ; / ? ⌫
            var r3 = new KeyRow();
            r3.Keys.Add(new KeyModel("!", 0x31, 1.1, KeyActionType.Character) { PrimaryLabel = "!" });
            r3.Keys.Add(new KeyModel("\"", 0xDE, 1.1, KeyActionType.Character) { PrimaryLabel = "\"" });
            r3.Keys.Add(new KeyModel("'", 0xDE, 1.1, KeyActionType.Character) { PrimaryLabel = "'" });
            r3.Keys.Add(new KeyModel(":", 0xBA, 1.1, KeyActionType.Character) { PrimaryLabel = ":" });
            r3.Keys.Add(new KeyModel(";", 0xBA, 1.1, KeyActionType.Character) { PrimaryLabel = ";" });
            r3.Keys.Add(new KeyModel("/", 0xBF, 1.1, KeyActionType.Character) { PrimaryLabel = "/" });
            r3.Keys.Add(new KeyModel("?", 0xBF, 1.1, KeyActionType.Character) { PrimaryLabel = "?" });
            r3.Keys.Add(new KeyModel("⌫", Win32.VK_BACK, 1.6, KeyActionType.VirtualKey));
            layout.Rows.Add(r3);

            // Row 4: ABC, ,, Space, ., Enter
            var r4 = new KeyRow();
            r4.Keys.Add(new KeyModel("ABC", 0, 1.4, KeyActionType.Macro) { MacroCommand = "SWITCH_LAYOUT:touch" });
            r4.Keys.Add(new KeyModel(",", 0xBC, 1.0));
            r4.Keys.Add(new KeyModel("Space", Win32.VK_SPACE, 5.0, KeyActionType.VirtualKey));
            r4.Keys.Add(new KeyModel(".", 0xBE, 1.0));
            r4.Keys.Add(new KeyModel("↵ Enter", Win32.VK_RETURN, 1.6, KeyActionType.VirtualKey));
            layout.Rows.Add(r4);

            return layout;
        }
    }
}
