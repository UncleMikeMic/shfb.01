using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bumpers : MonoBehaviour {

    public GameManager gm;
    public int bumperPoints = 1;
    public int bumperSound = 0;
    public int bumperForce = 1;
    private Rigidbody rb;
    public bool hasAnim = false;
    private Animator anim;
    public string animName = "bump";
    public bool hasLight = false;
    private Light bumperLight;
    

    //spawn stuff
    public bool hasSpawn = false;
    public Transform prefabSpawnLocation;
    public GameObject spawnPrefab;
    public GameObject prefabSpawnParticles;

    [Header("shake scale")]
    //scale stuff 
    public bool shakeScale = false;
    public float shakeDuration = .5f;
    // The shake strength. Using a Vector3 instead of a float lets you choose the strength for each axis.
    public float shakeStrength = .25f;
    // Indicates how much will the shake vibrate.
    public int shakeVibrato = 2;
    //Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction. 
    public float shakeRandomness = 11f;
    //If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not.
    public bool shakeFadeOut = true;

    [Header("lerpScale")]
    public bool lerpScaleOn = true;
    Vector3 originalScale;
    private Vector3 newScale;
    public float scaleMultiplier = .75f;
    private bool repeatable;
    public float lerpSpeed = 10f;
    public float lerpDuration = .5f;

    void OnEnable()
    {
        originalScale = transform.localScale;
        newScale = originalScale * scaleMultiplier;
    }

    private void Start()
    {
        //connect to the game manager script and store as variable "gm"
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();

        //connect to the animator and store as variable "anim"
        if (hasAnim == true)
        {
            anim = GetComponent<Animator>();
        }

        //get light set to bumperLight
        if (hasLight == true)
        {
            bumperLight = GetComponent<Light>();
        }

  

   


        }
    void OnCollisionEnter(Collision collision)
    {                                           // --> Detect collision when bumper enter on collision with other objects
        ContactPoint[] tmpContact = collision.contacts;
        foreach (ContactPoint contact in tmpContact)
        {                               // if there is a collision : 
            Rigidbody rb = contact.otherCollider.GetComponent<Rigidbody>();             // Access rigidbody Component
            float t = collision.relativeVelocity.magnitude;                             // save the collision.relativeVelocity.magnitude value
           // rb.velocity = new Vector3(rb.velocity.x * .25f, rb.velocity.y * .25f, rb.velocity.z * .25f);        // reduce the velocity at the impact. Better feeling with the slingshot
            rb.AddForce(-1 * contact.normal * bumperForce, ForceMode.VelocityChange);           // Add Force
        }

        if (collision.gameObject.CompareTag("ball"))
        {
            //sfx
           AudioController.Play("bumper" + bumperSound);

            //bumper animation
            if (hasAnim == true)
            {
                anim.Play(animName);
            }

            //light toggle
            if (hasLight == true)
            {
                bumperLight.enabled = !bumperLight.enabled;
            }

            //Spawn prefab
            if (hasSpawn == true)
            {
                Instantiate(spawnPrefab, prefabSpawnLocation.position, prefabSpawnLocation.rotation);

                //particles
                Instantiate(prefabSpawnParticles, prefabSpawnLocation.position, prefabSpawnLocation.rotation);
            }

            if (shakeScale == true)
            {

                transform.DOShakeScale(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, shakeFadeOut);
               

            }

            if(lerpScaleOn == true)
            {
                StartCoroutine("LerpScale");
            }

            //raise score and coins in the game manager by set amount for this object
            gm.score += bumperPoints;
            gm.coins += bumperPoints;

            //updates scoreboard
            gm.IncrementScore();

         
        }
    }

    IEnumerator LerpScale()
    {
        repeatable = true;
        while (repeatable)
        {
            yield return RepeatLerp(originalScale, newScale, lerpDuration);
            yield return RepeatLerp(newScale, originalScale, lerpDuration);
            repeatable = false;
        }
    }

    public IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = (1.0f / time) * lerpSpeed;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
    }
  
}
