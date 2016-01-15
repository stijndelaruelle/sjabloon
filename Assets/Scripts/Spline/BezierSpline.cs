using System.Collections.Generic;
using UnityEngine;

namespace Sjabloon
{
    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }

    public static class Bezier
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            //Quadratic Beziér curve
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }
    }

    public class BezierSpline : Spline
    {
        [SerializeField]
        private List<Vector3> m_Points;

        [SerializeField]
        private List<BezierControlPointMode> m_Modes;

        [SerializeField]
        private bool m_Loop;

        private bool m_IsInitialized = false;

        public void Awake()
        {
            if (!m_IsInitialized)
                Initizalize();
        }

        private void Initizalize()
        {
            m_IsInitialized = true;

            if (m_Points == null)
                m_Points = new List<Vector3>();

            if (m_Modes == null)
                m_Modes = new List<BezierControlPointMode>();
        }

        private void EnforceMode(int index)
        {
            if (!m_IsInitialized)
                Initizalize();

            int modeIndex = (index + 1) / 3;

            if (modeIndex >= m_Modes.Count)
                return;

            BezierControlPointMode mode = m_Modes[modeIndex];
            if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == m_Modes.Count - 1)
            {
                return;
            }

            //Determine which points we need to adjust
            int middleIndex = modeIndex * 3;
            int fixedIndex = 0;
            int enforcedIndex = 0;

            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = m_Points.Count - 2;
                }
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= m_Points.Count)
                {
                    enforcedIndex = 1;
                }

            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= m_Points.Count)
                {
                    fixedIndex = 1;
                }
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = m_Points.Count - 2;
                }
            }

            //Actually enfore the mode
            Vector3 middle = m_Points[middleIndex];
            Vector3 enforcedTangent = new Vector3();

            switch (mode)
            {
                case BezierControlPointMode.Mirrored:
                    enforcedTangent = middle - m_Points[fixedIndex];
                    break;

                case BezierControlPointMode.Aligned:
                    enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_Points[enforcedIndex]);
                    break;

                case BezierControlPointMode.Free:
                    break;

                default:
                    break;
            }

            m_Points[enforcedIndex] = middle + enforcedTangent;
        }

        //Special unity funcion
        public void Reset()
        {
            m_IsInitialized = false;

            if (!m_IsInitialized)
                Initizalize();

            m_Points.Clear();
        }

        //Mutators
        public void AddCurve()
        {
            if (!m_IsInitialized)
                Initizalize();

            Vector3 lastPoint = new Vector3();
            BezierControlPointMode lastMode = BezierControlPointMode.Free;

            if (m_Points.Count > 0)
            {
                lastPoint = m_Points[m_Points.Count - 1];
                lastMode = m_Modes[m_Modes.Count - 1];
            }

            //Add points
            m_Points.Add(new Vector3(lastPoint.x + 1.0f, 0.0f, 0.0f));
            m_Points.Add(new Vector3(lastPoint.x + 2.0f, 0.0f, 0.0f));
            m_Points.Add(new Vector3(lastPoint.x + 3.0f, 0.0f, 0.0f));

            //Add a control mode
            m_Modes.Add(lastMode);

            //First curve: add extra stuff
            if (m_Points.Count == 3)
            {
                m_Points.Add(new Vector3(lastPoint.x + 4.0f, 0.0f, 0.0f));
                m_Modes.Add(lastMode);
            }

            //Enforce the mode on the last point
            EnforceMode(m_Points.Count - 4);

            if (m_Loop)
            {
                m_Points[m_Points.Count - 1] = m_Points[0];
                m_Modes[m_Modes.Count - 1] = m_Modes[0];
                EnforceMode(0);
            }
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return;

            //If we are the point on 2 curves, move the 2 control points along
            if (index % 3 == 0)
            {
                Vector3 delta = point - m_Points[index];
                if (m_Loop)
                {
                    if (index == 0)
                    {
                        m_Points[1] += delta;
                        m_Points[m_Points.Count - 2] += delta;
                        m_Points[m_Points.Count - 1] = point;
                    }
                    else if (index == m_Points.Count - 1)
                    {
                        m_Points[0] = point;
                        m_Points[1] += delta;
                        m_Points[index - 1] += delta;
                    }
                    else
                    {
                        m_Points[index - 1] += delta;
                        m_Points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        m_Points[index - 1] += delta;
                    }

                    if (index + 1 < m_Points.Count)
                    {
                        m_Points[index + 1] += delta;
                    }
                }
            }

            m_Points[index] = point;
            EnforceMode(index);
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return;

            int modeIndex = (index + 1) / 3;
            m_Modes[modeIndex] = mode;
            if (m_Loop)
            {
                if (modeIndex == 0)
                {
                    m_Modes[m_Modes.Count - 1] = mode;
                }
                else if (modeIndex == m_Modes.Count - 1)
                {
                    m_Modes[0] = mode;
                }
            }

            EnforceMode(index);
        }

        public void SetLoop(bool value)
        {
            if (!m_IsInitialized)
                Initizalize();

            m_Loop = value;
            if (value == true)
            {
                m_Modes[m_Modes.Count - 1] = m_Modes[0];
                SetControlPoint(0, m_Points[0]);
            }
        }

        //Accesors
        public Vector3 GetControlPoint(int index)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return new Vector3();

            return m_Points[index];
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return BezierControlPointMode.Free;

            return m_Modes[(index + 1) / 3];
        }

        public override Vector3 GetPoint(float t)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (m_Points.Count < 4)
                return new Vector3();

            int i;
            if (t >= 1.0f)
            {
                t = 1.0f;
                i = m_Points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * GetCurveCount();
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return transform.TransformPoint(Bezier.GetPoint(m_Points[i], m_Points[i + 1], m_Points[i + 2], m_Points[i + 3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (m_Points.Count < 4)
                return new Vector3();

            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = m_Points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * GetCurveCount();
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return transform.TransformPoint(Bezier.GetFirstDerivative(m_Points[i + 0], m_Points[i + 1], m_Points[i + 2], m_Points[i + 3], t)) - transform.position;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public int GetControlPointCount()
        {
            if (!m_IsInitialized)
                Initizalize();

            return m_Points.Count;
        }

        public int GetCurveCount()
        {
            return (GetControlPointCount() - 1) / 3;
        }

        public override bool GetLoop()
        {
            return m_Loop;
        }

        public override float GetTotalLength()
        {
            //STIJN: FIX TODO
            return 1.0f;
        }
    }
}