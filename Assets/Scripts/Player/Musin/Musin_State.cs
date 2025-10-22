using UnityEngine;

public class Musin_State : PlayerState
{
    [Header("=== Musin State ===")]
    [Header("swordModuel")]
    public bool swordSlash = false;

    [Header("granadeModuel")]
    public bool fireGranade = false;
    public bool impactGranade = false;
    public bool electricGranade = false;
}
