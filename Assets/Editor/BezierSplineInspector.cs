using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sjabloon
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInspector : Editor
    {
        private static Color[] c_ModeColors = { Color.white, Color.yellow, Color.cyan };

        private const int c_LineStepsPerCurve = 10;
        private const float c_DirectionScale = 0.5f;
        private const float c_HandleSize = 0.04f;
        private const float c_PickSize = 0.06f;

        private BezierSpline m_Spline;
        private Transform m_HandleTransform;
        private Quaternion m_HandleRotation;

        private int m_SelectedIndex = -1;

        //Inspector
        public override void OnInspectorGUI()
        {
            //Loop checkbox
            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", m_Spline.GetLoop());
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Toggle Loop");
                EditorUtility.SetDirty(m_Spline);
                m_Spline.SetLoop(loop);
            }

            //Add curve button
            if (m_SelectedIndex >= 0 && m_SelectedIndex < m_Spline.GetControlPointCount())
            {
                DrawSelectedPointInspector();
            }

            m_Spline = target as BezierSpline;
            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(m_Spline, "Add Curve"); //Makes sure we can undo
                EditorUtility.SetDirty(m_Spline);         //Makes sure unity asks us to save after changing
                m_Spline.AddCurve();
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");

            //Position
            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", m_Spline.GetControlPoint(m_SelectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Move Point");
                EditorUtility.SetDirty(m_Spline);
                m_Spline.SetControlPoint(m_SelectedIndex, point);
            }

            //Edit mode
            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", m_Spline.GetControlPointMode(m_SelectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Change Point Mode");
                EditorUtility.SetDirty(m_Spline);
                m_Spline.SetControlPointMode(m_SelectedIndex, mode);
            }
        }

        //Scene
        private void OnSceneGUI()
        {
            m_Spline = target as BezierSpline;
            m_HandleTransform = m_Spline.transform;
            m_HandleRotation = Tools.pivotRotation == PivotRotation.Local ? m_HandleTransform.rotation : Quaternion.identity;

            //Show points
            Vector3 p0 = ShowPoint(0);

            for (int i = 1; i < m_Spline.GetControlPointCount(); i += 3)
            {
                Vector3 p1 = ShowPoint(i);
                Vector3 p2 = ShowPoint(i + 1);
                Vector3 p3 = ShowPoint(i + 2);

                //Draw the handle lines
                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1); //p1 helps p0
                Handles.DrawLine(p2, p3); //p2 helps p3

                //Draw the bezier lines
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 3.0f);
                p0 = p3;
            }

            //Show the directions
            //ShowDirections();
        }

        private void ShowDirections()
        {
            Handles.color = Color.green;
            Vector3 point = m_Spline.GetPoint(0f);

            Handles.DrawLine(point, point + m_Spline.GetDirection(0f) * c_DirectionScale);

            int steps = c_LineStepsPerCurve * m_Spline.GetCurveCount();
            for (int i = 1; i <= steps; i++)
            {
                point = m_Spline.GetPoint(i / (float)steps);
                Handles.DrawLine(point, point + m_Spline.GetDirection(i / (float)steps) * c_DirectionScale);
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = m_HandleTransform.TransformPoint(m_Spline.GetControlPoint(index));

            Handles.color = c_ModeColors[(int)m_Spline.GetControlPointMode(index)];

            //Show buttons when this point isn't selected
            float size = HandleUtility.GetHandleSize(point); //Makes sure they are always the same scale (independant of the camera)
            if (index == 0)
            {
                size *= 2.0f;
            }

            if (Handles.Button(point, m_HandleRotation, size * c_HandleSize, size * c_PickSize, Handles.DotCap))
            {
                m_SelectedIndex = index;
                Repaint(); //Makes sure the inspector updates
            }

            //Show a fully fledged handle when we're selected
            if (m_SelectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();

                point = Handles.DoPositionHandle(point, m_HandleRotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_Spline, "Move Point");   //Makes sure we can undo
                    EditorUtility.SetDirty(m_Spline);            //Makes sure unity asks us to save after changing
                    m_Spline.SetControlPoint(index, m_HandleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
}