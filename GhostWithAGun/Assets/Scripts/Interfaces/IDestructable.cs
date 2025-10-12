using UnityEngine;

public interface IDestructable
{
    public void TakeDamage(float amount);
    public void Shatter();
}
