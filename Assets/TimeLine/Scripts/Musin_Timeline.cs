using UnityEngine;

public class Musin_Timeline : MonoBehaviour
{
    public void AE_DashSound()
    {
        if (SoundManager.instance) SoundManager.instance.PlaySFX(SoundManager.SFX.Dash);
    }
}
