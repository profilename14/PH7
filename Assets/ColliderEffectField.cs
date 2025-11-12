using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class ColliderEffectField : MonoBehaviour
{
    public Chemical effectType;

    public float damageOnEnter;

    public bool applyEffect;

    //public float damageOverTime;

    //public float damageTickInterval;

    public Vector2 staticKnockback;

    public float dynamicKnockback;

    public bool causeHit;

    public bool notifyTarget;

    public bool cancelTriggeringHits;

    public List<IHittable> doTEntities = new();

    public bool useHeightLimit;

    public float height;

    public bool generateSaltPlatforms;

    public bool triggerDebuff = false;

    public bool triggerReactions = false;

    public bool canHitPlayer;

    public bool disableInteractionsWithOtherEffectFields;

    public bool enableDamageOnEnemies;

    private void OnDisable()
    {
        doTEntities.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (!gameObject.activeInHierarchy) return;

        if (this.enabled == false) return;

        if (other.CompareTag("Hitbox")) return;

        int collLayer = other.gameObject.layer;
        if (collLayer == 17)
        {
            if (disableInteractionsWithOtherEffectFields) return;

            if (!generateSaltPlatforms)
            {
                ColliderEffectField effectField = other.gameObject.GetComponentInParentOrChildren<ColliderEffectField>();

                if (effectField is ColliderEffectField e && effectType != e.effectType && !effectField.generateSaltPlatforms)
                {
                    Debug.Log("Chemical reaction " + effectType + " with " + e.effectType);
                    ChemicalReactionManager.instance.DoReaction(effectType, e.effectType, (transform.position + e.transform.position) / 2);
                    this.transform.parent.gameObject.SetActive(false);
                }
            }
            else
            {
                ColliderEffectField effectField = other.gameObject.GetComponentInParentOrChildren<ColliderEffectField>();

                if (effectField is ColliderEffectField e && effectType != e.effectType)
                {
                    Debug.Log("Chemical reaction " + effectType + " with " + e.effectType);
                    effectField.transform.parent.gameObject.SetActive(false);
                    ChemicalReactionManager.instance.CreateSaltPlatform(effectField.transform.parent.transform.position);
                }
            }
        }
        else if ((canHitPlayer && other.CompareTag("Player")) ||  other.CompareTag("Enemy"))
        {
            // Should only need to get hittable if this is a new character to apply an effect to
            Debug.Log(other);
            if (cancelTriggeringHits) return;

            if (useHeightLimit && Mathf.Abs(transform.position.y - other.ClosestPointOnBounds(transform.position).y) > height) return;

            // Check if we have collided with a hittable object.
            IHittable hittableScript = other.gameObject.GetComponentInParentOrChildren<IHittable>();

            if (hittableScript is Character c)
            {
                ApplyEffectTo(c);
            }
        }
    }

    public void ApplyEffectTo(Character character)
    {
        //if (doTEntities.Contains(character)) return;

       //Debug.Log("Hit something" + character);

        //Debug.Log("Hittable: " + other.gameObject.name);

        if (causeHit && character != null)
        {
            if(character is Enemy && !enableDamageOnEnemies)
            {
                character.Hit(this, 0);
            }
            else
            {
                character.Hit(this, damageOnEnter);
            }

        }

        if (applyEffect && character != null)
        {
            //Debug.Log("Applying effect to " + character);
            //character.ApplyEffect()
        }

        doTEntities.Add(character);
    }
}
