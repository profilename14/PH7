using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class ChemicalBlob : MonoBehaviour
{
    [SerializeField]
    Animator anim;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    float fallSpeed;

    [SerializeField]
    float minDecaySpeed;

    [SerializeField]
    float maxDecaySpeed;

    [SerializeField]
    float decaySpeed;

    [SerializeField]
    bool isDecaying;

    [SerializeField]
    float puddleDecayShrinkMultiplier;

    [SerializeField]
    float puddleDecayShrinkMultiplierVertical;

    [SerializeField]
    ColliderEffectField effectField;

    public bool useHeightLimit;

    public float height;

    [SerializeField]
    float timeUntilCanFlatten;

    float flattenTimer;

    private void Awake()
    {
        
    }

    private void Start()
    {
        transform.eulerAngles = Vector3.zero;
        decaySpeed = Random.Range(minDecaySpeed, maxDecaySpeed);
        flattenTimer = timeUntilCanFlatten;
    }

    private void FixedUpdate()
    {
        if (fallSpeed > 0 && !isDecaying) transform.position -= new Vector3(0, fallSpeed, 0);

        if (flattenTimer > 0) flattenTimer -= Time.fixedDeltaTime;

        if (isDecaying && flattenTimer <= 0)
        {
            if(transform.localScale.x > 0 && transform.localScale.y > 0 && transform.localScale.z > 0)transform.localScale -= 
                    new Vector3(decaySpeed * Time.fixedDeltaTime * puddleDecayShrinkMultiplier, decaySpeed * Time.fixedDeltaTime * puddleDecayShrinkMultiplier * puddleDecayShrinkMultiplierVertical, 
                    decaySpeed * Time.fixedDeltaTime * puddleDecayShrinkMultiplier);
            else
            {
                isDecaying = false;
                this.gameObject.SetActive(false);
            }
            transform.position -= new Vector3(0, decaySpeed * Time.fixedDeltaTime, 0);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!gameObject.activeInHierarchy) return;

        // If this script is disabled, then the effect field is disabled
        if (this.enabled == false) return;

        if (other.CompareTag("Hitbox")) return;

        if (useHeightLimit && Mathf.Abs(transform.position.y - other.ClosestPointOnBounds(transform.position).y) > height) return;

        int collLayer = other.gameObject.layer;
        if(collLayer == 0 || collLayer == 10 && (flattenTimer <= 0 && rb.velocity.y >= 0))
        {
            anim.Play("BlobFlatten");
            rb.isKinematic = true;
            isDecaying = true;
        }
        else if(other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            IHittable hittableScript = other.gameObject.GetComponentInParentOrChildren<IHittable>();

            if (hittableScript != null)
            {
                Debug.Log("Applying effect on " + hittableScript);
                if (hittableScript is Character) effectField.ApplyEffectTo((Character)hittableScript); 
            }
        }
    }
}
