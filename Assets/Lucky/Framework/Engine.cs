using Lucky.Framework.Inputs_;
using Lucky.Kits.Interactive;
using Lucky.Kits.Managers;
using Unity.VisualScripting;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Lucky.Framework
{
    /// <summary>
    /// 在设置界面保证Engine最先调用, 然后Engine去初始化各种Manager, 以保证更新顺序正确
    /// </summary>
    public class Engine : ManagedBehaviour
    {
        public const float OneFrameTime = 1 / 60f;

        protected virtual void Awake()
        {
            Time.fixedDeltaTime = OneFrameTime;
            Settings.Initialize();
            Inputs.Initialize();
            GameCursor.Instance = this.AddComponent<GameCursor>();
        }

        protected virtual void Update()
        {
            // Input有最高优先级
            Inputs.Update();
        }

        protected void FixedUpdate()
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    Time.timeScale /= 2;
                else if (Input.GetKeyDown(KeyCode.E))
                    Time.timeScale *= 2;
            }
#endif
        }
    }
}