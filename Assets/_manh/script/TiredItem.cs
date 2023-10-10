using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiredItem : MonoBehaviour
{
    [SerializeField][Range(0f, 5f)] float tiredAmount = .1f;

    [SerializeField] ColliderFilter colliderFilter;

    private void OnEnable()
    {
        colliderFilter.onTriggerEnter.AddListener(OnPlayerContact);
    }

    private void OnDisable()
    {
        colliderFilter.onTriggerEnter.RemoveListener(OnPlayerContact);

    }

    void OnPlayerContact(Collider2D collider)
    {
        var player = collider.attachedRigidbody.GetComponent<Player>();
        player.AddToTireness(tiredAmount);
        gameObject.SetActive(false);
    }
}
