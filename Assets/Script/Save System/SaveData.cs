using System;
using System.Collections.Generic;

[Serializable]
public class PlacedObjectData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotZ;
    public float scaleX, scaleY, scaleZ;
}

[Serializable]
public class SaveData
{
    public string sceneName;
    public string lastManaBlockID;
    public float playerX, playerY;
    public int mana;
    public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
    public List<string> triggeredManaBlocks = new List<string>();
    public int maxMana;
    public int selectionLimit;
    public int gold;
}
