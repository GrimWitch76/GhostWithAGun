using UnityEngine;
using UnityEngine.Events;

public enum BodyPart
{
    Head,
    Torso,
    Arm,
    Leg
}

public class PlayerHealth : MonoBehaviour
{
    [Header("General Health")]
    [SerializeField] private float maxTorsoHealth = 100f;
    [SerializeField] private float limbHealth = 25f;
    [SerializeField] private float bleedDamagePerSecond = 2f;
    [SerializeField] private ScreenDamageIndicator _screenDamage;

    [Header("Events")]
    public UnityEvent onPlayerDamaged;
    public UnityEvent onPlayerDied;

    public float Health => torsoHealth;
    public bool Dead => dead;
    // State
    private float torsoHealth;
    private int armHits = 0;
    private int legHits = 0;
    private bool bleeding = false;
    private bool dead = false;

    private void Awake()
    {
        torsoHealth = maxTorsoHealth;
    }

    public void ApplyDamage(BodyPart part, float amount)
    {
        if (dead) return;

        switch (part)
        {
            case BodyPart.Head:
                return;

            case BodyPart.Torso:
                torsoHealth -= amount;
                _screenDamage.UpdateBaseHealth(torsoHealth / maxTorsoHealth);
                break;

            case BodyPart.Arm:
                armHits++;
                torsoHealth -= amount;
                _screenDamage.UpdateBaseHealth(torsoHealth / maxTorsoHealth);
                _screenDamage.DamageLimb(part);
                if (armHits == 1) Debug.Log("One arm injured: can't throw.");
                if (armHits >= 2) Debug.Log("Both arms injured: can't pick up items.");
                break;

            case BodyPart.Leg:
                legHits++;
                torsoHealth -= amount;
                _screenDamage.UpdateBaseHealth(torsoHealth / maxTorsoHealth);
                _screenDamage.DamageLimb(part);
                if (legHits == 1) Debug.Log("One leg injured: can't sprint.");
                if (legHits >= 2) Debug.Log("Both legs crippled: crawl speed only.");

                break;
        }


        if (torsoHealth <= 0) Die();
        onPlayerDamaged?.Invoke();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        onPlayerDied?.Invoke();
        Debug.LogError("Player died!");
    }

    // Accessors for other scripts (movement, interaction, etc.)
    public bool CanThrow => armHits < 1;
    public bool CanPickUp => armHits < 2;
    public bool CanSprint => legHits < 1;
    public bool Crippled => legHits >= 2;
    public bool IsBleeding => bleeding;
}
