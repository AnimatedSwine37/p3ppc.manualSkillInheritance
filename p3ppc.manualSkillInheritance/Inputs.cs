using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.manualSkillInheritance
{
    internal unsafe class Inputs
    {
        internal static InputStruct* _inputs;
        private static Dictionary<Input, DateTime> _timePressed = new();

        internal static void Initialise(InputStruct* inputs)
        {
            _inputs = inputs;
        }

        internal static bool IsHeld(Input input, TimeSpan cooldown, TimeSpan initialDelay)
        {
            InputFlag flag = (InputFlag)(1 << (int)input);
            if (_inputs->Pressed.HasFlag(flag) || _inputs->ThumbstickPressed.HasFlag(flag))
            {
                if (_timePressed.ContainsKey(input))
                    _timePressed[input] = DateTime.Now + initialDelay;
                else
                    _timePressed.Add(input, DateTime.Now + initialDelay);
                return true;
            }

            if (_inputs->Held.HasFlag(flag) || _inputs->ThumbstickHeld.HasFlag(flag))
            {
                if (!_timePressed.TryGetValue(input, out var timePressed))
                {
                    _timePressed.Add(input, DateTime.Now + initialDelay);
                    timePressed = DateTime.Now + initialDelay;
                }
                if (DateTime.Now >= timePressed + cooldown)
                {
                    _timePressed[input] = DateTime.Now;
                    return true;
                }
                return false;
            }
            else
            {
                if (_timePressed.ContainsKey(input))
                    _timePressed.Remove(input);
            }

            return false;
        }

        [Flags]
        internal enum InputFlag
        {
            Start = 1 << Input.Start,
            Up = 1 << Input.Up,
            Right = 1 << Input.Right,
            Down = 1 << Input.Down,
            Left = 1 << Input.Left,
            Confirm = 1 << Input.Confirm,
            Escape = 1 << Input.Escape,
            SubMenu = 1 << Input.SubMenu,
        }

        internal enum Input
        {
            Start = 3,
            Up = 4,
            Right = 5,
            Down = 6,
            Left = 7,
            Confirm = 13,
            Escape = 14,
            SubMenu = 15,
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputStruct
        {
            [FieldOffset(0)]
            internal InputFlag Pressed;

            [FieldOffset(8)]
            internal InputFlag Held;

            [FieldOffset(16)]
            internal InputFlag ThumbstickHeld;

            [FieldOffset(20)]
            internal InputFlag ThumbstickPressed;

            [FieldOffset(160)]
            internal fixed float HeldDuration[16];
        }
    }
}
