using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace IGensoukyo
{
    [CreateAssetMenu(menuName = "Custom/ExcelDataSetList")]
    public class ExcelDataSetList : ScriptableObject
    {
        public const string FileName = "ExcelList";

        [SerializeField]
        string[] fileList = new string[0];

        public string[] FileList { set => fileList = value; get => fileList; }

        public static ExcelDataSetList Instance
        {
            get
            {
                return Resources.Load<ExcelDataSetList>(FileName);
            }
        }
    }
}
