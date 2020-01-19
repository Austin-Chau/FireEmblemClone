//A struct to be used while initializing a unit containing the stats needed to init
public struct UnitStats
{
    public int Strength;
    public int MaxHealth;
    public int CurrentHealth;
    public int Defence;
    public int Movement;

    public UnitStats(int strength, int maxHealth, int currentHealth, int defence, int movement)
    {
        Strength = strength;
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
        Defence = defence;
        Movement = movement;
    }

    public UnitStats(int strength, int maxHealth, int defence, int movement)
    {
        Strength = strength;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Defence = defence;
        Movement = movement;
    }
}
