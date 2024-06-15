using System.Collections.Generic;
using ExcelLibrary.BinaryFileFormat;
using TMPro;
using UnityEngine;

namespace Game.BattleTable.UI
{
    public class SelectionTarget : MonoBehaviour
    {
        public void addNumber(int number)
        {
            var numberText = Instantiate(numberTemplate.gameObject, numberRoot).GetComponent<TextMeshPro>();
            numberText.gameObject.SetActive(true);
            numberText.text = number.ToString();
            numbers.Add(numberText);
        }
        public void updateNumberPosition(float deltaTime)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                var number = numbers[i];
                var targetPos = getTargetPosition(i);
                var localPos = number.transform.localPosition;
                localPos = localPos * 0.9f + targetPos * 0.1f;
                number.transform.localPosition = localPos;
            }
        }
        private Vector3 getTargetPosition(int index)
        {
            int count = numbers.Count;
            int division = count + 1;
            float angleDiff = maxDegrees / division;
            float angle = angleDiff * (index - (count - 1) * 0.5f);
            float rad = Mathf.Deg2Rad * angle;
            return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
        }
        [SerializeField]
        private float maxDegrees = 180;
        [SerializeField]
        private float radius = 1;
        [SerializeField]
        private TextMeshPro numberTemplate;
        [SerializeField]
        private Transform numberRoot;
        [SerializeField]
        private List<TextMeshPro> numbers;
    }
}
