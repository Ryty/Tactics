using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class RiflemanVariables
{
    public List<Ability> ClassAbilities = new List<Ability>();
    public int healthMax = 300;
    public int staminaMax = 150;
    public Sprite classSprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Sprites/class_rifleman.png", typeof(Sprite));

    public RiflemanVariables()
    {
        ClassAbilities.Add(new Shoot(80));
    }
}

public class AssaultVariables
{
    public int healthMax = 500;
}
