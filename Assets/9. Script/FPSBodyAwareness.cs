using UnityEngine;

public class FPSBodyAwareness : MonoBehaviour
{
    [Header("Body Setup")]
    public GameObject soldierModel;  // Din soldier-modell
    public Transform fpsCameraRig;   // Kameran

    [Header("Hide Head")]
    public string headBoneName = "Head";
    public string neckBoneName = "Neck";

    [Header("Body Position")]
    public Vector3 bodyOffset = new Vector3(0, -1.7f, 0);

    private SkinnedMeshRenderer[] bodyRenderers;
    private Transform headBone;

    void Start()
    {
        SetupBodyModel();
        HideHeadFromFirstPerson();
    }

    void SetupBodyModel()
    {
        if (soldierModel == null) return;

        // Placera kroppen under kameran
        soldierModel.transform.SetParent(transform);
        soldierModel.transform.localPosition = bodyOffset;
        soldierModel.transform.localRotation = Quaternion.identity;

        // Sätt kroppen på Viewmodel layer så den renderas av viewmodel-kameran
        SetLayerRecursively(soldierModel, LayerMask.NameToLayer("Viewmodel"));

        // Hitta alla mesh renderers
        bodyRenderers = soldierModel.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    void HideHeadFromFirstPerson()
    {
        // Hitta huvudet och gör det osynligt från första person
        Transform[] bones = soldierModel.GetComponentsInChildren<Transform>();

        foreach (Transform bone in bones)
        {
            if (bone.name.Contains(headBoneName) || bone.name.Contains(neckBoneName))
            {
                // Skala ner huvudet så det inte syns
                bone.localScale = Vector3.zero;
            }
        }
    }

    void Update()
    {
        // Kroppen följer spelarens rotation (bara Y-axeln)
        if (soldierModel != null)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            soldierModel.transform.rotation = Quaternion.Euler(0, eulerAngles.y, 0);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer == -1) return;

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}