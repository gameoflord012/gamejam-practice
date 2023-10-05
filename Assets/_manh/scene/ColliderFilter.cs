using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderFilter : MonoBehaviour
{
    Collider2D col;
    HashSet<Collider2D> touchCols = new();

    [SerializeField] string[] filterTags;
    [SerializeField] LayerMask filterLayers;

    public List<Collider2D> GetTouchCols()
    {
        return touchCols.ToList();
    }

    private void Start()
    {
        col = GetComponent<Collider2D>();
    }

    bool CheckValid(Collider2D col)
    {
        if ((filterLayers.value & col.gameObject.layer) != 0) return false;

        foreach(string tag in filterTags)
        {
            if (col.CompareTag(tag)) return false;
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(CheckValid(collision))
        {
            touchCols.Add(collision);
        }

        //Debug.Log("In" + collision.gameObject + " " + touchCols.Count);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CheckValid(collision))
        {
            touchCols.Remove(collision);
        }

        //Debug.Log("Out" + collision.gameObject + " " + touchCols.Count);
    }
}
