using UnityEngine;

public abstract class Ability : ScriptableObject {
    public string abilityName = "Ability";
    public string description = "This is a description of the ability.";

    public virtual void Activate() {
        Debug.Log("Activating " + abilityName);
    }

    public virtual void Deactivate() {
        Debug.Log("Deactivating " + abilityName);
    }

    public virtual void Update() {
        Debug.Log("Updating " + abilityName);
    }

    public bool isContinuous = false;

    public float cooldown = 1.0f;

    private float lastActivationTime = 0.0f;
    private bool isActive = false;
    public bool IsActive { get => isActive; }
}