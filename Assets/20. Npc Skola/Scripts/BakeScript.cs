using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class BakeScript : MonoBehaviour
{
    public NavMeshSurface surface;
    public float rebuildDelay = 0.5f;
    public Rotate rotate;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            rotate.started = true;

            if (rotate.started)
            {
                StartCoroutine("NewBuild"); 
            }
        }
    }


    IEnumerator NewBuild()
    {
        surface.BuildNavMesh();
        yield return new WaitForSeconds(rebuildDelay);
        StartCoroutine("NewBuild");
    }
}
