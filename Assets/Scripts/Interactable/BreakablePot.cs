using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakablePot : MonoBehaviour, IHittable, ICanPickup
{
    
    [SerializeField] public GameObject splashEffectPrefab;
    [SerializeField] public GameObject puddlePrefab;
    [SerializeField] private UnityEvent onShatter;

    private Rigidbody rigid;
    private bool pickedUp = false;
    private FloatingBubble pickuper;
    private float shatterVelocity = 2; // Speed at which the bubble must be moving for the pot to break if it collides with an enemy


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (pickuper == null && pickedUp) // Bubble disappears
        {
            pickedUp = false;
        }

        if (pickedUp)
        {
            rigid.transform.position = pickuper.transform.position;
        }
    }

    public void Shattered()
    {
        onShatter.Invoke();
        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);
        }
        if (puddlePrefab != null)
        {
            Instantiate(puddlePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    
    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (!pickedUp) Shattered();
    }
    
    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        if (!pickedUp) Shattered();
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }

    public void Pickup(FloatingBubble bubble)
    {
        pickedUp = true;
        pickuper = bubble;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.layer == 10 || collision.gameObject.layer == 18)
        {
            if (pickedUp && pickuper.getCurSpeed().magnitude > shatterVelocity)
            {
                pickuper.Pop();
                Shattered();
            }
        }
    }
}
