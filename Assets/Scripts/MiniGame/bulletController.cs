using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels � la m�thode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("Background"))
        {
            Destroy(this.gameObject);
        }

    }
}
