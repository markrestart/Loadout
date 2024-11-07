using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player_Skin_Manager : NetworkBehaviour
{
    [SerializeField]
    private List<Material> skins = new List<Material>();
    [SerializeField]
    private List<Color> skinColors = new List<Color>();
    [SerializeField]
    private SkinnedMeshRenderer playerMesh;

    [Rpc(SendTo.Everyone)]
    public void SetSkinRpc(int skinIndex, int colorIndex)
    {
        while(skinIndex >= skins.Count){
            skinIndex -= skins.Count;
        }
        while(colorIndex >= skinColors.Count){
            colorIndex -= skinColors.Count;
        }
        var skin = new Material(skins[skinIndex]);
        skin.color = skinColors[colorIndex];
        playerMesh.material = skin;
    }
}
