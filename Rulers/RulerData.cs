using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loqheart.Utility
{
    // Ruler editor creates rulers between transforms and displays an arrow from source to destination along with distance between them
    [Serializable]
    public class Ruler
    {
        public bool isVisible = true;
        public Transform a;
        public Transform b;
        public Color color;

        public Ruler()
        {
        }

        public bool isValid
        {
            get
            {
                return a != null && b != null;
            }
        }

        public Vector3 delta
        {
            get
            {
                if (isValid)
                {
                    return b.position - a.position;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }
    }

    // a component to contain the data in the scene for the ruler editor
    [Serializable]
    public class RulerData : MonoBehaviour
    {
        public Ruler[] rulers;

        public bool showTooltips = false;

        public int fontSize = 20;
        public Color fontColor = new Color(.8f, .8f, .8f);
        public Color rulerColor = new Color(1f, .75f, 0f);
        public int rulerThickness = 2;

        public Transform filterTransform;

        public RulerData()
        {
            rulers = new Ruler[0];
        }

        public void Add(Ruler r, int index = -1)
        {
            var newRulers = new Ruler[rulers.Length + 1];

            for (int i = 0, j = 0; i < newRulers.Length; ++i)
            {
                if (index != -1 && i == index)
                {
                    newRulers[i] = r;
                }
                else
                {
                    if (j < rulers.Length)
                    {
                        newRulers[i] = rulers[j];
                        ++j;
                    }
                    else
                    {
                        newRulers[i] = r;
                    }
                }
            }
            rulers = newRulers;
        }

        public void RemoveAt(int removeIndex)
        {
            var newRulers = new Ruler[rulers.Length - 1];
            for (int i = 0, j = 0; i < rulers.Length; ++i)
            {
                if (i != removeIndex)
                {
                    newRulers[j] = rulers[i];
                    ++j;
                }
            }

            rulers = newRulers;
        }
    }
}
