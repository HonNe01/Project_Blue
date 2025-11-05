using UnityEngine;

public class Musin_Timeline : MonoBehaviour
{
    public void AE_DashSound()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.Dash);
    }
}
