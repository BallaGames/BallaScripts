using Balla.Input;
using System;
using Unity.Profiling;
using UnityEngine;
namespace Balla.Core
{


    public class GameCore : MonoBehaviour
    {

        static readonly ProfilerMarker frameMarker = new("GameCore.OnFrame"), afterFrameMarker = new("GameCore.AfterFrame"), timestepMarker = new("GameCore.Timestep");


        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InitialiseAtRuntime()
        {
            GameObject go = new GameObject("LUNAR MANAGER");
            Core = go.AddComponent<GameCore>();
            PlayerInput.InputManager = go.AddComponent<PlayerInput>();
            InputManager.Initialised();
            DontDestroyOnLoad(go);
        }
        internal static GameCore Core { get; private set; }
        internal static PlayerInput InputManager => PlayerInput.InputManager;
        public delegate void Frame();
        public static Frame frame, afterFrame, timestep;

        public static float TimeMultiplier = 1f;
        public static float Delta = Time.fixedDeltaTime;
        public bool allowManualTimeManip;
        private void OnGUI()
        {
            if (allowManualTimeManip)
                TimeManipGUI();
        }
        void TimeManipGUI()
        {
            float scaleX = Screen.width / 1280f;
            float scaleY = Screen.height / 720f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));
            GUILayout.BeginVertical();
            GUILayout.Label($"Timestep:{TimeMultiplier:0.0}");
            TimeMultiplier = GUILayout.HorizontalSlider(TimeMultiplier, 0.01f, 2f, GUILayout.Width(100));
            GUILayout.EndVertical();
        }



        internal static void Subscribe(IBallaMessages script)
        {
            frame += script.OnFrame;
            afterFrame += script.AfterFrame;
            timestep += script.Timestep;
        }
        internal static void Unsubscribe(IBallaMessages script)
        {
            frame -= script.OnFrame;
            afterFrame -= script.AfterFrame;
            timestep -= script.Timestep;
        }

        private void Update()
        {
            frameMarker.Begin();
            frame?.Invoke();
            frameMarker.End();
        }
        private void FixedUpdate()
        {
            timestepMarker.Begin();
            Delta = Time.fixedDeltaTime * TimeMultiplier;
            timestep?.Invoke();
            Physics.Simulate(Delta);
            //Physics2D.Simulate(Delta);
            timestepMarker.End();
        }
        private void LateUpdate()
        {
            afterFrameMarker.Begin();
            afterFrame?.Invoke();
            afterFrameMarker.End();
        }
    }
}