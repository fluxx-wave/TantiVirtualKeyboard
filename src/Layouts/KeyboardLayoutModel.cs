using System.Collections.Generic;

namespace HotVirtualKeyboard.Layouts
{
    public enum KeyActionType
    {
        Character,
        VirtualKey,
        Modifier,
        Function,
        Macro,
        Navigation
    }

    public class KeyModel
    {
        public string PrimaryLabel { get; set; }
        public string ShiftLabel { get; set; }
        public ushort VkCode { get; set; }
        public double WidthRatio { get; set; }
        public KeyActionType ActionType { get; set; }
        public string MacroCommand { get; set; }
        public bool IsRepeatable { get; set; }
        public string Tag { get; set; }

        public KeyModel()
        {
            WidthRatio = 1.0;
            ActionType = KeyActionType.Character;
            IsRepeatable = true;
        }

        public KeyModel(string primary, ushort vk, double width, KeyActionType type)
        {
            PrimaryLabel = primary;
            VkCode = vk;
            WidthRatio = width;
            ActionType = type;
            IsRepeatable = true;
        }

        public KeyModel(string primary, ushort vk, double width)
        {
            PrimaryLabel = primary;
            VkCode = vk;
            WidthRatio = width;
            ActionType = KeyActionType.Character;
            IsRepeatable = true;
        }

        public KeyModel(string primary, ushort vk)
        {
            PrimaryLabel = primary;
            VkCode = vk;
            WidthRatio = 1.0;
            ActionType = KeyActionType.Character;
            IsRepeatable = true;
        }

        public KeyModel(string primary, string shift, ushort vk, double width)
        {
            PrimaryLabel = primary;
            ShiftLabel = shift;
            VkCode = vk;
            WidthRatio = width;
            ActionType = KeyActionType.Character;
            IsRepeatable = true;
        }

        public KeyModel(string primary, string shift, ushort vk)
        {
            PrimaryLabel = primary;
            ShiftLabel = shift;
            VkCode = vk;
            WidthRatio = 1.0;
            ActionType = KeyActionType.Character;
            IsRepeatable = true;
        }
    }

    public class KeyRow
    {
        public List<KeyModel> Keys { get; set; }

        public KeyRow()
        {
            Keys = new List<KeyModel>();
        }
    }

    public class KeyboardLayout
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<KeyRow> Rows { get; set; }

        public KeyboardLayout()
        {
            Rows = new List<KeyRow>();
        }
    }
}
