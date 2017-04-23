using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TornadoDestroy : NetworkBehaviour {
    private Transform playerTransform;
    private Vector2 offset; 

    //void Update()
    //{
    //    if (playerTransform != null) {
    //        transform.position = (Vector2) playerTransform.position + offset;
    //    }
    //}

    //public void SetFollow(Transform target, Vector2 off)
    //{
    //    playerTransform = target;
    //    offset = off;

    //}
    public void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Contains("Fruit") || other.tag.Contains("Junk"))
        {
            Destroy(other);
        }
    }
}
