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
    public float playerX, playerY;
    public int mana;
    public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
}
