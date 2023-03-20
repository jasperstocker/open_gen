using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opengen.simple
{

   [Serializable]
    public class InputKeyItem
    {
        public string name;
        public KeyCode code;
        public float intertia = 0;
        public float value = 0;
        public float maxValue = 1;
    }
    
    [Serializable]
    public class InputKeyAxis
    {
        public string name;
        public KeyCode positiveCode;
        public KeyCode negativeCode;
        public float intertia = 0;
        public float value = 0;
        public float maxValue = 1;
        public float minValue = -1;
    }
    
    public class SimpleInput : MonoBehaviour
    {
        public bool showDebug;
        public List<InputKeyItem> keyInputs = new List<InputKeyItem>();
        public List<InputKeyAxis> axisInputs = new List<InputKeyAxis>();

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            int inputKeyCount = keyInputs.Count;
            for (int i = 0; i < inputKeyCount; i++)
            {
                InputKeyItem item = keyInputs[i];

                if (Input.GetKey(item.code))
                {
                    item.value = Mathf.Lerp(item.value, item.maxValue, 1f);
                }
                else
                {
                    item.value = Mathf.Lerp(item.value, 0, 1f);
                }
            }
            
            int inputAxisCount = axisInputs.Count;
            for (int i = 0; i < inputAxisCount; i++)
            {
                InputKeyAxis item = axisInputs[i];

                bool positive = Input.GetKey(item.positiveCode);
                bool negative = Input.GetKey(item.negativeCode);

                if (Input.GetKey(item.negativeCode))
                {
                    item.value = Mathf.Lerp(item.value, item.maxValue, 1f);
                }
                else
                {
                    item.value = Mathf.Lerp(item.value, 0, 1f);
                }
            }
        }

        private void OnGUI()
        {
            int inputKeyCount = keyInputs.Count;
            
            GUILayout.BeginVertical("box");

            for (int i = 0; i < inputKeyCount; i++)
            {
                InputKeyItem item = keyInputs[i];
                GUILayout.BeginHorizontal("box");
                
                GUILayout.Label($"{item.name}{item.value}");
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
        }
    }
}