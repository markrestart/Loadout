using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Skin_Manager : MonoBehaviour
{
    [SerializeField]
    private List<Material> skins = new List<Material>();
    [SerializeField]
    private List<Color> skinColors = new List<Color>();
    [SerializeField]
    private SkinnedMeshRenderer playerMesh;

    public void SetSkin(int skinIndex, int colorIndex)
    {
        while(skinIndex >= skins.Count){
            skinIndex -= skins.Count;
        }
        while(colorIndex >= skinColors.Count){
            colorIndex -= skinColors.Count;
        }
        var skin = new Material(skins[Random.Range(0, skins.Count)]);
        skin.color = skinColors[Random.Range(0, skinColors.Count)];
        playerMesh.material = skin;
    }
}
