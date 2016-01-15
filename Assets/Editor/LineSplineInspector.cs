using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sjabloon
{
    [CustomEditor(typeof(LineSpline))]
    public class LineSplineInspector : Editor
    {
        private const int c_LineStepsPerCurve = 10;
        private const float c_DirectionScale = 0.5f;
        private const float c_HandleSize = 0.04f;
        private const float c_PickSize = 0.06f;

        private LineSpline m_Spline;
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
            if (m_SelectedIndex >= 0 && m_SelectedIndex < m_Spline.GetPointCount())
            {
                DrawSelectedPointInspector();
            }

            m_Spline = target as LineSpline;
            if (GUILayout.Button("Add Line"))
            {
                Undo.RecordObject(m_Spline, "Add Line"); //Makes sure we can undo
                EditorUtility.SetDirty(m_Spline);         //Makes sure unity asks us to save after changing
                m_Spline.AddLine();
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
                m_Spline.SetPoint(m_SelectedIndex, point);
            }
        }

        //Scene
        private void OnSceneGUI()
        {
            m_Spline = target as LineSpline;
            m_HandleTransform = m_Spline.transform;
            m_HandleRotation = Tools.pivotRotation == PivotRotation.Local ? m_HandleTransform.rotation : Quaternion.identity;

            //Show points
            Vector3 p0 = ShowPoint(0);

            for (int i = 1; i < m_Spline.GetPointCount(); ++i)
            {
                Vector3 p1 = ShowPoint(i);

                //Draw the handle lines
                Handles.color = Color.white;
                Handles.DrawLine(p0, p1); //p1 helps p0

                p0 = p1;
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = m_HandleTransform.TransformPoint(m_Spline.GetControlPoint(index));
            Handles.color = Color.yellow;

            //Show buttons when this point isn't selected
            float size = HandleUtility.GetHandleSize(point); //Makes sure they are always the same scale (independant of the camera)

            size *= 2.0f;
            if (index == 0)
                size *= 2.0f;

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
                    m_Spline.SetPoint(index, m_HandleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
}