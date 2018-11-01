using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TouhouHeartstone.Frontend
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu("UI/TextMeshExtend")]
    public class TextMeshExtend : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        string _text;

        /// <summary>
        /// 文本
        /// </summary>
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Redraw();
            }
        }

        [SerializeField]
        [Range(1, 500)]
        float sizePrescaler = 100;

        [SerializeField]
        int sortingOrder;

        MeshFilter _filter;
        MeshFilter filter
        {
            get
            {
                if (_filter == null) _filter = GetComponent<MeshFilter>();
                return _filter;
            }
        }

        RectTransform _rectTransform;
        RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        MeshRenderer _meshRenderer;
        MeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }

        TextGenerator _generator;
        TextGenerator generator
        {
            get
            {
                _generator = _generator ?? new TextGenerator();
                return _generator;
            }
        }

        Mesh _mesh;
        Mesh mesh
        {
            get
            {
                _mesh = _mesh ?? new Mesh();
                return _mesh;
            }
        }



        [SerializeField]
        TextGenerationSettingsInternal settingsInternal = new TextGenerationSettingsInternal()
        {
            color = Color.black,
            fontSize = 16,
            lineSpacing = 1,
            scaleFactor = 1,
        };

        private void Update()
        {
            Redraw();
        }

        private void Redraw()
        {
            generator.Populate(_text, convertSetting(settingsInternal));

            var vts = generator.GetVerticesArray();
            VertsToMesh(mesh, vts);

            filter.mesh = mesh;
            meshRenderer.sortingOrder = sortingOrder;
        }

        public TextGenerationSettings convertSetting(TextGenerationSettingsInternal set)
        {
            TextGenerationSettings settings = new TextGenerationSettings();
#if false // reflection
            var ta = typeof(TextGenerationSettingsInternal);
            var tb = typeof(TextGenerationSettings);
            var apts = ta.GetFields();

            foreach (var item in apts)
            {
                var val = item.GetValue(set);
                var ptb = tb.GetField(item.Name);

                ptb.SetValue(settings, val);
            }
#else
            settings.alignByGeometry = set.alignByGeometry;
            settings.color = set.color;
            settings.font = set.font;
            settings.fontSize = set.fontSize;
            settings.fontStyle = set.fontStyle;
            settings.generateOutOfBounds = set.generateOutOfBounds;
            settings.generationExtents = rectTransform.rect.size * sizePrescaler;
            settings.horizontalOverflow = set.horizontalOverflow;
            settings.lineSpacing = set.lineSpacing;
            settings.pivot = rectTransform.pivot;
            settings.resizeTextForBestFit = set.resizeTextForBestFit;
            settings.resizeTextMaxSize = set.resizeTextMaxSize;
            settings.resizeTextMinSize = set.resizeTextMinSize;
            settings.richText = set.richText;
            settings.scaleFactor = set.scaleFactor;
            settings.textAnchor = set.textAnchor;
            settings.updateBounds = set.updateBounds;
            settings.verticalOverflow = set.verticalOverflow;
#endif
            return settings;
        }

        private void VertsToMesh(Mesh msh, UIVertex[] vts)
        {
            msh.Clear();
            msh.SetVertices(vts.Select(e => e.position / sizePrescaler).ToList());
            msh.SetTangents(vts.Select(e => e.tangent).ToList());
            msh.SetNormals(vts.Select(e => e.normal).ToList());
            msh.SetColors(vts.Select(e => e.color).ToList());
            msh.SetUVs(0, vts.Select(e => e.uv0).ToList());
            msh.SetUVs(1, vts.Select(e => e.uv1).ToList());
            msh.SetUVs(2, vts.Select(e => e.uv2).ToList());
            msh.SetUVs(3, vts.Select(e => e.uv3).ToList());
            msh.triangles = getTriangle(vts.Length);
        }

        int[] getTriangle(int count)
        {
            List<int> intList = new List<int>();

            for (int i = 0; i < count; i += 4)
            {
                intList.AddRange(new int[] { 0 + i, 1 + i, 2 + i, 0 + i, 2 + i, 3 + i });
            }

            return intList.ToArray();
        }
    }

    [Serializable]
    public struct TextGenerationSettingsInternal
    {
        public Font font;
        [HideInInspector]
        public Vector2 pivot;
        [HideInInspector]
        public Vector2 generationExtents;
        public HorizontalWrapMode horizontalOverflow;
        public VerticalWrapMode verticalOverflow;
        public bool updateBounds;
        public int resizeTextMaxSize;
        public int resizeTextMinSize;
        public bool generateOutOfBounds;
        public bool resizeTextForBestFit;
        public TextAnchor textAnchor;
        public FontStyle fontStyle;
        public float scaleFactor;
        public bool richText;
        public float lineSpacing;
        public int fontSize;
        public Color color;
        public bool alignByGeometry;
    }
}