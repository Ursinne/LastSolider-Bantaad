using UnityEngine;

public class SkillStamina
{
    public string skillStamina;
    public float level;
    public float modifier;

    public SkillStamina(string name, float lvl, float mod)
    {
        skillStamina = name;
        level = lvl;
        modifier = mod;
    }
}
