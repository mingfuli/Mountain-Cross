using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public delegate void ObjectDestoryedHandler(bool harmful);
    public event ObjectDestoryedHandler OnDestroyed;

    public int destoryTime = 2;
    public GameObject effect;
    public bool harmful;

    internal int fruitIndex;

    public void OnHit()
    {
        if (harmful)
        {
            SceneController.instance.playBomb();
        }
        else
        {
            SceneController.instance.playSlice();
            FruitSpriteData.Instance.CreatePartFruit(fruitIndex, transform);
        }

        // shake
        Handheld.Vibrate();
        if (OnDestroyed != null)
        {
            OnDestroyed(harmful);
        }

        Vector3 temp = transform.position;
        Destroy(gameObject);
        var instance = Instantiate(effect, temp, Quaternion.identity);
        Destroy(instance, 1);
    }
}
