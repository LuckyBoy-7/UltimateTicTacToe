using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky.Framework.Inputs_
{
    /// <summary>
    /// 对VirtualButton的管理
    /// </summary>
    public static class Inputs
    {
        public static void Initialize()
        {
            Left = new VirtualButton(Settings.Left, 0);
            Right = new VirtualButton(Settings.Right, 0);
            Up = new VirtualButton(Settings.Up, 0);
            Down = new VirtualButton(Settings.Down, 0);
        }

        public static void Update()
        {
            foreach (var button in _inputs)
            {
                button.Update();
            }
        }

        public static void Register(VirtualInput input) => _inputs.Add(input);
        public static void DeRegister(VirtualInput input) => _inputs.Remove(input);
        private static readonly List<VirtualInput> _inputs = new();
        
        
        public static VirtualButton Left;
        public static VirtualButton Right;
        public static VirtualButton Up;
        public static VirtualButton Down;


        public static KeyCode GetCurrentPressedKey()
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                    return key;
            }

            return KeyCode.None;
        }
    }
}