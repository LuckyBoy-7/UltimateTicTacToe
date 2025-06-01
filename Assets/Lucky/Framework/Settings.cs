using Lucky.Framework.Inputs_;
using UnityEngine;

namespace Lucky.Framework
{
    public static class Settings
    {

        public static void Initialize()
        {
            SetDefaultKeyboardControls(false);
        }

        public static void SetDefaultKeyboardControls(bool reset)
        {
            if (reset || Esc.Keys.Count <= 0)
            {
                Esc.Keys.Clear();
                Esc.Add(KeyCode.Escape);
            }

            if (reset || Pause.Keys.Count <= 0)
            {
                Pause.Keys.Clear();
                Pause.Add(KeyCode.Escape);
            }

            if (reset || Left.Keys.Count <= 0)
            {
                Left.Keys.Clear();
                // Left.Add(KeyCode.LeftArrow);
                Left.Add(KeyCode.J);
            }

            if (reset || Right.Keys.Count <= 0)
            {
                Right.Keys.Clear();
                // Right.Add(KeyCode.RightArrow);
                Right.Add(KeyCode.L);
            }

            if (reset || Down.Keys.Count <= 0)
            {
                Down.Keys.Clear();
                // Down.Add(KeyCode.DownArrow);
                Down.Add(KeyCode.K);
            }

            if (reset || Up.Keys.Count <= 0)
            {
                Up.Keys.Clear();
                // Up.Add(KeyCode.UpArrow);
                Up.Add(KeyCode.I);
            }
        }


        public static Binding Esc = new Binding();

        public static Binding Pause = new Binding();

        public static Binding Left = new Binding();

        public static Binding Right = new Binding();

        public static Binding Down = new Binding();
        public static Binding Up = new Binding();
    }
}