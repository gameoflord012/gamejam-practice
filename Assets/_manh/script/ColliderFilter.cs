using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ColliderFilter : MonoBehaviour
{
    Collider2D col;
    HashSet<Collider2D> touchCols = new();

    public UnityEvent<Collider2D> onTriggerEnter;
    public UnityEvent<Collider2D> onTriggerExit;

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
            if (col.CompareTag(tag)) return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(CheckValid(collision))
        {
            touchCols.Add(collision);
            onTriggerEnter.Invoke(collision);
        }

        //Debug.Log("In" + collision.gameObject + " " + touchCols.Count);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CheckValid(collision))
        {
            touchCols.Remove(collision);
            onTriggerExit.Invoke(collision);
        }

        //Debug.Log("Out" + collision.gameObject + " " + touchCols.Count);
    }
}
