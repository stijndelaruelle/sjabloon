using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sjabloon
{
    public static class Line
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, float t)
        {
            return Vector3.Lerp(p0, p1, t);
        }

        public static float GetLength(Vector3 p0, Vector3 p1)
        {
            Vector3 diff = p0 - p1;
            return Vector3.Magnitude(diff);
        }
    }

    public class LineSpline : Spline
    {
        [SerializeField]
        private List<Vector3> m_Points;

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
        public void AddLine()
        {
            if (!m_IsInitialized)
                Initizalize();

            Vector3 lastPoint = new Vector3();

            if (m_Points.Count > 0)
            {
                lastPoint = m_Points[m_Points.Count - 1];
            }

            //Add points
            m_Points.Add(new Vector3(lastPoint.x + 1.0f, 0.0f, 0.0f));

            //First line: add extra stuff
            if (m_Points.Count == 1)
            {
                m_Points.Add(new Vector3(lastPoint.x + 2.0f, 0.0f, 0.0f));
            }

            if (m_Loop)
            {
                m_Points[m_Points.Count - 1] = m_Points[0];
            }
        }

        public void SetPoint(int index, Vector3 point)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return;

            m_Points[index] = point;
        }

        public void SetLoop(bool value)
        {
            if (!m_IsInitialized)
                Initizalize();

            m_Loop = value;
        }

        //Accessors
        public Vector3 GetControlPoint(int index)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (index < 0 || index >= m_Points.Count)
                return new Vector3();

            return m_Points[index];
        }

        public override Vector3 GetPoint(float t)
        {
            if (!m_IsInitialized)
                Initizalize();

            if (m_Points.Count < 2)
                return new Vector3();

            int i = 0;
            if (t >= 1.0f)
            {
                t = 1.0f;
                i = m_Points.Count - 2;
            }
            else
            {
                //Length dependant way
                t = Mathf.Clamp01(t);

                float cummulativePercent = 0.0f;
                float totalLength = GetTotalLength();
                for (int j = 0; j < GetLineCount(); ++j)
                {
                    float partLength = (Line.GetLength(m_Points[j], m_Points[j + 1]) / totalLength);
                    cummulativePercent += partLength;

                    if (t <= cummulativePercent)
                    {
                        i = j;
                        if (i > 0)
                        {
                            t -= (cummulativePercent - partLength);
                        }

                        t = (t / partLength);

                        break;
                    }
                }

                //Length independant way
                //t = Mathf.Clamp01(t) * GetLineCount();
                //i = (int)t;
                //t -= i;
            }

            return transform.TransformPoint(Line.GetPoint(m_Points[i], m_Points[i + 1], t));
        }

        public int GetPointCount()
        {
            if (!m_IsInitialized)
                Initizalize();

            return m_Points.Count;
        }

        public int GetLineCount()
        {
            return GetPointCount() - 1;
        }

        public override bool GetLoop()
        {
            return m_Loop;
        }

        public override float GetTotalLength()
        {
            float cummulativeLength = 0.0f;
            for (int i = 0; i < GetLineCount(); ++i)
            {
                cummulativeLength += Line.GetLength(m_Points[i], m_Points[i + 1]);
            }

            return cummulativeLength;
        }
    }
}