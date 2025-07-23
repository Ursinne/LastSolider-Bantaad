using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360,
        fow.viewRadius);
        Vector3 viewAngleA = DirectionFromAngle(fow.transform.eulerAngles.y,
        -fow.viewAngle / 2);
        Vector3 viewAngleB = DirectionFromAngle(fow.transform.eulerAngles.y,
        fow.viewAngle / 2);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA *
        fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB *
        fow.viewRadius);
        Handles.color = Color.red;
        foreach (Collider visibleTarget in fow.objectsInView)
        {
            if (fow.canSeePlayer)
            {
                Handles.DrawLine(fow.transform.position,
                visibleTarget.gameObject.transform.position);
            }
        }
    }
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0,
Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}