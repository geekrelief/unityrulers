using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loqheart.Utility
{
    public enum RulerAngleMode
    {
        DirectionCosines,
        PlaneProjection,
        POV
    }

    public enum RulerExDataMode
    {
        None,
        Distance,
        Angle
    }

    // Ruler editor creates rulers between transforms and displays an arrow from source to destination along with distance between them
    [Serializable]
    public class Ruler
    {
        static Vector3 X = new Vector3(1, 0, 0);
        static Vector3 Y = new Vector3(0, 1, 0);
        static Vector3 Z = new Vector3(0, 0, 1);

        public bool isVisible = true;
        public Transform a;
        public Transform b;
        public Color color;
        public Color textColor;
        public RulerExDataMode exDataMode = RulerExDataMode.None;

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

        public Vector3 GetAngles(RulerAngleMode mode)
        {
            var angles = Vector3.zero;
            var unit = delta.normalized;
            switch (mode)
            {
                case RulerAngleMode.DirectionCosines:
                    angles.x = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, X), -1, 1));
                    angles.y = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, Y), -1, 1));
                    angles.z = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, Z), -1, 1));
                    angles = angles * Mathf.Rad2Deg;
                    break;

                case RulerAngleMode.PlaneProjection:
                    var xy = new Vector3(unit.x, unit.y, 0f);
                    angles.x = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, xy), -1, 1));
                    var yz = new Vector3(0f, unit.y, unit.z);
                    angles.y = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, yz), -1, 1));
                    var xz = new Vector3(unit.x, 0f, unit.z);
                    angles.z = Mathf.Acos(Mathf.Clamp(Vector3.Dot(unit, xz), -1, 1));
                    angles = angles * Mathf.Rad2Deg;
                    break;

                case RulerAngleMode.POV:
                    //angles = (Quaternion.Inverse(a.rotation) * Quaternion.LookRotation(unit, a.up)).eulerAngles;
                    angles = (Quaternion.Inverse(a.rotation) * Quaternion.LookRotation(unit, b.up)).eulerAngles;
                    if (angles.x > 180f)
                    {
                        angles.x -= 360f;
                    }
                    if (angles.y > 180f)
                    {
                        angles.y -= 360f;
                    }
                    if (angles.z > 180f)
                    {
                        angles.z -= 360f;
                    }
                    break;
            }
            return angles;
        }
    }

    // a component to contain the data in the scene for the ruler editor
    [Serializable]
    public class RulerData : MonoBehaviour
    {
        public Ruler[] rulers;

        public bool showTooltips = false;
        public bool enableShortcuts = true;

        public int fontSize = 14;
        public Color textColor = new Color(.1f, .1f, .1f);
        public Color rulerColor = new Color(1f, .75f, 0f);
        public int rulerThickness = 2;

        public Transform filterTransform;

        public RulerAngleMode angleMode = RulerAngleMode.DirectionCosines;

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
