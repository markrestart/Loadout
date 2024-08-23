using Unity.Netcode;
using UnityEngine;
[CreateAssetMenu(fileName = "Ability", menuName = "Ability", order = 0)]
public class SO_Ability : ScriptableObject {
    public string abilityName = "Ability";
    public string description = "This is a description of the ability.";
    public bool isContinuous = false;
    public float cooldown = 1.0f;
    private float lastActivationTime = 0.0f;
    private bool isActive = false;
    public bool IsActive { get => isActive; }
}